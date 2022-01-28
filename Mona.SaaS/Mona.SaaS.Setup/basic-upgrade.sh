#!/bin/bash

THIS_MONA_VERSION="1.0"

check_az() {
    az version >/dev/null

    # TODO: Should we be more specific about which version of az is required?

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
        echo "$lp ✔   All Mona upgrade prerequisites installed."
    else
        return 1
    fi
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
        echo "Expected Mona web app [$web_app_name] not found in resource group [$rg_name]. Upgrade failed." >&2

        return 1
    else

        # Alright so we're doing this...

        local upgrade_slot_name="mona-upgrade-$(date +%s)"

        # Create a temporary slot to push the new version of Mona to...

        # FYI - We used to do this _right before

        echo
        echo "Creating app service [$web_app_name] temporary deployment slot [$upgrade_slot_name]..."
        echo

        az webapp deployment slot create \
            --name "$web_app_name" \
            --resource-group "$rg_name" \
            --slot "$upgrade_slot_name" \
            --configuration-source "$web_app_name" \
            --subscription "$subscription_id"

        # Alright, let's build the new version of Mona...

        echo
        echo "Packaging new Mona web app for deployment to app service [$web_app_name]..."
        echo

        dotnet publish -c Release -o ./topublish ../Mona.SaaS.Web/Mona.SaaS.Web.csproj
        cd ./topublish
        zip -r ../topublish.zip . >/dev/null
        cd ..

        echo
        echo "Deploying upgraded Mona web app to app service [$web_app_name] temporary deployment slot [$upgrade_slot_name]..."
        echo

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
        echo "Upgraded Mona web app has been deployed to app service [$web_app_name] deployment slot [$upgrade_slot_name]. Swapping production slot with upgraded deployment slot [$upgrade_slot_name]..."

        az webapp deployment slot swap \
            --slot "$upgrade_slot_name" \
            --action "swap" \
            --name "$web_app_name" \
            --resource-group "$rg_name" \
            --subscription "$subscription_id" \
            --target-slot "production"

        echo
        echo "Upgraded Mona web app promoted to production slot."
        echo
        echo "Please visit the upgraded Mona admin center [ https://$web_app_name.azurewebsites.net/admin ] and test landing pages [ https://$web_app_name.azurewebsites.net/test ] to confirm that your upgraded Mona deployment is working as expected."
        echo
        read -p "Is Mona [$deployment_name] working as expected? [y/N] " complete_upgrade

        case "$complete_upgrade" in
            [yY1]   )    
                echo
                echo "Tagging Mona resource group [$rg_name] with updated Mona version..."
                echo
                
                az tag update \
                    --resource-id "/subscriptions/$subscription_id/resourcegroups/$rg_name" \
                    --operation "merge" \
                    --tags "Mona Version"="$THIS_MONA_VERSION"
            ;;
            *       )
                echo
                echo "Sorry to hear that you're having issues with your Mona upgrade. Please visit [ https://github.com/microsoft/mona-saas/discussions ] for assistance. Rolling back Mona [$deployment_name] upgrade..."
                echo

                az webapp deployment slot swap \
                    --slot "$upgrade_slot_name" \
                    --action "swap" \
                    --name "$web_app_name" \
                    --resource-group "$rg_name" \
                    --subscription "$subscription_id" \
                    --target-slot "production"
            ;;
        esac

        echo
        echo "Deleting app service [$web_app_name] deployment slot [$upgrade_slot_name]..."

        az webapp deployment slot delete \
            --slot "$upgrade_slot_name" \
            --name "$web_app_name" \
            --resource-group "$rg_name" \
            --subscription "$subscription_id"

        echo # Alright. Next? 
    fi
}

cat ./spash.txt & echo;

check_prereqs

if [[ $? -ne 0 ]]; then
    echo "$lp ❌   Please install all Mona setup prerequisites then try again. Setup failed."
    exit 1
fi

echo "Scanning subscriptions for upgradeable Mona deployments..."
echo

subscription_ids=$(az account subscription list \
    --query "[].subscriptionId" \
    --output "tsv")

for subscription_id in $subscription_ids; do

    echo "Scanning subscription [$subscription_id]..."

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

            echo "Azure resource group [$mona_rg_name (subscription: $subscription_id)] is tagged with Mona version [$rg_mona_version] but has no \"Deployment Name\" tag. Unable to upgrade Mona without a deployment name." >&2

        elif [[ "$THIS_MONA_VERSION" != "$rg_mona_version" ]]; then # Different Mona version so potentially upgradeable.

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

            read -p "Upgrade Mona deployment [$rg_mona_name] to version [$THIS_MONA_VERSION]? [y/N] " initiate_upgrade

            case "$initiate_upgrade" in
                [yY1]   ) upgrade_mona_rg "$subscription_id" "$mona_rg_name" "$rg_mona_name";; # We have a winner!
                *       ) ;; # Move along now. Nothing to see here...
            esac
        else
            echo "Mona deployment [$rg_mona_name (subscription: $subscription_id; resource group: $mona_rg_name)] is already version [$THIS_MONA_VERSION]."
        fi
    done
done

