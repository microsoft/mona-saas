#!/bin/bash

SECONDS=0 # Let's time it...

usage() { echo "Usage: $0 <-n name> <-r deployment_region> [-d display_name]"; }

check_az() {
    az version >/dev/null

    if [[ $? -ne 0 ]]; then
        echo "❌   Please install the Azure CLI before continuing. See [https://docs.microsoft.com/cli/azure/install-azure-cli] for more information."
        return 1
    else
        echo "✔   Azure CLI installed."
    fi
}

check_dotnet() {
    dotnet_version=$(dotnet --version)

    if [[ $dotnet_version == 6.* ]]; then # Needs to be .NET 6
        echo "✔   .NET [$dotnet_version] installed."
    else
        read -p "⚠️  .NET 6 is required to run this script but is not installed. Would you like to install it now? [Y/n]" install_dotnet

        case "$install_dotnet" in
            [yY1]   )
                wget https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh
                chmod +x ./dotnet-install.sh
                ./dotnet-install.sh 

                if [[ $? == 0 ]]; then
                    export PATH="$HOME/.dotnet:$PATH"
                    dotnet_version=$(dotnet --version)
                    echo "✔   .NET [$dotnet_version] installed."
                    return 0
                else
                    echo "❌   .NET 6 installation failed. See [https://docs.microsoft.com/cli/azure/install-azure-cli] for more information."
                    return 1
                fi
            ;;
            *       )
                echo "❌   Please install .NET 6 before continuing. See [https://docs.microsoft.com/cli/azure/install-azure-cli] for more information."
                return 1
            ;;
        esac
    fi
}

check_deployment_name() {
    name=$1

    if [[ $name =~ ^[a-z0-9]{5,13}$ ]]; then
        echo "✔   [$name] is a valid deployment name."
    else
        echo "❌   [$name] is not a valid deployment name. The name must contain only lowercase letters and numbers and be between 5 and 13 characters in length."
        return 1
    fi
}

check_deployment_region() {
    region=$1

    region_display_name=$(az account list-locations -o tsv --query "[?name=='$region'].displayName")

    if [[ -z $region_display_name ]]; then
        echo "❌   [$region] is not a valid Azure region. For a full list of Azure regions, run 'az account list-locations -o table'."
        return 1
    else
        echo "✔   [$region] is a valid Azure region ($region_display_name)."
    fi
}

clean_up() {
    echo "🧹   Cleaning up..."

    rm -rf ./mona_web_topublish >/dev/null
    rm -rf ./mona_web_topublish.zip >/dev/null
    rm -rf ./relay_topublish >/dev/null
    rm -rf ./relay_topublish.zip >/dev/null
    rm -rf ./turn_api_topublish >/dev/null
    rm -rf ./turn_api_topublish.zip >/dev/null
    rm -rf ./turn_web_topublish >/dev/null
    rm -rf ./turn_web_topublish.zip >/dev/null
    rm -rf ./turnstile >/dev/null
}

splash() {
    echo "Mona + Turnstile Unified Installer"
    echo "https://github.com/microsoft/mona-saas"
    echo "https://github.com/microsoft/turnstile"
    echo
    echo "Copyright (c) Microsoft Corporation. All rights reserved."
    echo "Licensed under the MIT License. See LICENSE in project root for more information."
    echo
}

# Howdy!

splash

# Clean up anything left behind if the previous run failed...

clean_up

# Make sure all pre-reqs are installed...

echo "Checking setup prerequisites..."

check_az;           [[ $? -ne 0 ]] && prereq_check_failed=1
check_dotnet;       [[ $? -ne 0 ]] && prereq_check_failed=1

if [[ -z $prereq_check_failed ]]; then
    echo "✔   All setup prerequisites installed."
else
    echo "❌   Please install all setup prerequisites then try again."
    return 1
fi

# Upgrade to the latest version of Bicep...

az bicep upgrade

# Log in the user if they aren't already...

while [[ -z $current_user_oid ]]; do
    current_user_oid=$(az ad signed-in-user show --query id --output tsv 2>/dev/null);
    if [[ -z $current_user_oid ]]; then az login; fi;
done

# Get our parameters...

while getopts "n:r:d:" opt; do
    case $opt in
        n)
            p_deployment_name=$OPTARG
        ;;
        r)
            p_deployment_region=$OPTARG
        ;;
        d)
            p_display_name=$OPTARG
        ;;
        \?)
            usage
            exit 1
        ;;
    esac
