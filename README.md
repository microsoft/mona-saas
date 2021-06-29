# Mona SaaS

![CI Build](https://github.com/microsoft/mona-saas/actions/workflows/dotnet.yml/badge.svg)

> ⚠ __WARNING__ | Mona SaaS is currently in private preview. We do not yet recommend it for production scenarios.

 Mona SaaS is a [__M__]arketplace [__On__]boarding [__A__]ccelerator designed to make it easier for Microsoft's [ISV partners](https://partner.microsoft.com/community/my-partner-hub/isv) to rapidly onboard transactable SaaS solutions to [Azure Marketplace](https://azure.microsoft.com/marketplace) and [AppSource](https://appsource.microsoft.com). It includes lightweight, reusable code modules that ISVs deploy in their own Azure subscription, and [low/no-code integration templates](https://azure.microsoft.com/en-us/solutions/low-code-application-development) featuring [Azure Logic Apps](https://azure.microsoft.com/services/logic-apps).

 ## How does Mona SaaS work?

 Mona SaaS implements all of the various customer and publisher (you, the ISV) flows that are required by Microsoft's [SaaS fulfillment APIs](https://docs.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2) including both [the landing page](https://docs.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#purchased-but-not-yet-activated-pendingfulfillmentstart) that customers will see when purchasing your SaaS offer and [the webhook](https://docs.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#implementing-a-webhook-on-the-saas-service) that we use to notify you of [subscription changes](https://docs.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#managing-the-saas-subscription-life-cycle) like [cancellations](https://docs.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#canceled-unsubscribed) and [suspensions](https://docs.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#suspended-suspended).
 
  ![Mona Architecture Overview](docs/images/mona_arch_overview.png)
 
Each of these operations is exposed to your SaaS application by Mona SaaS through events published to [a custom Event Grid topic](https://docs.microsoft.com/azure/event-grid/custom-topics) automatically provisioned during setup. By default, we deploy a set of "stub" Logic Apps into your Azure subscription that are enabled by default and configured to be triggered by these subscription events.
 
 Since Mona SaaS exposes these subscription-related events to your SaaS application through an Event Grid topic, [you have lots of options for handling them](https://docs.microsoft.com/azure/event-grid/overview#event-handlers). Because we're using Event Grid, multiple event subscribers can handle the same events simultaneously. These flows can be easily modified in production with no downtime.

## How do I get started with Mona SaaS?

### Prerequisites

First, ensure that the following prerequisites are met.

 * You have an active Azure subscription. [If you don't already have one, get one free here](https://azure.microsoft.com/free).
 * You have the ability to create new app registrations within your Azure Active Directory (AAD) tenant. In order to create app registrations, you must be a directory administrator. For more information, see [this article](https://docs.microsoft.com/en-us/azure/active-directory/roles/permissions-reference).
 * You have the ability to create resources and resource groups within the target Azure subscription. Typically, this requires at least [contributor-level access](https://docs.microsoft.com/azure/role-based-access-control/built-in-roles#contributor) to the subscription.

### Gain access to the Mona SaaS GitHub repository
 
During our private preview phase, the Mona SaaS GitHub repository is private.

Per Microsoft policy, you must use two-factor authentication (2FA) to access this repo using your own GitHub credentials. Since the Azure cloud shell doesn't support GitHub 2FA, you'll need to both [enable 2FA for your GitHub account](https://docs.github.com/en/github/authenticating-to-github/securing-your-account-with-two-factor-authentication-2fa) and [generate a personal access token (PAT) with the `repo` scope selected](https://docs.github.com/en/github/authenticating-to-github/keeping-your-account-and-data-secure/creating-a-personal-access-token). Save this PAT in a secure place as you'll need it later (as your password) when cloning this repository into your own Azure environment.

 ### Clone the Mona SaaS GitHub repository

 Navigate to [the Azure portal](https://portal.azure.com) and [launch the Bash cloud shell](https://docs.microsoft.com/azure/cloud-shell/quickstart#start-cloud-shell).
 
 > If this is the first time that you've used the cloud shell, you will be prompted to [create or choose an existing an Azure Files share](https://docs.microsoft.com/azure/cloud-shell/overview#connect-your-microsoft-azure-files-storage).

Run this command from the cloud shell to clone the Mona SaaS repository —

```shell
git clone https://github.com/microsoft/mona-saas
```

Your user name is your GitHub user name. Your password is the PAT generated in the previous section. By default, the Mona SaaS repository will be cloned to a local directory named `mona-saas`. Navigate to the setup folder by running the following command —

```shell
cd ./mona-saas/Mona.SaaS/Mona.SaaS.Setup
```

Finally, enable the setup script to be executed locally by running —

```shell
chmod +x ./basic-deploy.sh
```

### Set up Mona SaaS

At a minimum, you need this information before running the setup script —

* [The Azure region](https://azure.microsoft.com/global-infrastructure/geographies/) in which you wish to deploy Mona SaaS.
    * For a complete list of available regions, run `az account list-locations -o table` from the cloud shell. Be sure to use the region's `Name`, not `DisplayName` or `RegionalDisplayName`.
* An arbitrary, globally-unique name for this Mona deployment.
    * This identifier must consist of lowercase alphanumeric characters (a-z, 0-9) _only_.
    * It must also be between 3 and 13 characters in length.
* A friendly display name for your Mona deployment which your customers will see when authenticating to the landing page that Mona SaaS deploys. Although a display name isn't technically required, it's highly recommended.

#### Setup script examples

To deploy a Mona instance named `monaex01` to the West Europe (`westeurope`) Azure region, you would run the following command from the cloud shell. Note that, since we didn't explicitly provide a display name, Mona will default to using `monaex01` as the display name.

```shell
./basic-deploy.sh -r "westeurope" -n "monaex01"
```

To include the display name `Mona Example 01` explicitly, your would run —

```shell
./basic-deploy.sh -r "westeurope" -n "monaex01" -d "Mona Example 01"
```

The setup script supports additional optional parameters detailed in the table below. For example, if you wish to deploy Mona into a specific Azure subscription (`9897b07c-86fa-4779-92e3-6273664ec722`) and resource group (`monaex01group`), you can run —

```shell
# Broken down into multiple lines for readability...

./basic-deploy.sh \
   -r "westeurope" \
   -n "monaex01" \
   -d "Mona Example 01" \
   -g "monaex01group" \
   -s "9897b07c-86fa-4779-92e3-6273664ec722"
```

#### Setup script parameters

| Switch | Name | Required | Default | Notes |
| --- | --- | --- | --- | --- |
| `-n` | Deployment name | __Yes__ | N/A | A user-defined, globally-unique name for this Mona SaaS deployment. The deployment name must contain _only_ alphanumeric characters and be 13 characters in length or less. |
| `-r` | Deployment region | __Yes__ | N/A | [The Azure region](https://azure.microsoft.com/global-infrastructure/geographies/) to which Mona SaaS should be deployed. For a complete list of available regions, run `az account list-locations -o table` from the Azure CLI (Bash cloud shell). Be sure to use the region's `Name`, not `DisplayName` or `RegionalDisplayName`.
| `-d` | Display name | No | Same as deployment name (`-n`) | A "friendly" display name for this Mona SaaS deployment. If provided, this is also the name of the Azure Active Directory (AAD) app created during setup. __While providing this parameter isn't required, it's highly recommended.__ |
| `-a` | Existing App Service plan ID | No | N/A | The complete resource ID (i.e., `/subscriptions/{subscriptionId}/...`) of an existing [App Service plan](https://docs.microsoft.com/azure/app-service/overview-hosting-plans) to publish the Mona web app to. If provided, the App Service plan must exist in the same region (see `-l`) and Azure subscription (see `-s`) where Mona is being deployed. If not provided, the setup script will automatically provision a new App Service plan (S1) within the same resource group (see `-r`) where Mona is being deployed. It must also be a Windows-based App Service plan. |
| `-g` | Deployment Azure resource group name | No | `mona-[deployment name (-n)]` | The Azure resource group to deploy Mona SaaS into. If the resource group already exists, it must be empty. If the group doesn't exist, it will be automatically created during setup. |
| `-s` | Deployment Azure subscription ID | No | The current subscription | The ID of the Azure subscription to deploy Mona SaaS into. |
| `-h` | __Flag__ - Don't show script splash screen. | No | N/A | When set, the setup script will not display the standard Mona setup splash screen. |
| `-p` | __Flag__ - Don't publish the web app. | No | N/A | When set, the setup script will provision all Azure and Azure Active Directory reources as usual _but_ won't actually publish the Mona web app. |

### Complete Mona SaaS setup

Once the script is finished, note the information provided in the `Mona Deployment Summary`. We strongly recommend saving these values somewhere safe and convenient as you will likely need to refer to them again later.

Locate the setup URL at the _very bottom_ of the script output. It will look similiar to this —

```shell
https://mona-web-monaex01.azurewebsites.net/setup

# Where "monaex01" is the Mona deployment name.
```

Click the URL (it's automatically linked within the cloud shell) to navigate to that site and complete the Mona SaaS setup wizard.

> The setup wizard is hosted entirely within your own Mona SaaS deployment so you're aren't sharing any information with Microsoft (or anyone else) at this point.

### Finish setting up your offer(s) in Partner Center

[Use the Partner Center to configure your offer(s) and begin transacting with Microsoft!](https://docs.microsoft.com/azure/marketplace/create-new-saas-offer)

## How much does Mona SaaS cost?

Mona SaaS is open source (see [our license](./LICENSE.txt)) and free to use.

Since Mona SaaS is deployed into your Azure environment, the only costs that you're responsible for are those of hosting its supporting Azure resources. These resources include —

* An S1 (Standard) [App Service Plan](https://docs.microsoft.com/azure/app-service/overview-hosting-plans) ([Pricing](https://azure.microsoft.com/pricing/details/app-service/windows/))
    * Note that you can deploy Mona SaaS to a compatible existing App Service Plan using the [`-a` setup script parameter](#setup-script-parameters).
* An [Event Grid topic](https://docs.microsoft.com/en-us/azure/event-grid/custom-topics) ([Pricing](https://azure.microsoft.com/pricing/details/event-grid/))
* Six (6) independent [Azure Logic Apps](https://docs.microsoft.com/en-us/azure/logic-apps/logic-apps-overview) preconfigured to handle different types of Marketplace events (Consumption Plan | [Pricing](https://azure.microsoft.com/en-us/pricing/details/logic-apps/))
* A Basic Integration Account ([Pricing](https://azure.microsoft.com/en-us/pricing/details/logic-apps/))
* A locally-redundant (LRS) standard (GPv2) storage account ([Pricing](https://azure.microsoft.com/pricing/details/storage/blobs/))

Your actual costs may vary based on the following —

 * __The integrations that you build.__ For example, Logic Apps offers [a growing list of Standard and Enterprise connectors](https://docs.microsoft.com/en-us/azure/connectors/apis-list) that allow you to easily access various cloud-based and on-premises services. [The connectors that you use have a direct impact on your overall Azure costs](https://azure.microsoft.com/en-us/pricing/details/logic-apps/) outside of the base Mona SaaS deployment.
 * __Where you deploy Mona SaaS.__ Be aware that costs for the same Azure services can vary across regions.
 * __Any special pricing arrangements you have with Microsoft.__ Many of the ISVs that we work with have special pricing arrangements through their partnerships with Microsoft.

Use the [Azure pricing calculator](https://azure.microsoft.com/pricing/calculator/) to better understand your unique costs.

For help in forecasting your Mona SaaS costs [see this article](https://docs.microsoft.com/en-us/azure/cost-management-billing/costs/cost-analysis-common-uses#view-forecasted-costs).

## Dependencies

Mona SaaS takes a dependency on the open source [Commercial Marketplace .NET Client (`commercial-marketplace-client-dotnet`)](https://github.com/microsoft/commercial-marketplace-client-dotnet). This library's DLL is conveniently included in the Mona SaaS repository.

All other dependencies are automatically satisfied using [Nuget](https://docs.microsoft.com/nuget/what-is-nuget) during the Mona SaaS setup process. For more information on Mona SaaS' dependencies, [check out our dependency graph](https://github.com/microsoft/mona-saas/network/dependencies).

## Who supports Mona SaaS?

> ⚠ __WARNING__ | Mona SaaS is currently in private preview. We do not yet recommend it for production scenarios.

Please see [our support docs](SUPPORT.md) for more information.

## Security

Please see [our security docs](SECURITY.md) for more information.

## Considerations and limitations

* Due to various Azure resource limitations, you can only have one Mona SaaS deployment per Azure region at a time. We plan on addressing this in future releases.
* The deployment name (`-n` setup script switch) mentioned in [this section](#setup-script-parameters) _must be_ globally unique.

## How can I contribute?

Please refer to these docs for more information.

* [__Start Here__: Contibuting Guide](./CONTRIBUTING.md)
* [Microsoft Open Source Code of Conduct](./CODE_OF_CONDUCT.md)
* [Security](./SECURITY.md)

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general). Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.
