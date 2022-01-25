#!/bin/bash

THIS_MONA_VERSION="1.0"

upgrade_mona_rg() {
    local subscription_id="$1"
    local rg_name="$2"
    local deployment_name="$3"
    local web_app_name="mona-web-$deployment_name"

    local web_app_id=$(az resource show \
        --resource-group "$rg_name" \
        --subscription "$subscription_id" \
        --name "$web_app_name" \
        --query "id" \
        --output "tsv")

    if [[ -z "$web_app_id" ]]; then

        # There should be a web app here but there isn't. Let the user know then bail early...

        echo "Expected Mona web app [$web_app_name] not found in resource group [$rg_name]. Upgrade failed." >&2

        return 1
    else

        # Alright so we're doing this...

        local upgrade_slot_name="mona-upgrade-$(date +%s)"

        # Create a temporary slot to push the new version of Mona to...

        echo "Creating temporary upgrade deployment slot [$upgrade_slot_name]..."

        az webapp deployment slot create \
            --name "$web_app_name" \
            --resource-group "$rg_name" \
            --slot "$upgrade_slot_name" \
            --configuration-source "$web_app_name" \
            --subscription "$subscription_id"

        
    fi
}

echo "Scanning subscriptions for upgradeable Mona deployments..."
echo

subscriptions_ids=$(az account subscription list \
    --query "[].subscriptionId" \
    --output "tsv")

for subscription_id in subscription_ids; do

    echo "Scanning subscription [$subscription_id]..."

    # If a resource group has a "Mona Version" tag, we can be pretty confident 
    # that it's a Mona deployment. Get all the resource groups that we can touch
    # that probably contain a Mona deployment.

    mona_rg_names=$(az group list \
        --subscription "$subscription_id" \
        --query "[?tags.\"Mona Version\" != null].name" \
        --output "tsv")

    for mona_rg_name in mona_rg_names; do

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

            echo
            echo "Azure resource group [$mona_rg_name] in subscription [$subscription_id] is tagged with Mona version [$rg_mona_version] but has no \"Deployment Name\" tag. Unable to upgrade Mona without a deployment name." >&2

        elif [[ "$THIS_MONA_VERSION" -ne "$rg_mona_version" ]]; then # Different Mona version so potentially upgradeable.

            # TODO: Add additional logic here to actually compare Mona versions to prevent downgrade?

            echo
            echo "Potentially upgradeable Mona deployment found."
            echo
            echo "Deployment Name:      [$rg_mona_name]"
            echo "Azure Subscription:   [$subscription_id]"
            echo "Resouce Group:        [$mona_rg_name]"
            echo
            echo "Current Version:      [$rg_mona_version]"
            echo "Upgrade to Version?:  [$THIS_MONA_VERSION]"
            echo

            read -p "Upgrade Mona deployment [$rg_mona_name] to version [$THIS_MONA_VERSION]? [y/N]" confirm_upgrade

            case "$confirm_upgrade" in
                [yY1]   ) upgrade_mona_rg "$subscription_id" "$mona_rg_name" "$rg_mona_name";; # We have a winner!
                *       ) ;;
            esac
        fi
    done
done

