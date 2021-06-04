# Mona SaaS (Pilot)

![CI Build](https://github.com/microsoft/mona-saas/actions/workflows/dotnet.yml/badge.svg)

> ⚠ __WARNING__ | Mona SaaS is currently in pilot. We do not yet recommend it for production scenarios.

 Mona SaaS is a [__M__]arketplace [__On__]boarding [__A__]ccelerator designed to make it easier for [our ISV partners](https://partner.microsoft.com/community/my-partner-hub/isv) to rapidly onboard their transactable SaaS solutions to the [Azure Marketplace](https://azure.microsoft.com/marketplace) and [AppSource](https://appsource.microsoft.com). Mona SaaS accomplishes this through lightweight, reusable code modules deployed directly into the ISV's own Azure subscription and [low/no-code integration templates](https://azure.microsoft.com/en-us/solutions/low-code-application-development) featuring [Azure Logic Apps](https://azure.microsoft.com/services/logic-apps).

 Read further to learn more.

* [How does Mona SaaS work?](#how-does-mona-saas-work)
* [How much does Mona SaaS cost?](#how-much-does-mona-saas-cost)
* [How do I set up Mona SaaS?](#how-do-i-set-up-mona-saas)
* [How can I contribute?](#how-can-i-contribute)
* [Considerations and limitations](#considerations-and-limitations)
* [Trademarks](#trademarks)

 ## How does Mona SaaS work?

 This module implements all of the various customer and publisher (you, the ISV) flows that are required by our own [SaaS fulfillment APIs](https://docs.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2) including both [the landing page](https://docs.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#purchased-but-not-yet-activated-pendingfulfillmentstart) that customers will see when purchasing your SaaS offer and [the webhook](https://docs.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#implementing-a-webhook-on-the-saas-service) that we use to notify you of [subscription changes](https://docs.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#managing-the-saas-subscription-life-cycle) like [cancellations](https://docs.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#canceled-unsubscribed) and [suspensions](https://docs.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#suspended-suspended).
 
  ![Mona Architecture Overview](docs/images/mona_arch_overview.png)
 
Each of these operations is exposed to your SaaS application by Mona SaaS through events published to [a custom Event Grid topic](https://docs.microsoft.com/azure/event-grid/custom-topics) automatically provisioned during setup. By default, we deploy a set of "stub" Logic Apps into your Azure subscription that are enabled by default and configured to be triggered by these subscription events.
 
 Since Mona SaaS exposes these subscription-related events to your SaaS application through an Event Grid topic, [you have lots of options for handling them](https://docs.microsoft.com/azure/event-grid/overview#event-handlers). Because we're using Event Grid, multiple event subscribers can handle the same events simultaneously. These flows can be easily modified in production with no downtime.

 ## How much does Mona SaaS cost?

 As an open source project, Mona SaaS itself is free to use (within the confines of [our license](./LICENSE.txt), of course).
 
 As Mona SaaS is designed to be deployed directly into an ISV's subscription, the ISV is ultimately responsible for the costs of Azure services. Use the [Azure pricing calculator](https://azure.microsoft.com/pricing/calculator/) for cost guidance 
 
 * Azure App Service plan
 * Azure Logic Apps
 * Application Insights
 * Azure Storage
 * Azure EventGrid

Your actual costs may vary depending on a number of factors including —
 * __The integrations that you build.__ For example, Logic Apps offers [a growing list of Standard and Enterprise connectors](https://docs.microsoft.com/en-us/azure/connectors/apis-list) that allow you to easily access various cloud-based and on-premises services. [The connectors that you use have a direct impact on your overall Azure costs](https://azure.microsoft.com/en-us/pricing/details/logic-apps/) outside of the base Mona SaaS deployment.
 * __Where you deploy Mona SaaS.__ Be aware that costs for the same Azure services can vary across regions.
 * __Any special pricing arrangements you have with Microsoft.__ Many of the ISVs that we work with have special pricing arrangements through their partnerships with Microsoft.

 For help in forecasting your Mona SaaS costs, [see this article](https://docs.microsoft.com/en-us/azure/cost-management-billing/costs/cost-analysis-common-uses#view-forecasted-costs).

 To learn more about Azure pricing in general, [visit this site](https://azure.microsoft.com/pricing).

 ## How do I set up Mona SaaS?

First, ensure that the following prerequisites are met.

 * You have an active Azure subscription. [If you don't already have one, get one free here](https://azure.microsoft.com/free).
 * You have the ability to create new app registrations within your Azure Active Directory (AAD) tenant. In order to create app registrations, you must be a directory administrator. For more information, see [this article](https://docs.microsoft.com/en-us/azure/active-directory/roles/permissions-reference).
 * You have the ability to create resources and resource groups within the target Azure subscription. Typically, this requires at least [contributor-level access](https://docs.microsoft.com/azure/role-based-access-control/built-in-roles#contributor) to the subscription.

 Next, follow the instructions below.
 
 > During pilot, this repository is private. Per Microsoft policy, you must use two-factor authentication (2FA) to access this repo using your own Github credentials. Since the Azure cloud shell doesn't support Github 2FA, you'll need to both [enable 2FA for your Github account](https://docs.github.com/en/github/authenticating-to-github/securing-your-account-with-two-factor-authentication-2fa) and [generate a personal access token (PAT) with the `repo` scope selected](https://docs.github.com/en/github/authenticating-to-github/keeping-your-account-and-data-secure/creating-a-personal-access-token). Save this PAT in a secure place as you'll need it later (as your password) when cloning this repository into your own Azure environment.

 1. Navigate to [the Azure portal](https://portal.azure.com) and [launch the Bash cloud shell](https://docs.microsoft.com/azure/cloud-shell/quickstart#start-cloud-shell).
 2. Clone the Mona SaaS Github repository into your own Azure environment by running `git clone https://github.com/microsoft/mona-saas` from the cloud shell.
    * During our pilot phase, you will need to provide your Github credentials at this step.
3. Once the repository has been cloned, navigate to the `./Mona.SaaS/Mona.SaaS.Setup/` folder within your newly cloned Mona repository and ensure that the setup script is locally executable by running `chmod +x ./basic-deploy.sh` from the cloud shell.
4. Run [the Mona SaaS setup script](./Mona.SaaS/Mona.SaaS.Setup/basic-deploy.sh).
    * To deploy a Mona SaaS instance named `"monadocs01"` (display name is `"Mona Docs 01"`) into the North Europe (`"northeurope"`) region, you would run `./basic-deploy.sh -r "northeurope" -n "monadocs01" -d "Mona Docs 01"` from the cloud shell.
    * Refer to [this table](#setup-script-parameters) for more information on Mona SaaS setup script parameters.
    * Setup takes approximately five minutes. Grab a cup of coffee. ☕
5. Once the script is finished, note the information provided in the `Mona Deployment Summary`. We recommend saving these values somewhere convenient as you will likely need them later.
6. Find the setup URL at the _very bottom_ of the script output. It should look similiar to this - `"https://your-mona-web.azurewebsites.net/setup"`. Navigate to that site in your browser to complete setup.
7. [Use the Partner Center to finish onboarding your SaaS solution and begin transacting with Microsoft!](https://docs.microsoft.com/azure/marketplace/create-new-saas-offer)


### Setup script parameters

| Switch | Name | Required | Default | Notes |
| --- | --- | --- | --- | --- |
| `-n` | Deployment name | __Yes__ | N/A | A user-defined, globally-unique name for this Mona SaaS deployment. The deployment name must contain _only_ alphanumeric characters and be 13 characters in length or less. |
| `-r` | Deployment region | __Yes__ | N/A | [The Azure region](https://azure.microsoft.com/global-infrastructure/geographies/) to which Mona SaaS should be deployed. For a complete list of available regions, run `az account list-locations -o table` from the Azure CLI (Bash cloud shell). Be sure to use the region's `Name`, not `DisplayName` or `RegionalDisplayName`.
| `-d` | Display name | No | Same as deployment name (`-n`) | A "friendly" display name for this Mona SaaS deployment. If provided, this is also the name of the Azure Active Directory (AAD) app created during setup. __While providing this parameter isn't required, it's highly recommended.__ |
| `-a` | Existing App Service plan ID | No | N/A | The complete resource ID (i.e., `/subscriptions/{subscriptionId}/...`) of an existing [App Service plan](https://docs.microsoft.com/azure/app-service/overview-hosting-plans) to publish the Mona web app to. If provided, the App Service plan must exist in the same region (see `-l`) and Azure subscription (see `-s`) where Mona is being deployed. If not provided, the setup script will automatically provision a new App Service plan (S1) within the same resource group (see `-r`) where Mona is being deployed. It must also be a Windows-based App Service plan. |
| `-g` | Deployment Azure resource group name | No | `mona-[deployment name (-n)]` | The Azure resource group to deploy Mona SaaS into. If the resource group already exists, it must be empty. If the group doesn't exist, it will be automatically created during setup. |
| `-l` | UI language | No | English (`en`) | The ISV's preferred UI language. Currently, only English (`en`) and Spanish (`es`) are supported. |
| `-s` | Deployment Azure subscription ID | No | The current subscription | The ID of the Azure subscription to deploy Mona SaaS into. |
| `-h` | __Flag__ - Don't show script splash screen. | No | N/A | When set, the setup script will not display the standard Mona setup splash screen. |
| `-p` | __Flag__ - Don't publish the web app. | No | N/A | When set, the setup script will provision all Azure and Azure Active Directory reources as usual _but_ won't actually publish the Mona web app. |

## How can I contribute?

Please refer to these docs for more information.

* [__Start Here__: Contibuting Guide](./CONTRIBUTING.md)
* [Microsoft Open Source Code of Conduct](./CODE_OF_CONDUCT.md)
* [Security](./SECURITY.md)

## Dependency

Mona SaaS uses Open Source [commercial-marketplace-client-dotnet](https://github.com/microsoft/commercial-marketplace-client-dotnet).  It is currently not distributed via NuGet so dll is conveniently included in the repo.

## Considerations and limitations

* Due to various Azure resource limitations, you can only have one Mona SaaS deployment per Azure region at a time. We plan on addressing this in future releases.
* The deployment name (`-n` setup script switch) mentioned in [this section](#setup-script-parameters) _must be_ globally unique.

## Security

[Reporting security issues](SECURITY.md)


## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general). Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.
