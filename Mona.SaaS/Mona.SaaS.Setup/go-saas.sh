#!/bin/bash

SECONDS=0 # Let's time it...

usage() { echo "Usage: $0 <-n name> <-r deployment_region> [-d display_name] [-i integration_pack]"; }

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

# Log in the user if they aren't already...

while [[ -z $current_user_oid ]]; do
    current_user_oid=$(az ad signed-in-user show --query objectId --output tsv 2>/dev/null);
    if [[ -z $current_user_oid ]]; then az login; fi;
done

# Get our parameters...

while getopts "n:r:d:i:" opt; do
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
        i)
            p_integration_pack=$OPTARG
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

if [[ -z $p_display_name ]]; then
    display_name="$p_deployment_name"
else
    display_name=p_display_name
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

mona_aad_app_name="$display_name"
mona_aad_app_secret=$(openssl rand -base64 64)

echo "🛡️   Creating Mona Azure Active Directory (AAD) app [$mona_aad_app_name] registration..."

mona_aad_app_id=$(az ad app create \
    --display-name "$mona_aad_app_name" \
    --available-to-other-tenants true \
    --end-date "2299-12-31" \
    --password "$mona_aad_app_secret" \
    --optional-claims @./aad/manifest.optional_claims.json \
    --required-resource-accesses @./aad/manifest.resource_access.json \
    --app-roles @./aad/manifest.app_roles.json \
    --query appId \
    --output tsv);

turn_aad_app_name="$display_name Seating"
turn_aad_app_secret=$(openssl rand -base64 64)

echo "🛡️   Creating Turnstile Azure Active Directory (AAD) app [$turn_aad_app_name] registration..."

turn_aad_app_id=$(az ad app create \
    --display-name "$turn_aad_app_name" \
    --available-to-other-tenants true \
    --end-date "2299-12-31" \
    --password "$turn_aad_app_secret" \
    --optional-claims @./aad/turnstile/manifest.optional_claims.json \
    --required-resource-accesses @./aad/turnstile/manifest.resource_access.json \
    --app-roles @./aad/turnstile/manifest.app_roles.json \
    --query appId \
    --output tsv);

echo "🛡️   Creating Mona AAD app [$mona_aad_app_name] service principal..."

mona_aad_sp_id=$(az ad sp create --id "$mona_aad_app_id" --query objectId --output tsv);

if [[ -z $mona_aad_sp_id ]]; then
    echo "$lp ❌   Unable to create service principal for Mona AAD app [$mona_aad_app_name ($mona_aad_app_id)]. See above output for details. Setup failed."
    exit 1
fi

echo "🛡️   Creating Turnstile AAD app [$turn_aad_app_name] service principal..."

turn_aad_sp_id=$(az ad sp create --id "$turn_aad_app_id" --query objectId --output tsv);

if [[ -z $turn_aad_sp_id ]]; then
    echo "$lp ❌   Unable to create service principal for Turnstile AAD app [$turn_aad_app_name ($turn_aad_app_id)]. See above output for details. Setup failed."
    exit 1
fi

echo "🔐   Granting Mona AAD app [$mona_aad_app_name] service principal [$mona_aad_sp_id] contributor access to resource group [$resource_group_name]..."

az role assignment create \
    --role "Contributor" \
    --assignee "$mona_aad_sp_id" \
    --resource-group "$resource_group_name"

echo "🔐   Granting Turnstile AAD app [$turn_aad_app_name] service principal [$turn_aad_sp_id] contributor access to resource group [$resource_group_name]..."

az role assignment create \
    --role "Contributor" \
    --assignee "$turn_aad_sp_id" \
    --resource-group "$resource_group_name"

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
