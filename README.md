# Mona for SaaS

[The Azure Marketplace is an event-driven system.](https://learn.microsoft.com/partner-center/marketplace-offers/partner-center-portal/pc-saas-fulfillment-life-cycle) Customers purchase subscriptions to your SaaS app. They can change their subscriptions or purchase more seats. Some customers might cancel their subscriptions. These crucial events are the pulse of your SaaS app. Mona embraces this event-driven design by publishing all Marketplace-related events to [a custom Event Grid topic](https://learn.microsoft.com/azure/event-grid/custom-topics) deployed in your environment. [From there, you can handle the events however you want.](https://learn.microsoft.com/azure/event-grid/event-handlers) By default, Mona deploys a set of [Logic Apps](https://learn.microsoft.com/azure/logic-apps/logic-apps-overview) that enable you to control how your SaaS app responds to Marketplace events in a simple low/no-code environment.

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
| **Region** | `r` | [The Azure region](https://azure.microsoft.com/explore/global-infrastructure/geographies/) where Mona should be deployed. For a complete list of Azure regions to choose from, run `az account list-locations -o table` from the Bash Cloud Shell. |
| **Name** | `n` | A unique name for your Mona deployment. It must be 5-13 alphanumeric characters. |

#### Example

```bash
./basic-deploy.sh -r "eastus" -n "monatest01"
```

Once the script is complete, you will be presented with a link to your Mona deployment's admin center. Click the link to finish setting up Mona. Be sure to bookmark the link so you can have quick and easy access to the Mona admin center later. 

> Both guests and members of your Entra (formerly Azure Active Directory) tenant have access to the Mona admin center.

### Configure event integrations

Take a moment to familiarize yourself with the Mona admin center by clicking through the tabs. 

Click on the **This Mona deployment** tab. This tab includes a deep link into the Azure portal and the resource group in which Mona was deployed. Click the resource group link. Within this resource group you will find seven different Logic Apps—each preconfigured to handle a specific Marketplace event. These Logic Apps are already connected to Mona's custom event grid topic.

#### Why Azure Logic Apps?

Azure Logic Apps simplifies the way that you connect legacy, modern, and cutting-edge systems across cloud, on premises, and hybrid environments. You can use low-code-no-code tools to develop highly scalable integration solutions that support your enterprise and business-to-business (B2B) scenarios. The Azure Logic Apps integration platform provides [more than 1,000 prebuilt connectors](https://learn.microsoft.com/connectors/connector-reference/connector-reference-logicapps-connectors) so that you can connect and integrate apps, data, services, and systems more easily and quickly.

> You don't have to use Azure Logic Apps. [Event Grid offers a wide range of built-in options for handling events.](https://learn.microsoft.com/azure/event-grid/event-handlers)

### Test your event integrations

Mona makes it easy to test your Marketplace integrations before going live with your SaaS offer.

From the Mona admin center, click on the **Integration testing** tab. On this tab, you'll find two URLs:

* **Test landing page URL**: Allows you test the complete end-to-end subscription purchasing experience.
* **Test webhook URL**: Allows you to publish webhook notifications against test subscriptions using the test landing page.

#### Using the test landing page

[The test landing page implements the same flow as the live landing page that the Azure Marketplace will redirect your subscribers to.](https://learn.microsoft.com/partner-center/marketplace-offers/azure-ad-saas#process-steps-for-purchase-management) Any guest or member of your Entra tenant can use the test landing page. By default, the test landing page endpoint creates a fake test subscription complete with a full set of fake properties.

You can fully customize the test subscription that is created allowing you to test various subscription scenarios using the query string parameters listed below.

* `subscriptionId`
* `subscriptionName`
* `offerId`
* `planId`
* `isFreeTrial`
* `seatQuantity`
* `term_startDate`
* `term_endDate`
* `term_termUnit`
* `beneficiary_aadObjectId`
* `beneficiary_aadTenantId`
* `beneficiary_userEmail`
* `beneficiary_userId`
* `purchaser_aadObjectId`
* `purchaser_aadTenantId`
* `purchaser_userEmail`
* `purchaser_userId`

For example, to customize the subscription name and offer ID, add query string parameters like this:

```url
https://mona-admin-yourdeployment.azurewebsites.net/test?subscriptionName=MySubscription&offerId=OfferA
```

Properties not configured via query string parameters will automatically be assigned fake values. See [the Azure Marketplace docs](https://learn.microsoft.com/partner-center/marketplace-offers/partner-center-portal/pc-saas-fulfillment-subscription-api) for more information about these subscription properties.

#### Using the test webhook

Once you've created a test subscription, you can try issuing webhook notifications against it. While you don't have to authenticate against the test webhook endpoint, it will only work for subscriptions previously created via the test landing page. Use your favorite API client (e.g., [Postman](https://www.postman.com/downloads/), [cURL](https://curl.se/docs/tutorial.html)) to POST JSON webhook notifications against the test webhook endpoint.

Refer to the table below for more information on issuing webhook notifications.

<table>
<tr>
<td><b>Plan change</b></td>
<td>

```json
{
  "action": "ChangePlan", 
  "subscriptionId": "11111111-1111-1111-1111-111111111111",
  "planId": "Casey's Plan"
} 
```
      
</td>
</tr>
<tr>
<td><b>Seat quantity change</b></td>
<td>

```json
{
  "action": "ChangeQuantity", 
  "subscriptionId": "11111111-1111-1111-1111-111111111111",
  "quantity": 15
} 
```
      
</td>
</tr>
<tr>
<td><b>Suspension</b></td>
<td>

```json
{
  "action": "Suspend", 
  "subscriptionId": "11111111-1111-1111-1111-111111111111"
} 
```
      
</td>
</tr>
<tr>
<td><b>Cancellation</b></td>
<td>

```json
{
  "action": "Unsubscribe", 
  "subscriptionId": "11111111-1111-1111-1111-111111111111"
} 
```
      
</td>
</tr>
<tr>
<td><b>Reinstatement</b></td>
<td>

```json
{
  "action": "Reinstate", 
  "subscriptionId": "11111111-1111-1111-1111-111111111111"
} 
```
      
</td>
</tr>
<tr>
<td><b>Renewal</b></td>
<td>

```json
{
  "action": "Renew", 
  "subscriptionId": "11111111-1111-1111-1111-111111111111"
} 
```
      
</td>
</tr>
</table>
              

