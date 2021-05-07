#!/bin/bash

mona_version="0.1-prerelease"

exec 3>&2 # Grabbing a reliable stderr handle...

usage() { printf "\nUsage: $0 <-n deployment-name> <-r deployment-region> [-d display-name] [-g resource-group] [-l ui-language] [-s subscription-id]\n"; }

check_az() {
    exec 3>&2

    az version >/dev/null 2>&1

    # TODO: Should we be more specific about which version of az is required?

    if [[ $? -ne 0 ]]; then 
        printf "\nPlease install the Azure CLI before continuing. See [https://docs.microsoft.com/cli/azure/install-azure-cli] for more information.\n" >&3
        return 1
    fi
}

check_dotnet() {
    exec 3>&2

    dotnet --version >/dev/null 2>&1

   # TODO: Should we be more specific about which version of dotnet is required?

    if [[ $? -ne 0 ]]; then 
        printf "\nPlease install .NET before continuing. See [https://dotnet.microsoft.com/download] for more information.\n" >&3
        return 1
    fi
}

check_prereqs() {
    printf "Checking Mona setup prerequisites..."; echo

    check_az;         if [[ $? -ne 0 ]]; then prereq_check_failed=1; fi; 
    check_dotnet;     if [[ $? -ne 0 ]]; then prereq_check_failed=1; fi;

    if [[ -z $prereq_check_failed ]]; then
        printf "\nAll Mona setup prerequisites installed.\n"
    else
        return 1
    fi
}

build_mona() {
    mona_sln_path="../Mona.SaaS.sln"

    printf "\nBuilding Mona...\n\n";

    dotnet build "$mona_sln_path"

    if [[ $? -ne 0 ]]; then
        printf "\nMona build has failed. Please correct issues then try again.\n" >&3
        exit 1
    fi

    printf "\nRunning Mona tests...\n\n"

    dotnet test "$mona_sln_path"

    if [[ $? -ne 0 ]]; then
        printf "\nOne or more Mona tests has failed. Please correct issues then try again.\n" >&3
        exit 1
    fi
}

language="en" # Default UI language is English ("en"). Can be overridden using [-l] flag below.

while getopts "d:g:l:n:r:s:" opt; do
    case $opt in
        d)
            display_name=$OPTARG
        ;;
        g)
            resource_group_name=$OPTARG
        ;;
        l)
            language=$OPTARG
        ;;
        n)
            deployment_name=$OPTARG
        ;;
        r)
            deployment_region=$OPTARG
        ;;
        s)
            subscription_id=$OPTARG
        ;;
        \?)
            usage
            exit 1
        ;;
    esac
done

# Validate parameters.

[[ -z $deployment_name || -z $deployment_region ]] && { usage; exit 1; }
[[ -z $resource_group_name ]] && resource_group_name="mona-$deployment_name";
[[ -z $display_name ]] && display_name="$deployment_name";

# Check setup pre-reqs.

check_prereqs

if [[ $? -ne 0 ]]; then
    printf "\nPlease install all Mona setup prerequisites then try again.\n" >&3
    exit 1
fi

# Build/test Mona locally.

build_mona

if [[ $? -ne 0 ]]; then exit 1; fi; # Mona build has failed.

# Ensure user is logged in to Azure.
# Get current user object ID (oid).

while [[ -z $current_user_oid ]]; do 
    current_user_oid=$(az ad signed-in-user show --query objectId --output tsv 2>/dev/null);
    if [[ -z $current_user_oid ]]; then az login; fi;
done

# Make sure that we're pointing at the right subscription.

if [[ -n $subscription_id ]]; then
    az account set --subscription $subscription_id
fi

# Create the resource group if it doesn't already exist.
# If it already exists confirm that it's empty.

if [[ $(az group exists --resource-group "$resource_group_name" --output tsv) -eq false ]]; then
    printf "\nCreating resource group [$resource_group_name]...\n"
    az group create --location "$deployment_region" --name "$resource_group_name" >/dev/null
elif [[ -n $(az resource list --resource-group "$resource_group_name" --output tsv) ]]; then
    printf "\nMona must be deployed into an empty resource group. Resource group [$resource_group_name] contains resources. Setup failed.\n" >2
    exit 1
fi

subscription_id=$(az account show --query id --output tsv);
current_user_tid=$(az account show --query tenantId --output tsv);

