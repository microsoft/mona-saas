#!/bin/bash

# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

mona_version=$(cat ../../VERSION)

exec 3>&2 # Grabbing a reliable stderr handle...

usage() { printf "\nUsage: $0 <-n deployment-name> <-r deployment-region> [-i integration-pack] [-a app-service-plan-id] [-d display-name] [-g resource-group] [-l ui-language] [-s subscription-id] [-e event-version] [-h] [-p]\n"; }

check_az() {
    exec 3>&2

    az version >/dev/null 2>&1

    # lp=$1 TODO: Need to check why this variable `lp` is used throughout the script. Maybe for formatting reasons?

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

    # lp=$1 TODO: Need to check why this variable `lp` is used throughout the script. Maybe for formatting reasons?

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

check_account_type() {
  lp=$1
  
  graph_token=$(az account get-access-token \
      --resource-type ms-graph \
      --query accessToken \
      --output tsv);
  
  user_type=$(curl --location --request GET 'https://graph.microsoft.com/v1.0/me?$select=userType' -H "Content-Type: application/json" -H "Authorization: Bearer $graph_token" --no-progress-meter | jq -r ".userType");

  if [ "$user_type" = "Guest" ]; then
      echo "$lp ❌   Mona cannot be installed using a Guest account. Please use Workplace or School account with type Member."
      return 1
  fi
  
  is_msa=$(curl --location --request GET 'https://graph.microsoft.com/v1.0/me?$select=identities' -H "Content-Type: application/json" -H "Authorization: Bearer $graph_token" --no-progress-meter | jq -r '.identities | map(. | select(.issuer=="MicrosoftAccount")) | . | length > 0');

  if [ "$is_msa" = "true" ]; then
    echo "$lp ❌   Mona cannot be installed using a personal Microsoft account. Please use Workplace or School account with type Member."
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
        echo "$lp ❌   [$region] is not a valid Azure region, but these are..."
        echo
        az account list-locations --output table --query "[].name"
        echo
        return 1
    else
        echo "$lp ✔   [$region] is a valid Azure region ($region_display_name)."
    fi
}

check_event_version() {
    lp=$1
    event_version=$2
    supported_versions=("2021-10-01" "2021-05-01")

    if [[ " ${supported_versions[*]} " == *"$event_version"* ]]; then
        echo "$lp ✔   [$event_version] subscription event version is supported."
    else
        echo "$lp ❌   [$event_version] subscription event version is not supported."
        return 1
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

event_version="2021-10-01" # Default event version is always the latest one. Can be overridden using [-e] flag below for backward compatibility.
language="en" # Default UI language is English ("en"). Can be overridden using [-l] flag below.
integration_pack="default"

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
        e)
            event_version=$OPTARG
        ;;
        i)
            integration_pack=$OPTARG
        ;;
        h)
            no_splash=1
        ;;
        p)
            no_publish=1
        ;;
        j)
            no_rbac=1 # Ill-advised. Only here for backward compatibility with early versions of Mona.
        ;;
        \?)
            usage
            exit 1
        ;;
    esac
done

# Check that user account is of valid type
check_account_type "$lp"

[[ $? -ne 0 ]] && exit 1;

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
check_event_version "$lp" "$event_version"; if [[ $? -ne 0 ]]; then param_valid_failed=1; fi;
check_language "$lp" "$language"; if [[ $? -ne 0 ]]; then param_valid_failed=1; fi;

if [[ -n $app_service_plan_id ]]; then
    check_app_service_plan "$lp" "$app_service_plan_id"
    if [[ $? -ne 0 ]]; then exit 1; fi;
fi

if [[ -n $param_valid_failed ]]; then
    echo "$lp ❌   Parameter validation failed. Please review then try again. Setup failed."
    exit 1
fi

while [[ -z $current_user_oid ]]; do
    current_user_oid=$(az ad signed-in-user show --query id --output tsv 2>/dev/null);
    if [[ -z $current_user_oid ]]; then az login; fi;
done

# Make sure that we're pointing at the right subscription.

if [[ -n $subscription_id ]]; then
    az account set --subscription $subscription_id
    [[ $? -ne 0 ]] && echo "$lp ❌   Azure subscription [$subscription_id] not found. Setup failed" && exit 1;
fi

# Create the resource group if it doesn't already exist.
# If it already exists confirm that it's empty.

if [[ $(az group exists --resource-group "$resource_group_name" --output tsv) == false ]]; then
    echo "$lp Creating resource group [$resource_group_name]..."

    az group create \
        --location "$deployment_region" \
        --name "$resource_group_name" \
        --tags \
            "Mona Version"="$mona_version" \
            "Deployment Name"="$deployment_name"

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

# Create the Mona app registration in AAD...

echo "$lp 🛡️   Creating Mona Azure Active Directory (AAD) app registration..."

mona_aad_app_name="$display_name"

graph_token=$(az account get-access-token \
    --resource-type ms-graph \
    --query accessToken \
    --output tsv);

