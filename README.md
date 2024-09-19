# Mona for SaaS

[The Azure Marketplace is an event-driven system.](https://learn.microsoft.com/partner-center/marketplace-offers/partner-center-portal/pc-saas-fulfillment-life-cycle) Customers purchase subscriptions to your SaaS app. They can change their subscriptions or purchase more seats. Some customers might cancel their subscriptions. These crucial events are the pulse of your organization. Mona embraces this event-driven design by publishing all Marketplace-related events to [a custom Event Grid topic](https://learn.microsoft.com/azure/event-grid/custom-topics) deployed in your environment. [From there, you can handle the events however you want.](https://learn.microsoft.com/azure/event-grid/event-handlers) By default, Mona deploys a set of [Logic Apps](https://learn.microsoft.com/azure/logic-apps/logic-apps-overview) that enable you to control how your SaaS app responds to Marketplace events in a simple low/no-code environment.

## Quickstart

### Clone this repo

[Open an Azure Cloud Shell, choose the Bash experience](https://learn.microsoft.com/azure/cloud-shell/get-started/ephemeral?tabs=azurecli#start-cloud-shell), and clone this repo. Navigate to the setup folder.

```sh
git clone https://github.com/microsoft/mona-saas
cd mona-saas/Mona.SaaS/Mona.SaaS.Setup
```

### Run the setup script

Run the `basic-deploy.sh` script to deploy Mona into your Azure environment. You'll need to provide only two parameters:

| Name | `-` | Description |
| --- | --- | --- |
| **Region** | `r` | [The Azure region](https://azure.microsoft.com/explore/global-infrastructure/geographies/) where Mona should be deployed.For a complete list of Azure regions to choose from, run `az account list-locations -o table` from the Bash Cloud Shell. |
| **Name** | `n` | A unique name for your Mona deployment. It must be 5-13 alphanumeric characters. |

#### Example

```bash
./basic-deploy.sh -r "eastus" -n "monatest01"
```

Once the script is complete, you will be presented with a link to your Mona deployment's admin center. Click the link to finish setting up Mona. Be sure to bookmark the link so you can have quick and easy access to the Mona admin center later. 

> Anyone in your Azure Entra tenant (including both guests and members) have access to the Mona admin center.

### Configure Marketplace event integrations

Navigate to the Mona admin center and click the **"This Mona deployment"** tab. Click on the link to navigate to the resource group in the Azure portal. You will find Logic Apps in this preconfigured to handle various Marketplace subscription events. Open these Logic Apps to configure how your SaaS app should behave in response to subscription events.

Of course, once you've customized the Logic Apps, you'll want to test them...

### Test your Marketplace event integrations