# Create the app registration in AAD.

aad_app_name="$display_name"
aad_app_secret=$(openssl rand -base64 64)

printf "\nCreating Azure Active Directory (AAD) app registration [$aad_app_name]...\n"

aad_app_id=$(az ad app create \
    --display-name "$aad_app_name" \
    --available-to-other-tenants true \
    --end-date "2299-12-31" \
    --password "$aad_app_secret" \
    --optional-claims @./aad/manifest.optional-claims.json \
    --required-resource-accesses @./aad/manifest.resource-access.json \
    --query appId \
    --output tsv);

printf "\nAAD app [$aad_app_name ($aad_app_id)] successfully registered with AAD tenant [$current_user_tid].\n"
printf "\nCreating app service principal. This might take a while...\n"

sleep 30 # Give AAD a chance to catch up...

aad_sp_id=$(az ad sp create --id "$aad_app_id" --query objectId --output tsv 2>/dev/null);

printf "\nGranting AAD app [$aad_app_name] service principal [$aad_sp_id] contributor access to resource group [$resource_group_name]...\n\n"

sleep 30 # Give AAD a chance to catch up...

az role assignment create \
    --role "Contributor" \
    --assignee "$aad_sp_id" \
    --resource-group "$resource_group_name"

printf "\nTagging resource group [$resource_group_name]...\n"

az group update \
    --name "$resource_group_name" \
    --tags \
        "Mona Version"="$mona_version" \
        "Deployment Name"="$deployment_name" \
        "AAD App ID"="$aad_app_id" \
        "AAD App Name"="$aad_app_name" >/dev/null

# Deploy the ARM template.

printf "\nDeploying Mona to subscription [$subscription_id] resource group [$resource_group_name]. This might take a while...\n";

az_deployment_name="mona-deploy-$deployment_name"

az group deployment create \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --template-file "./arm-templates/basic-deploy.json" \
    --parameters \
        deploymentName="$deployment_name" \
        aadTenantId="$current_user_tid" \
        aadClientId="$aad_app_id" \
        aadClientSecret="$aad_app_secret" \
        language="$language"; echo;
        
# Get ARM deployment output variables.

blob_conn_str=$(az deployment group show --resource-group "$resource_group_name" --name "$az_deployment_name" --query properties.outputs.storageConnectionString.value --output tsv);
blob_sub_container_name=$(az deployment group show --resource-group "$resource_group_name" --name "$az_deployment_name" --query properties.outputs.subscriptionStorageContainerName.value --output tsv);
event_grid_id=$(az deployment group show --resource-group "$resource_group_name" --name "$az_deployment_name" --query properties.outputs.eventGridTopicId.value --output tsv);
event_grid_topic_endpoint=$(az deployment group show --resource-group "$resource_group_name" --name "$az_deployment_name" --query properties.outputs.eventGridTopicEndpoint.value --output tsv);
event_grid_topic_key=$(az deployment group show --resource-group "$resource_group_name" --name "$az_deployment_name" --query properties.outputs.eventGridTopicKey.value --output tsv);
app_config_connection_string=$(az deployment group show --resource-group "$resource_group_name" --name "$az_deployment_name" --query properties.outputs.appConfigServiceConnectionString.value --output tsv);
app_config_name=$(az deployment group show --resource-group "$resource_group_name" --name "$az_deployment_name" --query properties.outputs.appConfigServiceName.value --output tsv);
web_app_base_url=$(az deployment group show --resource-group "$resource_group_name" --name "$az_deployment_name" --query properties.outputs.webAppBaseUrl.value --output tsv);
web_app_name=$(az deployment group show --resource-group "$resource_group_name" --name "$az_deployment_name" --query properties.outputs.webAppName.value --output tsv);
app_insights_conn_str=$(az deployment group show --resource-group "$resource_group_name" --name "$az_deployment_name" --query properties.outputs.appInsightsConnectionString.value --output tsv);

# Configure Mona.

printf "\nConfiguring Mona settings...\n\n";

