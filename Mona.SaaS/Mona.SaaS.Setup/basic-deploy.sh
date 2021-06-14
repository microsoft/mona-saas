#!/bin/bash

mona_version="0.1-prerelease"

exec 3>&2 # Grabbing a reliable stderr handle...

usage() { printf "\nUsage: $0 <-n deployment-name> <-r deployment-region> [-a app-service-plan-id] [-d display-name] [-g resource-group] [-l ui-language] [-s subscription-id] [-h] [-p]\n"; }

check_az() {
    exec 3>&2

    az version >/dev/null 2>&1

    lp=$1

    # TODO: Should we be more specific about which version of az is required?

    if [[ $? -ne 0 ]]; then 
        echo "$lp ❌   Please install the Azure CLI before continuing. See [https://docs.microsoft.com/cli/azure/install-azure-cli] for more information."
        return 1
    else
        echo "$lp ✔   Azure CLI installed."
    fi
}

check_dotnet() {
    exec 3>&2

    dotnet --version >/dev/null 2>&1

    lp=$1

   # TODO: Should we be more specific about which version of dotnet is required?

    if [[ $? -ne 0 ]]; then 
        echo "$lp ❌   Please install .NET before continuing. See [https://dotnet.microsoft.com/download] for more information."
        return 1
    else
        echo "$lp ✔   .NET installed."
    fi
}

check_prereqs() {
    lp=$1

    echo "$lp Checking Mona setup prerequisites...";

    check_az "$lp";         if [[ $? -ne 0 ]]; then prereq_check_failed=1; fi; 
    check_dotnet "$lp";     if [[ $? -ne 0 ]]; then prereq_check_failed=1; fi;

    if [[ -z $prereq_check_failed ]]; then
        echo "$lp ✔   All Mona setup prerequisites installed."
    else
        return 1
    fi
}

check_app_service_plan() {
    lp=$1
    plan_id=$2

    plan_name=$(az appservice plan show --ids "$plan_id" --output "tsv" --query "name");

    if [[ -z $plan_name ]]; then
        echo "$lp ❌   App service plan [$plan_id] not found."
        return 1
    else
        echo "$lp ✔   Will deploy Mona web app to app service plan [$plan_name]."
    fi
}

check_deployment_region() {
    lp=$1
    region=$2

    region_display_name=$(az account list-locations -o tsv --query "[?name=='$region'].displayName")

    if [[ -z $region_display_name ]]; then
        echo "$lp ❌   [$region] is not a valid Azure region. For a full list of Azure regions, run 'az account list-locations -o table'."
        return 1
    else
        echo "$lp ✔   [$region] is a valid Azure region ($region_display_name)."
    fi
}

check_language() {
    lp=$1
    language=$2
    supported_languages=("en" "es")

    if [[ " ${supported_languages[*]} " == *"$language"* ]]; then
        echo "$lp ✔   [$language] language is supported."
    else
        echo "$lp ❌   [$language] language is not supported."
        return 1
    fi
}

check_deployment_name() {
    name=$1

    if [[ $name =~ ^[a-z0-9]{5,13}$ ]]; then
        echo "✔   [$name] is a valid Mona deployment name."
    else
        echo "❌   [$name] is not a valid Mona deployment name. The name must contain only lowercase letters and numbers and be between 5 and 13 characters in length."
        return 1
    fi
}

language="en" # Default UI language is English ("en"). Can be overridden using [-l] flag below.

while getopts "a:d:g:l:n:r:s:hp" opt; do
    case $opt in
        a)
            app_service_plan_id=$OPTARG
        ;;
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
        h)
            no_splash=1
        ;;
        p)
            no_publish=1
        ;;
        \?)
            usage
            exit 1
        ;;
    esac
done

# Check for missing parameters.
# Set default resource group name and deployment display name.

[[ -z $deployment_name || -z $deployment_region ]] && { usage; exit 1; }

check_deployment_name "$deployment_name"

[[ $? -ne 0 ]] && exit 1;

[[ -z $resource_group_name ]] && resource_group_name="mona-$deployment_name";
[[ -z $display_name ]] && display_name="$deployment_name";

lp=$(printf '[%s%*s]>' "$deployment_name" "$((13-${#deployment_name}))" "");

echo "$lp Setting up Mona SaaS in your Azure environment...";

# Show Mona setup splash screen...

[[ -z $no_splash ]] && cat ./splash.txt && echo;

# Check setup pre-reqs.

check_prereqs "$lp"

if [[ $? -ne 0 ]]; then
    echo "$lp ❌   Please install all Mona setup prerequisites then try again. Setup failed."
    exit 1
fi

# Check parameter values.