done

echo "Validating script parameters..."

[[ -z p_deployment_name || -z p_deployment_region ]] && { usage; exit 1; }

check_deployment_region $p_deployment_region;   [[ $? -ne 0 ]] && param_check_failed=1
check_deployment_name $p_deployment_name;       [[ $? -ne 0 ]] && param_check_failed=1

if [[ -z $param_check_failed ]]; then
    echo "✔   All setup parameters are valid."
else
    echo "❌   Parameter validation failed. Please review and try again."
    return 1
fi

p_deployment_name=$(echo "$p_deployment_name" | tr '[:upper:]' '[:lower:]') # Lower the deployment name...

if [[ -z $p_display_name ]]; then
    display_name="$p_deployment_name SaaS"
else
    display_name="$p_display_name"
fi

# Go get Turnstile...

echo "⬇️   Cloning Turnstile repository..."

git clone https://github.com/microsoft/turnstile

# Create our resource group if it doesn't already exist...

resource_group_name="saas-$p_deployment_name"

if [[ $(az group exists --resource-group "$resource_group_name" --output tsv) == false ]]; then
    echo "Creating resource group [$resource_group_name]..."

    az group create --location "$p_deployment_region" --name "$resource_group_name"

    if [[ $? -eq 0 ]]; then
        echo "✔   Resource group [$resource_group_name] created."
    else
        echo "❌   Unable to create resource group [$resource_group_name]."
        exit 1
    fi
fi

# Create the Mona app registration in AAD...

echo "🛡️   Creating Mona Azure Active Directory (AAD) app registration..."

mona_aad_app_name="$display_name"

graph_token=$(az account get-access-token \
    --resource-type ms-graph \
    --query accessToken \
    --output tsv);

mona_admin_role_id=$(cat /proc/sys/kernel/random/uuid)
create_mona_app_json=$(cat ./aad/manifest.json)
create_mona_app_json="${create_mona_app_json/__aad_app_name__/${mona_aad_app_name}}"
create_mona_app_json="${create_mona_app_json/__deployment_name__/${p_deployment_name}}"
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

    if [[ -z $mona_aad_object_id || -z $mona_aad_app_id ]]; then
        if [[ i1 == 5 ]]; then
            # We tried and we failed. Such is life.
            clean_up
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

echo "🛡️   Creating Turnstile Azure Active Directory (AAD) app registration..."

turn_aad_app_name="$display_name Seating"

turn_tenant_admin_role_id=$(cat /proc/sys/kernel/random/uuid)
turn_admin_role_id=$(cat /proc/sys/kernel/random/uuid)
create_turn_app_json=$(cat ./aad/turnstile/manifest.json)
create_turn_app_json="${create_turn_app_json/__aad_app_name__/${turn_aad_app_name}}"
create_turn_app_json="${create_turn_app_json/__deployment_name__/${p_deployment_name}}"
create_turn_app_json="${create_turn_app_json/__tenant_admin_role_id__/${turn_tenant_admin_role_id}}"
create_turn_app_json="${create_turn_app_json/__turnstile_admin_role_id__/${turn_admin_role_id}}"