az appconfig kv set --name "$app_config_name" --key "Deployment:MonaVersion" --yes                                      --value "$mona_version"; echo;
az appconfig kv set --name "$app_config_name" --key "Deployment:AppInsightsConnectionString" --yes                      --value "$app_insights_conn_str"; echo;
az appconfig kv set --name "$app_config_name" --key "Deployment:AzureResourceGroupName" --yes                           --value "$resource_group_name"; echo;
az appconfig kv set --name "$app_config_name" --key "Deployment:AzureSubscriptionId" --yes                              --value "$subscription_id"; echo;
az appconfig kv set --name "$app_config_name" --key "Deployment:IsTestModeEnabled" --yes                                --value "true"; echo;
az appconfig kv set --name "$app_config_name" --key "Deployment:Name" --yes                                             --value "$deployment_name"; echo;
az appconfig kv set --name "$app_config_name" --key "Identity:AdminIdentity:AadTenantId" --yes                          --value "$current_user_tid"; echo;
az appconfig kv set --name "$app_config_name" --key "Identity:AdminIdentity:AadUserId" --yes                            --value "$current_user_oid"; echo;
az appconfig kv set --name "$app_config_name" --key "Identity:AppIdentity:AadClientId" --yes                            --value "$aad_app_id"; echo;
az appconfig kv set --name "$app_config_name" --key "Identity:AppIdentity:AadClientSecret" --yes                        --value "$aad_app_secret" >/dev/null # Sensitive
az appconfig kv set --name "$app_config_name" --key "Identity:AppIdentity:AadTenantId" --yes                            --value "$current_user_tid"; echo;
az appconfig kv set --name "$app_config_name" --key "Offer:IsSetupComplete" --yes                                       --value "false"; echo;
az appconfig kv set --name "$app_config_name" --key "Subscriptions:Events:EventGrid:TopicEndpoint" --yes                --value "$event_grid_topic_endpoint"; echo; 
az appconfig kv set --name "$app_config_name" --key "Subscriptions:Events:EventGrid:TopicKey" --yes                     --value "$event_grid_topic_key" >/dev/null # Sensitive
az appconfig kv set --name "$app_config_name" --key "Subscriptions:Repository:BlobStorage:ConnectionString" --yes       --value "$blob_conn_str" >/dev/null # Sensitive
az appconfig kv set --name "$app_config_name" --key "Subscriptions:Repository:BlobStorage:ContainerName" --yes          --value "$blob_sub_container_name"; echo;

# Configure AD app reply and ID URLs.

printf "Completing Mona configuration...\n"

az ad app update \
    --id "$aad_app_id" \
    --reply-urls "$web_app_base_url/signin-oidc";

# Set web app configuration service connection string.

az webapp config appsettings set \
    --resource-group "$resource_group_name" \
    --name "$web_app_name" \
    --settings APP_CONFIG_SERVICE_CONNECTION_STRING="$app_config_connection_string" >/dev/null # Sensitive

# Deploy Mona web application...

printf "\nPackaging Mona web app for deployment to [$web_app_name]...\n\n"

dotnet publish -c Release -o ./topublish ../Mona.SaaS.Web/Mona.SaaS.Web.csproj

printf "\nZipping Mona web app deployment package...\n"

cd ./topublish
zip -r ../topublish.zip . >/dev/null
cd ..

printf "\nDeploying Mona web app to [$web_app_name]...\n"

az webapp deployment source config-zip -g "$resource_group_name" -n "$web_app_name" --src ./topublish.zip

# And scene...

printf "Cleaning up...\n"

rm -rf ./topublish >/dev/null
rm -rf ./topublish.zip >/dev/null

printf "\nMona Deployment Summary\n"
printf "==============================\n"
printf "Deployment Name                     [$deployment_name]\n"
printf "Deployment Version                  [$mona_version]\n"
printf "Deployed to Azure Subscription      [$subscription_id]\n"
printf "Deployed to Resource Group          [$resource_group_name]\n"
printf "Deployment AAD Client ID            [$aad_app_id]\n"
printf "Deployment AAD Tenant ID            [$current_user_tid]\n"
printf "Landing Page URL                    [$web_app_base_url/]\n"
printf "Landing Page URL (Testing)          [$web_app_base_url/test]\n"
printf "Webhook URL                         [$web_app_base_url/webhook]\n"
printf "Webhook URL (Testing)               [$web_app_base_url/webhook/test]\n"
printf "Admin Center URL                    [$web_app_base_url/admin]\n"

printf "\nMona deployment complete.\n"
printf "\n==> Please visit [ $web_app_base_url/setup ] to complete your setup and begin transacting with Microsoft!\n"
