mona_admin_role_id=$(cat /proc/sys/kernel/random/uuid)
create_mona_app_json=$(cat ./aad/manifest.json)
create_mona_app_json="${create_mona_app_json/__aad_app_name__/${mona_aad_app_name}}"
create_mona_app_json="${create_mona_app_json/__deployment_name__/${deployment_name}}"
create_mona_app_json="${create_mona_app_json/__admin_role_id__/${mona_admin_role_id}}"

# Getting around some occasional consistency issues by implementing the retry pattern. This was fun.
# If you're reading this code and you've never heard of the retry pattern, check this out --
# https://docs.microsoft.com/en-us/azure/architecture/patterns/retry

for i1 in {1..5}; do
    create_mona_app_response=$(curl \
        -X POST \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $graph_token" \
        -d "$create_mona_app_json" \
        "https://graph.microsoft.com/v1.0/applications")

    mona_aad_object_id=$(echo "$create_mona_app_response" | jq -r ".id")
    mona_aad_app_id=$(echo "$create_mona_app_response" | jq -r ".appId")

    if [[ -z $mona_aad_object_id || -z $mona_aad_app_id || $mona_aad_object_id == null || $mona_aad_sp_id == null ]]; then
        if [[ $i1 == 5 ]]; then
            # We tried and we failed. Such is life.
            echo "$lp ❌   Failed to create Mona AAD app. Setup failed."
            exit 1
        else
            sleep_for=$((2**i1)) # Exponential backoff. 2..4..8..16 seconds.
            echo "$lp ⚠️   Trying to create app again in [$sleep_for] seconds."
            sleep $sleep_for
        fi
    else
        break
    fi
done

# Create the Mona client secret...

echo "$lp 🛡️   Creating Mona Azure Active Directory (AAD) client credentials..."

add_mona_password_json=$(cat ./aad/add_password.json)

for i2 in {1..5}; do
    add_mona_password_response=$(curl \
        -X POST \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $graph_token" \
        -d "$add_mona_password_json" \
        "https://graph.microsoft.com/v1.0/applications/$mona_aad_object_id/addPassword")

    mona_aad_app_secret=$(echo "$add_mona_password_response" | jq -r ".secretText")

    if [[ -z $mona_aad_app_secret || $mona_aad_app_secret == null ]]; then
        if [[ $i2 == 5 ]]; then
            echo "$lp ❌   Failed to create Mona AAD app client credentials. Setup failed."
            exit 1
        else
            sleep_for=$((2**i2))
            echo "$lp ⚠️   Trying to create client credentials again in [$sleep_for] seconds."
            sleep $sleep_for
        fi
    else
        break
    fi
done

echo "$lp 🛡️   Creating Mona AAD app [$mona_aad_app_name] service principal..."

for i3 in {1..5}; do
    mona_aad_sp_id=$(az ad sp create --id "$mona_aad_app_id" --query id --output tsv);

    if [[ -z $mona_aad_sp_id || $mona_aad_sp_id == null ]]; then
        if [[ $i3 == 5 ]]; then
            echo "$lp ❌   Failed to create Mona AAD app service principal. Setup failed."
            exit 1
        else
            sleep_for=$((2**i3))
            echo "$lp ⚠️   Trying to create service principal again in [$sleep_for] seconds."
            sleep $sleep_for
        fi
    else
        break
    fi
done

echo "$lp 🔐   Granting Mona service principal contributor access to [$resource_group_name]..."

for i4 in {1..5}; do
    az role assignment create \
        --role "Contributor" \
        --assignee "$mona_aad_sp_id" \
        --resource-group "$resource_group_name"

    if [[ $? -ne 0 ]]; then
        if [[ $i4 == 5 ]]; then
            echo "$lp ❌   Failed to grant Mona service principal contributor access. Setup failed."
            exit 1
        else
            sleep_for=$((2**i4))
            echo "$lp ⚠️   Trying to grant service principal contributor access again in [$sleep_for] seconds."
            sleep $sleep_for
        fi
    else
        break
    fi
done

# Deploy the ARM template.

echo "$lp 🦾   Deploying Mona to subscription [$subscription_id] resource group [$resource_group_name]. This might take a while...";

az_deployment_name="mona-deploy-$deployment_name"

az deployment group create \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --template-file "./templates/basic-deploy.json" \
    --parameters \
        deploymentName="$deployment_name" \
        aadTenantId="$current_user_tid" \
        aadPrincipalId="$mona_aad_sp_id" \
        aadClientId="$mona_aad_app_id" \
        aadClientSecret="$mona_aad_app_secret" \
        language="$language" \
        appServicePlanId="$app_service_plan_id" \
        eventVersion="$event_version"

[[ $? -eq 0 ]] && echo "$lp ✔   Mona resources successfully deployed [$az_deployment_name] to resource group [$resource_group_name].";
[[ $? -ne 0 ]] && echo "$lp ❌   Mona resource group [$resource_group_name] deployment [$az_deployment_name] has failed. Aborting setup..." && exit 1;

# Get ARM deployment output variables.

storage_account_name=$(az deployment group show --resource-group "$resource_group_name" --name "$az_deployment_name" --query properties.outputs.storageAccountName.value --output tsv);
web_app_base_url=$(az deployment group show --resource-group "$resource_group_name" --name "$az_deployment_name" --query properties.outputs.webAppBaseUrl.value --output tsv);
web_app_name=$(az deployment group show --resource-group "$resource_group_name" --name "$az_deployment_name" --query properties.outputs.webAppName.value --output tsv);

# Deploy integration pack.

integration_pack="${integration_pack#/}" # Trim leading...
integration_pack="${integration_pack%/}" # and trailing slashes.

pack_absolute_path="$integration_pack/deploy_pack.bicep" # Absolute...
pack_relative_path="./integration_packs/$integration_pack/deploy_pack.bicep" # and relative pack paths.

if [[ -f "$pack_absolute_path" ]]; then # Check the absolute path first...
    pack_path="$pack_absolute_path"
elif [[ -f "$pack_relative_path" ]]; then # then check the relative path.
    pack_path="$pack_relative_path"
fi

if [[ -z "$pack_path" ]]; then
    echo "$lp ⚠️   Integration pack [$integration_pack] not found at [$pack_absolute_path] or [$pack_relative_path]. No integration pack will be deployed."
else
    echo "$lp 🦾   Deploying [$integration_pack ($pack_path)] integration pack..."

    az deployment group create \
        --resource-group "$resource_group_name" \
        --name "mona-pack-deploy-${deployment_name}" \
        --template-file "$pack_path" \
        --parameters \
            deploymentName="$deployment_name" \
            aadClientId="$mona_aad_app_id" \
            aadClientSecret="$mona_aad_app_secret" \
            aadTenantId="$current_user_tid"

    [[ $? -eq 0 ]] && echo "$lp ✔   Integration pack [$integration_pack ($pack_path)] deployed.";
    [[ $? -ne 0 ]] && echo "$lp ⚠️   Integration pack [$integration_pack ($pack_path)] deployment failed."
fi

# Configure Mona.

echo "$lp 🔐   Adding you to the Mona administrators role...";

# Regardless of whether or not -j was set, add the current user to the admin role...

curl -X POST \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $graph_token" \
    -d "{ \"principalId\": \"$current_user_oid\", \"resourceId\": \"$mona_aad_sp_id\", \"appRoleId\": \"$mona_admin_role_id\" }" \
    "https://graph.microsoft.com/v1.0/users/$current_user_oid/appRoleAssignments"

if [[ -z $no_publish ]]; then
    # Deploy Mona web application...

    echo "$lp 🏗️   Building Mona web app for deployment to [$web_app_name]..."

    dotnet publish -c Release -o ./topublish ../Mona.SaaS.Web/Mona.SaaS.Web.csproj

    cd ./topublish
    zip -r ../topublish.zip . >/dev/null
    cd ..

    echo "$lp ☁️   Publishing Mona web app to [$web_app_name]..."

    az webapp deployment source config-zip -g "$resource_group_name" -n "$web_app_name" --src ./topublish.zip

    # And scene...

    echo "$lp 🧹   Cleaning up..."

    rm -rf ./topublish >/dev/null
    rm -rf ./topublish.zip >/dev/null
fi

printf "\n$lp Mona Deployment Summary\n"
echo
printf "$lp Deployment Name                     [$deployment_name]\n"
printf "$lp Deployment Version                  [$mona_version]\n"
printf "$lp Deployed to Azure Subscription      [$subscription_id]\n"
printf "$lp Deployed to Resource Group          [$resource_group_name]\n"
printf "$lp Deployment AAD Client ID            [$mona_aad_app_id]\n"
printf "$lp Deployment AAD Tenant ID            [$current_user_tid]\n"

if [[ -z $no_publish ]]; then
    printf "$lp Landing Page URL                    [$web_app_base_url/]\n"
    printf "$lp Landing Page URL (Testing)          [$web_app_base_url/test]\n"
    printf "$lp Webhook URL                         [$web_app_base_url/webhook]\n"
    printf "$lp Webhook URL (Testing)               [$web_app_base_url/webhook/test]\n"
    printf "$lp Admin Center URL                    [$web_app_base_url/admin]\n"
    printf "$lp Subscription Staging Store Base URL [https://$storage_account_name.blob.core.windows.net]\n"
fi

echo
echo "$lp ✔   Mona deployment complete."
echo
echo "$lp ⚠️   PLEASE READ CAREFULLY"
echo
echo "$lp By default, Mona will not automatically activate subscriptions with the Marketplace. See this doc... "
echo "$lp [ https://github.com/microsoft/mona-saas/tree/main/docs#how-do-i-notify-the-marketplace-that-a-subscription-has-been-activated ]"
echo
echo "$lp Check to make sure your logic apps are connected to Mona. See this doc... "
echo "$lp [ https://github.com/microsoft/mona-saas/tree/main/docs#why-arent-my-mona-events-triggering-my-logic-apps ]"
echo
echo "$lp For additional information, review our frequently asked questions (FAQs) at... "
echo "$lp [ https://github.com/microsoft/mona-saas/tree/main/docs ]"

if [[ -z $no_publish ]]; then
    echo
    echo "$lp ▶   Please visit [ $web_app_base_url/setup ] to complete your setup and begin transacting with Microsoft!"
fi