check_deployment_region "$lp" "$deployment_region"; if [[ $? -ne 0 ]]; then param_valid_failed=1; fi;
check_language "$lp" "$language"; if [[ $? -ne 0 ]]; then param_valid_failed=1; fi;

if [[ -n $app_service_plan_id ]]; then
    check_app_service_plan "$lp" "$app_service_plan_id"
    if [[ $? -ne 0 ]]; then exit 1; fi;
fi

if [[ -n $param_valid_failed ]]; then 
    echo "$lp ❌   Parameter validation failed. Please review then try again. Setup failed."
    exit 1
fi

# Ensure user is logged in to Azure.
# Get current user object ID (oid).

while [[ -z $current_user_oid ]]; do 
    current_user_oid=$(az ad signed-in-user show --query objectId --output tsv 2>/dev/null);
    if [[ -z $current_user_oid ]]; then az login; fi;
done

# Make sure that we're pointing at the right subscription.

if [[ -n $subscription_id ]]; then
    az account set --subscription $subscription_id
    [[ $? -ne 0 ]] && echo "$lp ❌   Azure subscription [$subscription_id] not found. Setup failed" && exit 1;
fi

# Create the resource group if it doesn't already exist.
# If it already exists confirm that it's empty.

if [[ $(az group exists --resource-group "$resource_group_name") -eq false ]]; then
    echo "$lp Creating resource group [$resource_group_name]..."
    az group create --location "$deployment_region" --name "$resource_group_name"

    if [[ $? -eq 0 ]]; then 
        echo "$lp ✔   Resource group [$resource_group_name] created."
    else
        echo "$lp ❌   Unable to create resource group [$resource_group_name]. See above output for details. Setup failed."
        exit 1
    fi
elif [[ -n $(az resource list --resource-group "$resource_group_name" --output tsv) ]]; then
    echo "$lp ❌   Mona must be deployed into an empty resource group. Resource group [$resource_group_name] contains resources. Setup failed."
    exit 1
fi

subscription_id=$(az account show --query id --output tsv);
current_user_tid=$(az account show --query tenantId --output tsv);

# Create the app registration in AAD.

aad_app_name="$display_name"
aad_app_secret=$(openssl rand -base64 64)

echo "$lp Creating Azure Active Directory (AAD) app registration [$aad_app_name]..."

aad_app_id=$(az ad app create \
    --display-name "$aad_app_name" \
    --available-to-other-tenants true \
    --end-date "2299-12-31" \
    --password "$aad_app_secret" \
    --optional-claims @./aad/manifest.optional-claims.json \
    --required-resource-accesses @./aad/manifest.resource-access.json \
    --query appId \
    --output tsv);

echo "$lp ✔   AAD app [$aad_app_name ($aad_app_id)] successfully registered with AAD tenant [$current_user_tid]."
echo "$lp Creating app service principal. This might take a while..."

sleep 30 # Give AAD a chance to catch up...

aad_sp_id=$(az ad sp create --id "$aad_app_id" --query objectId --output tsv 2>/dev/null);

echo "$lp Granting AAD app [$aad_app_name] service principal [$aad_sp_id] contributor access to resource group [$resource_group_name]..."

sleep 30 # Give AAD a chance to catch up...

az role assignment create \
    --role "Contributor" \
    --assignee "$aad_sp_id" \
    --resource-group "$resource_group_name"

echo "$lp Tagging resource group [$resource_group_name]..."

az group update \
    --name "$resource_group_name" \
    --tags \
        "Mona Version"="$mona_version" \
        "Deployment Name"="$deployment_name" \
        "AAD App ID"="$aad_app_id" \
        "AAD App Name"="$aad_app_name" >/dev/null

# Deploy the ARM template.

echo "$lp Deploying Mona to subscription [$subscription_id] resource group [$resource_group_name]. This might take a while...";

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
        language="$language" \
        appServicePlanId="$app_service_plan_id"

[[ $? -eq 0 ]] && echo "$lp ✔   Mona resources successfully deployed [$az_deployment_name] to resource group [$resource_group_name].";
[[ $? -ne 0 ]] && echo "$lp ❌   Mona resource group [$resource_group_name] deployment [$az_deployment_name] has failed. Aborting setup..." && exit 1;
        
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
app_insights_key=$(az deployment group show --resource-group "$resource_group_name" --name "$az_deployment_name" --query properties.outputs.appInsightsInstrumentationKey.value --output tsv);

# Configure Mona.

echo "$lp Configuring Mona settings...";

