#!/bin/bash

# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

THIS_MONA_VERSION=$(cat ../../VERSION)

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
    dotnet --version >/dev/null

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

    check_az;       if [[ $? -ne 0 ]]; then prereq_check_failed=1; fi;
    check_dotnet;   if [[ $? -ne 0 ]]; then prereq_check_failed=1; fi;

    if [[ -z $prereq_check_failed ]]; then
        echo "✔   All Mona upgrade prerequisites installed."
    else
        return 1
    fi
}

check_mona_health() {
    local deployment_name="$1"
    local web_app_name="$2"

    local health_status=""

    for i in {1..6}; do
        sleep 10

        health_status=$(curl -s -o /dev/null -w "%{http_code}" "https://$web_app_name.azurewebsites.net/health")

        echo "🩺   Checking Mona deployment [$deployment_name] health (attempt $i of 6)..."

        if [[ $health_status == "200" ]]; then
            echo "✔   Mona deployment [$deployment_name] is healthy (HTTP $health_status)!"
            return 0 # All good!
        fi
    done
    
    # If we got this far, something's definitely not right...

    echo "⚠️   Mona deployment [$deployment_name] is unhealthy (HTTP $health_status)."
    return 1
}

upgrade_mona_rg() {
    local subscription_id="$1"
    local rg_name="$2"
    local deployment_name="$3"
    local web_app_name="mona-web-$deployment_name"

    local web_app_id=$(az resource show \
        --resource-group "$rg_name" \
        --resource-type "Microsoft.Web/sites" \
        --subscription "$subscription_id" \
        --name "$web_app_name" \
        --query "id" \
        --output "tsv")

    if [[ -z "$web_app_id" ]]; then

        # There should be a web app here but there isn't. Let the user know then bail early...

        echo
        echo "⚠️   Expected app service [$web_app_name] not found in resource group [$rg_name]. Upgrade failed." >&2

        return 1
    else
        # Alright so we're doing this...

        local upgrade_slot_name="mona-upgrade-$(date +%s)"

        # Create a temporary slot to push the new version of Mona to...

        echo
        echo "🏗️   Creating app service [$web_app_name] temporary upgrade slot [$upgrade_slot_name]..."

        az webapp deployment slot create \
            --name "$web_app_name" \
            --resource-group "$rg_name" \
            --slot "$upgrade_slot_name" \
            --configuration-source "$web_app_name" \
            --subscription "$subscription_id"

        if [[ $? -ne 0 ]]; then
            # We couldn't create a deployment slot which means that we can't do this upgrade
            # safely which means that we're not going to try to do it at all. I mean, this thing
            # is probably running in production. More than likely, the app service that Mona
            # is deployed to doesn't support deployment slots (< Standard).

            echo
            echo "⚠️  Unable to create temporary upgrade slot. Can not safely perform upgrade. Please ensure that your App Service Plan SKU is Standard (S1) or higher. For more information, see [ https://docs.microsoft.com/azure/azure-resource-manager/management/azure-subscription-service-limits#app-service-limits ]."
            
            return 1
        fi

        # Alright, let's build the new version of Mona...

        echo
        echo "📦   Packaging new Mona web app for deployment to app service [$web_app_name] temporary upgrade slot [$upgrade_slot_name]..."

        dotnet publish -c Release -o ./topublish ../Mona.SaaS.Web/Mona.SaaS.Web.csproj
        cd ./topublish
        zip -r ../topublish.zip . >/dev/null
        cd ..

        echo
        echo "☁️   Deploying upgraded Mona web app to app service [$web_app_name] temporary upgrade slot [$upgrade_slot_name]..."

        az webapp deployment source config-zip \
            --src ./topublish.zip \
            --name "$web_app_name" \
            --resource-group "$rg_name" \
            --slot "$upgrade_slot_name" \
            --subscription "$subscription_id"

        # Clean up after ourselves...

        rm -rf ./topublish >/dev/null
        rm -rf ./topublish.zip >/dev/null

        echo
        echo "🔃   Upgraded Mona web app has been deployed to app service [$web_app_name] deployment slot [$upgrade_slot_name]. Swapping production slot with upgraded deployment slot [$upgrade_slot_name]..."

        az webapp deployment slot swap \
            --slot "$upgrade_slot_name" \
            --action "swap" \
            --name "$web_app_name" \
            --resource-group "$rg_name" \
            --subscription "$subscription_id" \
            --target-slot "production"

        echo
        echo "✔   Upgraded Mona web app promoted to production slot."

        # Let's try to see if the upgraded Mona deployment is healthy...

        check_mona_health "$deployment_name" "$web_app_name"

        if [[ $? == 0 ]]; then # Score! Mona deployment is healthy. Let's make this official...
            echo "✔   Upgrade successful."
            commit_upgrade      "$upgrade_slot_name" "$web_app_name" "$rg_name" "$subscription_id"
        else # Something broke. We need to roll it back.
            echo "❌  Upgrade failed."
            rollback_upgrade    "$upgrade_slot_name" "$web_app_name" "$rg_name" "$subscription_id"
        fi
    fi
}