for i2 in {1..5}; do
    create_turn_app_response=$(curl \
        -X POST \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $graph_token" \
        -d "$create_turn_app_json" \
        "https://graph.microsoft.com/v1.0/applications")

    turn_aad_object_id=$(echo "$create_turn_app_response" | jq -r ".id")
    turn_aad_app_id=$(echo "$create_turn_app_response" | jq -r ".appId")

     if [[ -z $turn_aad_object_id || -z $turn_aad_app_id ]]; then
        if [[ i2 == 5 ]]; then
            clean_up
            echo "$lp ❌   Failed to create Turnstile AAD app. Setup failed."
            exit 1
        else
            sleep_for=$((2**i2))
            echo "$lp ⚠️   Trying to create app again in [$sleep_for] seconds."
            sleep $sleep_for
        fi
    else
        break
    fi
done

# Create the Mona client secret...

echo "🛡️   Creating Mona Azure Active Directory (AAD) client credentials..."

add_mona_password_json=$(cat ./aad/add_password.json)

for i3 in {1..5}; do
    add_mona_password_response=$(curl \
        -X POST \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $graph_token" \
        -d "$add_mona_password_json" \
        "https://graph.microsoft.com/v1.0/applications/$mona_aad_object_id/addPassword")

    mona_aad_app_secret=$(echo "$add_mona_password_response" | jq -r ".secretText")

    if [[ -z $mona_aad_app_secret ]]; then
        if [[ i3 == 5 ]]; then
            clean_up
            echo "$lp ❌   Failed to create Mona AAD app client credentials. Setup failed."
            exit 1
        else
            sleep_for=$((2**i3))
            echo "$lp ⚠️   Trying to create client credentials again in [$sleep_for] seconds."
            sleep $sleep_for
        fi
    else
        break
    fi
done

# Then, create the Turnstile client secret...

echo "🛡️   Creating Turnstile Azure Active Directory (AAD) client credentials..."

add_turn_password_json=$(cat ./aad/turnstile/add_password.json)

for i4 in {1..5}; do
    add_turn_password_response=$(curl \
        -X POST \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $graph_token" \
        -d "$add_turn_password_json" \
        "https://graph.microsoft.com/v1.0/applications/$turn_aad_object_id/addPassword")

    turn_aad_app_secret=$(echo "$add_turn_password_response" | jq -r ".secretText")

    if [[ -z $turn_aad_app_secret ]]; then
        if [[ i4 == 5 ]]; then
            clean_up
            echo "$lp ❌   Failed to create Turnstile AAD app client credentials. Setup failed."
            exit 1
        else
            sleep_for=$((2**i4))
            echo "$lp ⚠️   Trying to create client credentials again in [$sleep_for] seconds."
            sleep $sleep_for
        fi
    else
        break
    fi
done

echo "🛡️   Creating Mona AAD app [$mona_aad_app_name] service principal..."

for i5 in {1..5}; do
    mona_aad_sp_id=$(az ad sp create --id "$mona_aad_app_id" --query id --output tsv);

    if [[ -z $mona_aad_sp_id ]]; then
        if [[ i5 == 5 ]]; then
            clean_up
            echo "$lp ❌   Failed to create Mona AAD app service principal. Setup failed."
            exit 1
        else
            sleep_for=$((2**i5))
            echo "$lp ⚠️   Trying to create service principal again in [$sleep_for] seconds."
            sleep $sleep_for
        fi
    else
        break
    fi
done

echo "🛡️   Creating Turnstile AAD app [$turn_aad_app_name] service principal..."

for i6 in {1..5}; do
    turn_aad_sp_id=$(az ad sp create --id "$turn_aad_app_id" --query id --output tsv);

     if [[ -z $turn_aad_sp_id ]]; then
        if [[ i6 == 5 ]]; then
            clean_up
            echo "$lp ❌   Failed to create Turnstile AAD app service principal. Setup failed."
            exit 1
        else
            sleep_for=$((2**i6))
            echo "$lp ⚠️   Trying to create service principal again in [$sleep_for] seconds."
            sleep $sleep_for
        fi
    else
        break
    fi
done

echo "🔐   Granting Mona service principal contributor access to [$resource_group_name]..."

for 17 in {1..5}; do
    az role assignment create \
        --role "Contributor" \
        --assignee "$mona_aad_sp_id" \
        --resource-group "$resource_group_name"

    if [[ $? -ne 0 ]]; then
        if [[ i7 == 5 ]]; then
            clean_up
            echo "$lp ❌   Failed to grant Mona service principal contributor access. Setup failed."
            exit 1
        else
            sleep_for=$((2**i7))
            echo "$lp ⚠️   Trying to grant service principal contributor access again in [$sleep_for] seconds."
            sleep $sleep_for
        fi
    else
        break
    fi
done

echo "🔐   Granting Mona service principal contributor access to [$resource_group_name]..."

for i8 in {1..5}; do
    az role assignment create \
        --role "Contributor" \
        --assignee "$turn_aad_sp_id" \
        --resource-group "$resource_group_name"

    if [[ $? -ne 0 ]]; then
        if [[ i8 == 5 ]]; then
            clean_up
            echo "$lp ❌   Failed to grant Turnstile service principal contributor access. Setup failed."
            exit 1
        else
            sleep_for=$((2**i8))
            echo "$lp ⚠️   Trying to grant service principal contributor access again in [$sleep_for] seconds."
            sleep $sleep_for
        fi
    else
        break
    fi
done

# Deploy the combined Bicep template...

subscription_id=$(az account show --query id --output tsv);
current_user_tid=$(az account show --query tenantId --output tsv);
az_deployment_name="saas-deploy-$p_deployment_name"

echo "🦾   Deploying Mona + Turnstile Bicep template to subscription [$subscription_id] resource group [$resource_group_name]..."

az deployment group create \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --template-file "./templates/mona_and_turnstile.bicep" \
    --parameters \
        deploymentName="$p_deployment_name" \
        monaAadClientId="$mona_aad_app_id" \
        monaAadPrincipalId="$mona_aad_sp_id" \
        monaAadTenantId="$current_user_tid" \
        monaAadClientSecret="$mona_aad_app_secret" \
        turnAadClientId="$turn_aad_app_id" \
        turnAadTenantId="$current_user_tid" \
        turnAadClientSecret="$turn_aad_app_secret"

storage_account_name=$(az deployment group show \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --query properties.outputs.storageAccountName.value \
    --output tsv)

storage_account_key=$(az deployment group show \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --query properties.outputs.storageAccountKey.value \
    --output tsv)

mona_publisher_config_json=$(az deployment group show \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --query properties.outputs.monaPublisherConfig.value \
    --output json)

turn_publisher_config_json=$(az deployment group show \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --query properties.outputs.turnPublisherConfig.value \
    --output json)

mona_web_app_name=$(az deployment group show \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --query properties.outputs.monaWebAppName.value \
    --output tsv)

relay_app_name=$(az deployment group show \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --query properties.outputs.relayName.value \
    --output tsv)

turn_web_app_name=$(az deployment group show \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --query properties.outputs.turnWebAppName.value \
    --output tsv)

turn_api_app_name=$(az deployment group show \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --query properties.outputs.turnApiAppName.value \
    --output tsv);

relay_app_id=$(az deployment group show \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --query properties.outputs.relayId.value \
    --output tsv)

event_grid_topic_id=$(az deployment group show \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --query properties.outputs.topicId.value \
    --output tsv)

event_grid_topic_name=$(az deployment group show \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --query properties.outputs.topicName.value \
    --output tsv)

event_grid_connection_name=$(az deployment group show \
    --resource-group "$resource_group_name" \
    --name "$az_deployment_name" \
    --query properties.outputs.topicConnectionName.value \
    --output tsv)

echo "⚙️   Applying publisher configuration..."

az storage blob upload \
    --account-name "$storage_account_name" \
    --account-key "$storage_account_key" \
    --container-name "mona-configuration" \
    --data "$mona_publisher_config_json" \
    --name "publisher-config.json" &
    
mona_pub_config_pid=$!

az storage blob upload \
    --account-name "$storage_account_name" \
    --account-key "$storage_account_key" \
    --container-name "turn-configuration" \
    --data "$turn_publisher_config_json" \
    --name "publisher_config.json" &
    
turn_pub_config_pid=$!

wait $mona_pub_config_pid
wait $turn_pub_config_pid

echo "🦾   Deploying integration packs..."

az deployment group create \
    --resource-group "$resource_group_name" \
    --name "mona-pack-deploy-$p_deployment_name" \
    --template-file "./integration_packs/default/deploy_pack.bicep" \
    --parameters \
        deploymentName="$p_deployment_name" \
        aadClientId="$mona_aad_app_id" \
        aadClientSecret="$mona_aad_app_secret" \
        aadTenantId="$current_user_tid" \
        eventGridTopicName="$event_grid_topic_name" \
        eventGridConnectionName="$event_grid_connection_name" &

deploy_mona_pack_pid=$!

az deployment group create \
    --resource-group "$resource_group_name" \
    --name "turn-pack-deploy-$p_deployment_name" \
    --template-file "./turnstile/Turnstile/Turnstile.Setup/integration_packs/default/deploy_pack.bicep" \
    --parameters \
        deploymentName="$p_deployment_name" \
        eventGridTopicName="$event_grid_topic_name" \
        eventGridConnectionName="$event_grid_connection_name" &

deploy_turn_pack_pid=$!

wait $deploy_mona_pack_pid
wait $deploy_turn_pack_pid

echo "🔐   Adding you to Mona and Turnstile administrative roles..."

# Add the current user to the Mona administrators role...

curl -X POST \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $graph_token" \
    -d "{ \"principalId\": \"$current_user_oid\", \"resourceId\": \"$mona_aad_sp_id\", \"appRoleId\": \"$mona_admin_role_id\" }" \
    "https://graph.microsoft.com/v1.0/users/$current_user_oid/appRoleAssignments" >/dev/null &

mona_admin_role_assign_pid=$!

# Add the current user to the subscriber tenant administrator's AAD role...

curl -X POST \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $graph_token" \
    -d "{ \"principalId\": \"$current_user_oid\", \"resourceId\": \"$turn_aad_sp_id\", \"appRoleId\": \"$turn_tenant_admin_role_id\" }" \
    "https://graph.microsoft.com/v1.0/users/$current_user_oid/appRoleAssignments" >/dev/null &

turn_tenant_admin_role_assign_pid=$!

# Add the current user to the turnstile administrator's AAD role...

curl -X POST \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $graph_token" \
    -d "{ \"principalId\": \"$current_user_oid\", \"resourceId\": \"$turn_aad_sp_id\", \"appRoleId\": \"$turn_admin_role_id\" }" \
    "https://graph.microsoft.com/v1.0/users/$current_user_oid/appRoleAssignments" >/dev/null &

turn_admin_role_assign_pid=$!

wait $mona_admin_role_assign_pid
wait $turn_tenant_admin_role_assign_pid
wait $turn_admin_role_assign_pid

echo
echo "🏗️   Building apps..."

# We have to stagger this kind of weird to parallelize the builds but not run into within the same sln.

dotnet publish -c Release -o ./mona_web_topublish ../Mona.SaaS.Web/Mona.SaaS.Web.csproj &

build_mona_web_pid=$!

dotnet publish -c Release -o ./turn_api_topublish ./turnstile/Turnstile/Turnstile.Api/Turnstile.Api.csproj &

build_turn_api_pid=$!

wait $build_mona_web_pid
wait $build_turn_api_pid

cd ./mona_web_topublish
zip -r ../mona_web_topublish.zip . >/dev/null
cd ..

cd ./turn_api_topublish
zip -r ../turn_api_topublish.zip . >/dev/null
cd ..

dotnet publish -c Release -o ./relay_topublish ../Mona.SaaS.TurnstileRelay/Mona.SaaS.TurnstileRelay.csproj &

build_relay_pid=$!

dotnet publish -c Release -o ./turn_web_topublish ./turnstile/Turnstile/Turnstile.Web/Turnstile.Web.csproj &

build_turn_web_pid=$!

wait $build_relay_pid
wait $build_turn_web_pid

cd ./relay_topublish
zip -r ../relay_topublish.zip . >/dev/null
cd ..

cd ./turn_web_topublish
zip -r ../turn_web_topublish.zip . >/dev/null
cd ..

echo "☁️   Deploying apps..."

az webapp deployment source config-zip \
    --resource-group "$resource_group_name" \
    --name "$mona_web_app_name" \
    --src "./mona_web_topublish.zip" &

deploy_mona_web_pid=$!

az functionapp deployment source config-zip \
    --resource-group "$resource_group_name" \
    --name "$relay_app_name" \
    --src "./relay_topublish.zip" &

deploy_relay_pid=$!

az functionapp deployment source config-zip \
    --resource-group "$resource_group_name" \
    --name "$turn_api_app_name" \
    --src "./turn_api_topublish.zip" &

deploy_turn_api_pid=$!

az webapp deployment source config-zip \
    --resource-group "$resource_group_name" \
    --name "$turn_web_app_name" \
    --src "./turn_web_topublish.zip"

deploy_turn_web_pid=$!

wait $deploy_mona_web_pid
wait $deploy_relay_pid
wait $deploy_turn_api_pid
wait $deploy_turn_web_pid

echo "🔌   Connecting Mona to Turnstile relay to event grid topic ..."

az eventgrid event-subscription create \
    --name "relay-connection" \
    --source-resource-id "$event_grid_topic_id" \
    --endpoint "$relay_app_id/functions/Relay" \
    --endpoint-type azurefunction

clean_up

echo "🏁   Mona + Turnstile deployment complete. It took [$SECONDS] seconds."
echo "➡️   Go to [ https://$mona_web_app_name.azurewebsites.net/setup ] to complete Mona setup, then..."
echo "    go to [ https://$turn_web_app_name.azurewebsites.net/publisher/setup ] to complete Turnstile setup."
echo

