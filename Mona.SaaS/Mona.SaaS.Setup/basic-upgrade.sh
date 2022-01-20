#!/bin/bash

usage() { echo "Usage: $0 <-n deployment_name> [-a app_service_id] [-r resource_group_name] [-s subscription_id] [-y]"; }

check_az() {
    exec 3>&2
cd
    az version >/dev/null 2>&1

    # TODO: Should we be more specific about which version of az is required?

    if [[ $? -ne 0 ]]; then
        echo "❌   Please install the Azure CLI before continuing. See [https://docs.microsoft.com/cli/azure/install-azure-cli] for more information."
        return 1
    else
        echo "✔   Azure CLI installed."
    fi
}

check_dotnet() {
    exec 3>&2

    dotnet --version >/dev/null 2>&1

   # TODO: Should we be more specific about which version of dotnet is required?

    if [[ $? -ne 0 ]]; then
        echo "❌   Please install .NET before continuing. See [https://dotnet.microsoft.com/download] for more information."
        return 1
    else
        echo "✔   .NET installed."
    fi
}

check_prereqs() {
    echo "Checking Mona upgrade prerequisites...";

    check_az;          if [[ $? -ne 0 ]]; then prereq_check_failed=1; fi;
    check_dotnet;      if [[ $? -ne 0 ]]; then prereq_check_failed=1; fi;

    if [[ -z $prereq_check_failed ]]; then
        echo "✔   All Mona upgrade prerequisites installed."
    else
        return 1
    fi
}

while getopts "n:a:r:s:y" opt; do
    case $opt in
        n)
            p_deployment_name=$OPTARG
        ;;
        a)
            p_app_service_id=$OPTARG
        ;;
        r)
            p_resource_group_name=$OPTARG
        ;;
        s)
            p_subscription_id=$OPTARG
        ;;
        y)
            p_yes=1
        ;;
        \?)
            usage
            exit 1
        ;;
    esac
done

[[ -z $p_deployment_name ]] && { usage; exit 1; }

eg_app_service_name="mona-web-$p_deployment_name"

echo "Locating existing Mona app service (e.g., \"$eg_app_service_name\") for upgrade..."

if [[ -z $p_app_service_id ]]; then # If a specific app service ID was _not_ provided...
    if [[ -n $p_subscription_id ]]; then # If a specific subscription ID was provided...
        az account set --subscription $p_subscription_id # Try to switch to it if we're not already there.

        if [[ $? -ne 0 ]]; then
            echo "❌   Azure subscription [$p_subscription_id] not found. Upgrade failed."
            exit 1
        fi
    fi

    if [[ -n $p_resource_group_name ]]; then # If a specific resource group name was provided...
        resource_group_name="$p_resource_group_name" # Then use it.
    else
        resource_group_name="mona-$p_deployment_name" # Otherwise, assume the conventional resource group name.
    fi

    app_service_id=$(az resource show --resource-group "$resource_group_name" --name "mona-web-$p_deployment_name" --query id --output tsv)
else
    app_service_id=$(az resource show --ids "$p_app_service_id" --query id --output tsv)
fi

# Alright, so does the app service actually exist?

if [[ -z $app_service_id ]]; then 
    echo "❌   Existing Mona app service (e.g., \"$eg_app_service_name\") not found. Upgrade failed."
    exit 1
else
    echo "Preparing to upgrade Mona web app at [$app_service_id]..."
fi

# Create the upgrade deployment slot if it doesn't already exist...

upgrade_slot_name="mona-upgrade";

if [[ -z $(az webapp deployment slot list --ids "$app_service_id" --query "[?name == '$upgrade_slot_name'].name" --output tsv) ]]; then
    echo "⚠️   Mona app service upgrade slot [$upgrade_slot_name] already exists."

    # TODO: Ask the user if it's cool if we blow away this deployment slot...


fi

# Create the upgrade deployment slot...

echo "Creating app service upgrade slot [$upgrade_slot_name]..."