commit_upgrade() {
    local upgrade_slot_name=$1
    local web_app_name=$2
    local rg_name=$3
    local subscription_id=$4

    echo
    echo "🏷️   Tagging Mona resource group [$rg_name] with updated Mona version..."

    az tag update \
        --resource-id "/subscriptions/$subscription_id/resourcegroups/$rg_name" \
        --operation "merge" \
        --tags "Mona Version"="$THIS_MONA_VERSION" \

    echo
    echo "🧹   Deleting app service [$web_app_name] temporary upgrade slot [$upgrade_slot_name]..."

    az webapp deployment slot delete \
        --slot "$upgrade_slot_name" \
        --name "$web_app_name" \
        --resource-group "$rg_name" \
        --subscription "$subscription_id"

    echo
    echo "✔   Upgrade to [$THIS_MONA_VERSION] complete."
}

rollback_upgrade() {
    local upgrade_slot_name=$1
    local web_app_name=$2
    local rg_name=$3
    local subscription_id=$4

    echo
    echo "🔙  Rolling Mona back to previous version..."

    az webapp deployment slot swap \
        --slot "$upgrade_slot_name" \
        --action "swap" \
        --name "$web_app_name" \
        --resource-group "$rg_name" \
        --subscription "$subscription_id" \
        --target-slot "production"

    echo
    echo "🪲   Leaving unhealthy Mona deployment in slot [$upgrade_slot_name] for debugging purposes."
}

cat ./splash.txt
echo

check_prereqs

if [[ $? -ne 0 ]]; then
    echo "❌   Please install all Mona setup prerequisites then try again. Setup failed."
    exit 1
fi

echo
echo "🔍   Scanning accessible subscriptions for upgradeable Mona deployments..."

subscription_ids=$(az account subscription list \
    --query "[].subscriptionId" \
    --output "tsv")

for subscription_id in $subscription_ids; do

    echo "🔍   Scanning subscription [$subscription_id]..."

    # If a resource group has a "Mona Version" tag, we can be pretty confident 
    # that it's a Mona deployment. Get all the resource groups that we can touch
    # that probably contain a Mona deployment.

    mona_rg_names=$(az group list \
        --subscription "$subscription_id" \
        --query "[?tags.\"Mona Version\" != null].name" \
        --output "tsv")

    for mona_rg_name in $mona_rg_names; do

        # Get this resource group's Mona version.

        rg_mona_version=$(az group show \
            --name "$mona_rg_name" \
            --subscription "$subscription_id" \
            --query "tags.\"Mona Version\"" \
            --output "tsv")

        # Get this resource group's Mona deployment name.

        rg_mona_name=$(az group show \
            --name "$mona_rg_name" \
            --subscription "$subscription_id" \
            --query "tags.\"Deployment Name\"" \
            --output "tsv")

        if [[ -z $rg_mona_name ]]; then

            # Well, this is awkward. Somehow, we got in a situation where we have a resource group that 
            # presumably contains a Mona deployment but doesn't have a "Deployment Name" tag. We need to 
            # know the name of the deployment in order to upgrade it so we'll have to skip this one. Bummer.

            echo "⚠️   Azure resource group [$mona_rg_name (subscription: $subscription_id)] is tagged with Mona version [$rg_mona_version] but has no \"Deployment Name\" tag. Unable to upgrade Mona without a deployment name." >&2

        elif [[ "$THIS_MONA_VERSION" != "$rg_mona_version" ]]; then # Different Mona version so potentially upgradeable.

            # Comparing semantic versions is difficult and gets even fuzzier when you're trying to compare
            # versions that contain suffixes like -prerelease. I was thinking about specifically breaking out 
            # the version number into resource group tags (a major, minor, and patch tag) but, for now, we'll
            # just have the user confirm that they want to upgrade a deployment if the version number is different
            # than the current version number.

            echo
            echo "❕   Potentially upgradeable Mona deployment found."
            echo
            echo "Deployment Name:      [$rg_mona_name]"
            echo "Azure Subscription:   [$subscription_id]"
            echo "Resouce Group:        [$mona_rg_name]"
            echo
            echo "Current Version:      [$rg_mona_version]"
            echo "Upgrade to Version?:  [$THIS_MONA_VERSION]"
            echo

            read -p "❔   Upgrade Mona deployment [$rg_mona_name] to version [$THIS_MONA_VERSION]? [y/N] " initiate_upgrade

            case "$initiate_upgrade" in
                [yY1]   ) 
                    upgrade_mona_rg "$subscription_id" "$mona_rg_name" "$rg_mona_name" # We have a winner!

                    echo
                    read -p "❔   Keep scanning for upgradeable Mona deployments? [y/N] " keep_scanning

                    case "$keep_scanning" in
                        [yY1]   ) ;;
                        *       ) exit 0 ;;
                    esac
                ;;
                *       )
                ;; # Move along now. Nothing to see here...
            esac
        else
            echo "Mona deployment [$rg_mona_name (subscription: $subscription_id; resource group: $mona_rg_name)] is already version [$THIS_MONA_VERSION]."
        fi
    done
done