az appconfig kv set --name "$app_config_name" --key "Deployment:MonaVersion" --yes                                      --value "$mona_version";                
az appconfig kv set --name "$app_config_name" --key "Deployment:AppInsightsInstrumentationKey" --yes                    --value "$app_insights_key";       
az appconfig kv set --name "$app_config_name" --key "Deployment:AzureResourceGroupName" --yes                           --value "$resource_group_name";         
az appconfig kv set --name "$app_config_name" --key "Deployment:AzureSubscriptionId" --yes                              --value "$subscription_id";             
az appconfig kv set --name "$app_config_name" --key "Deployment:IsTestModeEnabled" --yes                                --value "true";                         
az appconfig kv set --name "$app_config_name" --key "Deployment:Name" --yes                                             --value "$deployment_name";             
az appconfig kv set --name "$app_config_name" --key "Identity:AdminIdentity:AadTenantId" --yes                          --value "$current_user_tid";
az appconfig kv set --name "$app_config_name" --key "Identity:AdminIdentity:AadUserId" --yes                            --value "$current_user_oid";
az appconfig kv set --name "$app_config_name" --key "Identity:AppIdentity:AadClientId" --yes                            --value "$aad_app_id";
az appconfig kv set --name "$app_config_name" --key "Identity:AppIdentity:AadClientSecret" --yes                        --value "$aad_app_secret" >/dev/null
az appconfig kv set --name "$app_config_name" --key "Identity:AppIdentity:AadTenantId" --yes                            --value "$current_user_tid";
az appconfig kv set --name "$app_config_name" --key "Offer:IsSetupComplete" --yes                                       --value "false";
az appconfig kv set --name "$app_config_name" --key "Subscriptions:Events:EventGrid:TopicEndpoint" --yes                --value "$event_grid_topic_endpoint"; 
az appconfig kv set --name "$app_config_name" --key "Subscriptions:Events:EventGrid:TopicKey" --yes                     --value "$event_grid_topic_key" >/dev/null
az appconfig kv set --name "$app_config_name" --key "Subscriptions:Repository:BlobStorage:ConnectionString" --yes       --value "$blob_conn_str" >/dev/null
az appconfig kv set --name "$app_config_name" --key "Subscriptions:Repository:BlobStorage:ContainerName" --yes          --value "$blob_sub_container_name"; 

# Configure AD app reply and ID URLs.

echo "$lp Completing Mona configuration..."

az ad app update \
    --id "$aad_app_id" \
    --reply-urls "$web_app_base_url/signin-oidc";

# Set web app configuration service connection string.

az webapp config appsettings set \
    --resource-group "$resource_group_name" \
    --name "$web_app_name" \
    --settings APP_CONFIG_SERVICE_CONNECTION_STRING="$app_config_connection_string" >/dev/null # Sensitive

if [[ -z $no_publish ]]; then
    # Deploy Mona web application...

    echo "$lp Packaging Mona web app for deployment to [$web_app_name]..."

    dotnet publish -c Release -o ./topublish ../Mona.SaaS.Web/Mona.SaaS.Web.csproj

    echo "$lp Zipping Mona web app deployment package..."

    cd ./topublish
    zip -r ../topublish.zip . >/dev/null
    cd ..

    echo "$lp Deploying Mona web app to [$web_app_name]..."

    az webapp deployment source config-zip -g "$resource_group_name" -n "$web_app_name" --src ./topublish.zip

    # And scene...

    echo "$lp Cleaning up..."

    rm -rf ./topublish >/dev/null
    rm -rf ./topublish.zip >/dev/null
fi

printf "\n$lp Mona Deployment Summary\n"
echo
printf "$lp Deployment Name                     [$deployment_name]\n"
printf "$lp Deployment Version                  [$mona_version]\n"
printf "$lp Deployed to Azure Subscription      [$subscription_id]\n"
printf "$lp Deployed to Resource Group          [$resource_group_name]\n"
printf "$lp Deployment AAD Client ID            [$aad_app_id]\n"
printf "$lp Deployment AAD Tenant ID            [$current_user_tid]\n"

if [[ -z $no_publish ]]; then
    printf "$lp Landing Page URL                    [$web_app_base_url/]\n"
    printf "$lp Landing Page URL (Testing)          [$web_app_base_url/test]\n"
    printf "$lp Webhook URL                         [$web_app_base_url/webhook]\n"
    printf "$lp Webhook URL (Testing)               [$web_app_base_url/webhook/test]\n"
    printf "$lp Admin Center URL                    [$web_app_base_url/admin]\n"
fi

echo
echo "$lp ✔   Mona deployment complete."

if [[ -z $no_publish ]]; then
    echo
    echo "$lp ▶   Please visit [ $web_app_base_url/setup ] to complete your setup and begin transacting with Microsoft!"
fi