# Frequently asked questions

* [How do I install Mona?](#how-do-i-install-mona)
* [How do I upgrade Mona?](#how-do-i-upgrade-mona)
* [Why aren't my Mona events triggering my Logic Apps?](#why-arent-my-mona-events-triggering-my-logic-apps)
* [How do I notify the Marketplace that a subscription has been activated?](#how-do-i-notify-the-marketplace-that-a-subscription-has-been-activated)
* [How do I uninstall Mona?](#how-do-i-uninstall-mona)
* [Where is the admin center?](#where-is-the-admin-center)
* [Are guest or personal accounts allowed?](#are-guest-or-personal-accounts-allowed)
* [How can I return to the setup wizard?](#how-can-i-return-to-the-setup-wizard)
* [Why do I get Access Denied in setup wizard?](#why-do-i-get-access-denied-in-setup-wizard)
* [Where can I find my offer's Partner Center technical configuration details?](#where-can-i-find-my-offers-partner-center-technical-configuration-details)
* [Where can I learn more about the various events that Mona publishes?](#where-can-i-learn-more-about-the-various-events-that-mona-publishes)
* [Can I handle subscription events somewhere other than Logic Apps?](#can-i-handle-subscription-events-somewhere-other-than-logic-apps)
* [Who can access the admin center, setup wizard, and test endpoints?](#who-can-access-the-admin-center-setup-wizard-and-test-endpoints)
* [How do I manage Mona administrators?](#how-do-i-manage-mona-administrators)
* [What is the subscription purchase confirmation page?](#what-is-the-subscription-purchase-confirmation-page)
* [Can I retrieve subscription details from the purchase confirmation page?](#can-i-retrieve-subscription-details-from-the-purchase-confirmation-page)
* [What is the subscription configuration page?](#what-is-the-subscription-configuration-page)
* [How can I test my Marketplace integration logic before going live with an offer?](#how-can-i-test-my-marketplace-integration-logic-before-going-live-with-an-offer)
* [How do I use the test webhooks?](#how-do-i-use-the-test-webhooks)
* [Can I customize the test subscription that Mona generates?](#can-i-customize-the-test-subscription-that-mona-generates)
* [What is passthrough mode?](#what-is-passthrough-mode)
* [How can I modify Mona's configuration settings?](#how-can-i-modify-monas-configuration-settings)
* [Why did I receive a warning about needing admin approval when logging into Mona?](#why-did-i-receive-a-warning-about-needing-admin-approval-when-logging-into-mona)
* [Where can I find Mona's diagnostic logs?](#where-can-i-find-monas-diagnostic-logs)
* [How do I debug Mona?](#how-do-i-debug-mona)
* [Does Mona have a health check endpoint?](#does-mona-have-a-health-check-endpoint)

## How do I install Mona?

See [this doc](../README.md/#how-do-i-get-started-with-mona-saas).

## How do I upgrade Mona?

1. Open the [Bash cloud shell](https://docs.microsoft.com/azure/cloud-shell/quickstart#start-cloud-shell) from the Azure portal.
2. Get the latest version of Mona.
    * If you originally set up Mona, the repo is likely already there. If the `mona-saas` directory already exists, navigate to that directory by running `cd mona-saas` then run `git pull origin`. This will update your local copy of Mona to the latest version.
    * If you don't already have a `mona-saas` directory, run `git clone https://github.com/microsoft/mona-saas`. Run `cd mona-saas` to navigate to the Mona directory.
3. From the root Mona directory (`mona-saas`), navigate to the setup folder by running `cd Mona.SaaS/Mona.SaaS.Setup`.
4. Run `chmod +x ./basic-upgrade.sh` to allow the script to be executed within the cloud shell.
5. Run `./basic-upgrade.sh`.

The upgrade script will scan all Azure subscriptions that you have access to looking for existing Mona deployments. Once it finds a Mona deployment with a different version (indicated by the `Mona Version` resource group tag), the script will ask if you want to upgrade it. Once the upgrade is complete, the script automatically [checks the health of the deployment using Mona's health check endpoint](#does-mona-have-a-health-check-endpoint) and, if the check is successful, commits the upgraded Mona deployment to production. If the upgrade Mona deployment is unhealthy, the script automatically rolls it back to the previous version.

> Previous versions of Mona (pre-general availability) may not expose a health check endpoint. The upgrade script is unable to safely upgrade a Mona deployment that doesn't expose a health check endpoint. In these cases, you will likely need to redeploy Mona. 

> The upgrade script depends on [Azure App Service staging slots](https://docs.microsoft.com/azure/app-service/deploy-staging-slots) to perform a safe upgrade. The staging slots feature is available in the Standard (default for Mona deployment), Premium, and Isolated App Service tiers. Automated upgrades are not supported in Free, Shared, and Basic tiers. [For more information, see App Service limits.](https://docs.microsoft.com/azure/azure-resource-manager/management/azure-subscription-service-limits#app-service-limits) 

## Why aren't my Mona events triggering my Logic Apps?

When you deploy Mona, an event grid topic is created which all Mona events are published to then, by default, a set of logic apps are deployed (one per event type) that, usually, are automatically connected to the event grid topic. Sometimes and unfortunately, intermittently, these logic apps don't get connected to the event grid topic. This is a problem that we are actively working on solving but, for the time being, you may need to go into your logic apps and manually reconnect them to the event grid topic.

To do this, simply navigate to the Mona resource group, open the offending logic app, and navigate to the logic app designer. If the logic app trigger is not connected, an exclamation point ⚠️ will appear next to it. You will need to expand the trigger and reconnect it to the Mona event grid topic through the Azure portal. Creating a new client secret in the Azure AD Application might be needed to fix the connection using the "Connect with service principal" option. Then, on each logic app impacted, deleting the Event Type Item (eg: Mona.SaaS.Marketplace.SubscriptionCancelled), saving and retyping it as a custom value might be needed in order to recreate the Event Subscriptions in the Mona event grid if those are missing.

The issue should be fixed when you will have 7 Event subscriptions for the 7 Event Types managed by Mona:
* Mona.SaaS.Marketplace.SubscriptionSeatQuantityChanged
* Mona.SaaS.Marketplace.SubscriptionPlanChanged
* Mona.SaaS.Marketplace.SubscriptionReinstated
* Mona.SaaS.Marketplace.SubscriptionSuspended
* Mona.SaaS.Marketplace.SubscriptionRenewed
* Mona.SaaS.Marketplace.SubscriptionPurchased
* Mona.SaaS.Marketplace.SubscriptionCancelled

> Can't find Mona's event grid topic? Navigate to the __Mona admin center__ (`/admin`), open the __subscription event handlers__ tab, and locate the event grid topic link. Opening this link will navigate you to the custom event grid topic in the Azure portal.

## How do I notify the Marketplace that a subscription has been activated?

Once a customer has purchased their solution through the Marketplace and the Mona purchase onboarding workflow, it's still __your responsibility to notify the Marketplace that the subscription has been activated__. You have 30 days from the time of purchase to notify the Marketplace that the subscription has been activated on your side.

We try to make this as easy as possible by including a step at the end of the "subscription purchased" logic app (in your resource group, it will be called `mona-on-subscription-purchased-[your deployment name]`) that _conditionally_ notifies the Marketplace that the subscription has been activated. By default, this condition is set to _not_ automatically notify the Marketplace that the subscription has been activated because, depending on your SaaS app, subscriptions may be activated immediately or there may be some time window when you're provisioning infrastructure, going through sales processes, etc. that will elapse before you actually activate the subscription.

Below, we outline both scenarios and how you should configure Mona accordingly.

#### If you wish to activate the subscription immediately...

Navigate to the `Conditional | Notify the Marketplace` step in the logic app designer and toggle the condition as shown below to `true`. This will automatically notify the Marketplace when a subscription has been activated and you're done.

> ⚠️ __Warning!__ When using [passthrough mode](https://github.com/microsoft/mona-saas/tree/main/docs#what-is-passthrough-mode), the subscription will be automatically activated before the subscriber sees your purchase confirmation page. It's important that you give the subscriber an opportunity to confirm their purchase before you activate their subscription with the Marketplace.

![Toggling Mona activation on](images/mona_activate_immediately.png)

#### If you with to active the subscription at some point in the future...

Navigate to the `Conditional | Notify the Marketplace` step in the logic app designer and expand the `Conditional | Notify the Marketplace` step and that condition's `True` branch. Within that branch, you will find a HTTP POST action that conditionally notifies the Marketplace that the subscription has been activated. This action contains the Azure Active Directory details that you will need to make this call on your own. Find these values as shown below and note them.

![Toggling Mona activation on](images/mona_activate_later.png)

You'll need to take these same values and use them to [call the Marketplace on your own as part of activation](https://learn.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-subscription-api#activate-a-subscription). [This doc](https://learn.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-registration#get-the-token-with-an-http-post) describes how to obtain an Azure Active Directory access token to call the API on your own. Note that you can also copy/paste the existing logic app action into another logic app that your app can call later once the subscription has been activated.

## How do I uninstall Mona?

> ⚠️ __Warning!__ These actions are irreversible.

* [Delete Mona's Azure Active Directory (AAD) app registration.](https://docs.microsoft.com/azure/active-directory/develop/howto-remove-app#remove-an-application-authored-by-you-or-your-organization) Client ID can be found on Mona resource group tag `AAD App ID`.
* [Delete Mona's resource group.](https://docs.microsoft.com/azure/azure-resource-manager/management/delete-resource-group?tabs=azure-portal#delete-resource-group)

## Where is the admin center?

In your browser, navigate to `/admin` (e.g., `https://mona-web-***.azurewebsites.net/admin`).

## Are guest or personal accounts allowed?

Mona restricts administrative access to members and not guests or personal accounts.  Please only use workplace/school accounts.

## How can I return to the setup wizard?

In your browser, navigate to `/setup` (e.g., `https://mona-web-***.azurewebsites.net/setup`).

## Why do I get Access Denied in setup wizard?

If you installed Mona with a Guest account or a Personal account (Microsoft Account) you will get an Access Denied error when accessing
the setup wizard or admin center. 

## Where can I find my offer's Partner Center technical configuration details?

When you create a SaaS offer through the Partner Center, you have to tell the Partner Center how to connect to you SaaS app to enable transactability with the Microsoft commercial marketplace. This is accomplished through your [offer's technical configuration details](https://docs.microsoft.com/en-us/azure/marketplace/create-new-saas-offer-technical).

These values can always be found in the Mona admin center (`/admin`).

## Where can I learn more about the various events that Mona publishes?

See [this doc](event-models/README.md).

## Can I handle subscription events somewhere other than Logic Apps?

Absolutely. As a convenience, we deploy "stub" Logic Apps into your Azure environment allowing you to rapidly build out subscription lifecycle workflows using a simple visual designer. These workflows are triggered by [events that Mona publishes to a custom event grid topic](event-models/README.md). There are many ways to subscribe to a custom event grid topic including [Azure Functions](https://docs.microsoft.com/azure/azure-functions/functions-bindings-event-grid-trigger) and [Power Automate](https://flow.microsoft.com/connectors/shared_azureeventgrid/azure-event-grid/) allowing you to select the integration option that makes the most sense for you and your organization.

> Can't find Mona's event grid topic? Navigate to the __Mona admin center__ (`/admin`), open the __subscription event handlers__ tab, and locate the event grid topic link. Opening this link will navigate you to the custom event grid topic in the Azure portal.

## Who can access the admin center, setup wizard, and test endpoints?

Only Mona Administrators can access the admin center, setup wizard, and test endpoints. 

## How do I manage Mona administrators?

1. Navigate to the admin center (`/admin`).
2. Open the __Mona SaaS configuration settings__ tab.
3. Click __Manage users__. 

You will be redirected to [Mona's Azure Active Directory (AAD) Mona Administrators app role where you can add/remove users](https://docs.microsoft.com/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps#assign-users-and-groups-to-roles).

## What is the subscription purchase confirmation page?

Mona acts as a proxy between the Microsoft commercial marketplace and your SaaS app by implenting the required landing page and webhook endpoints. When your customer confirms that they wish to purchase a subscription by clicking the __Confirm purchase__ button on the landing page, Mona redirects them to the _purchase confirmation page_. Essentially, this is where Mona hands new Microsoft commercial marketplace subscription purchases off to your app.

Mona administrators can configure the purchase confirmation page URL at any time by navigating to the setup wizard (`/setup`).

* Mona will automatically replace the URL token `{subscription-id}` with the applicable subscription ID on redirect.
* Mona provides time-limited, bearer URL access to full subscription details through the `_sub` query string parameter on redirect. [For more information on how this works, see this question.](#can-i-retrieve-subscription-details-from-the-purchase-confirmation-page)

## Can I retrieve subscription details from the purchase confirmation page?

After a customer has confirmed their AppSource/Marketplace purchase through the Mona landing page, they are automatically redirected to a [publisher-managed purchase confirmation page](#what-is-the-subscription-purchase-confirmation-page) to complete their subscription configuration.

By default, Mona will also include a partial link (via the `_sub` query parameter highilghted in the below image) that, when combined with the base storage URL (provided during Mona setup), can be used to efficiently and securely pull the subscription details. Note that the `_sub` parameter is URL encoded. In order to use it, you must first URL decode it before combining it with the base storage URL.

![Subscription details URL construction](images/complete-redirect-url.PNG)

> By default, subscription definitions are staged in Azure blob storage. The URL that you construct is actually a [shared access signature (SAS) token](https://docs.microsoft.comazure/storage/common/storage-sas-overview#sas-token) granting time-limited, read-only access to the subscription definition blob.

When you issue an HTTP GET request against the combined URL, the full subscription details will be returned as a JSON object. The URL itself is valid only for a short period of time (by default, five (5) minutes). After that time, the URL is useless.

This design prevents outside actors from either spoofing the subscription details or reading the subscription details since you need both pieces of information (the base storage URL and the `_sub` parameter value) in order to access the information.

> This functionality is enabled by default. You can disable it by setting the `Deployment:SendSubscriptionDetailsToPurchaseConfirmationPage` configuration value to `false` and restarting the web app.

## What is the subscription configuration page?

Microsoft provides a link (through the various commercial marketplace web interfaces) to your subscribers allowing them to manage their subscription. In practice, this link redirects the user to your landing page (the same one Mona exposes to support new subscription purchases) with a token that resolves to an existing subscription. As the SaaS provider, it's your responsibility to check for this condition and, if needed, redirect the customer to a subscription management experience. To simplify things, Mona always checks for this condition and, if the subscription already exists, automatically redirects the user to the configured _subscription configuration page_.

* Mona will automatically replace the URL token `{subscription-id}` with the applicable subscription ID on redirect.

## How can I test my Marketplace integration logic before going live with an offer?

By default, Mona exposes a set of test landing page and webhook endpoints that Mona administrators can use to test integration logic without the need for an actual Microsoft commercial marketplace offer.

> These endpoints can be disabled by [setting the `Deployment:IsTestModeEnabled` configuration setting to `false`](config-settings.md).

You can find both test endpoints in the __Testing__ tab of the Mona admin center (`/admin`).

The test landing page (`/test`) can only be accessed by Mona administrators. The test landing page behaves and looks just like the live landing page except for a warning banner across the top of the page indicating that you're running in test mode. You can customize the test subscription that Mona automatically generates by using [these query string parameters](https://github.com/microsoft/mona-saas/blob/357aa09039f9c8c0dfd324cdd7903b3dbdef88c6/Mona.SaaS/Mona.SaaS.Web/Controllers/SubscriptionController.cs#L591) _only_ on the test landing page.

You can use tools like [cURL](https://curl.se/) (scriptable; great for automated testing) and [Postman](https://www.postman.com/) and the Mona test webhook endpoint (`/webhook/test`) to test [all kinds of Marketplace webhook invocations](https://docs.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#implementing-a-webhook-on-the-saas-service) against subscriptions previously created through the test landing page (`/test`). These test subscriptions automatically expire (you can no longer perform webhook operations against them) after 30 days of inactivity. Like the live webhook, the test webhook requires no authentication but operations succeed only when executed against existing test subscriptions.

## How do I use the test webhooks?

To begin testing the webhook endpoints, you go to the (`/admin`) page of your Mona deployment and then to the Testing Tab.  You should see the following:

![image](https://user-images.githubusercontent.com/111533671/228665759-28361274-646b-45a8-ac4a-79e64731cdc8.png)

The Test Connection Webhook URL is what you will use to send requests to test the webhooks.  Next, you will want to grab the subscription URL.  You can click on the Test Landing Page URL to take you to the (`/test`) landing page.  You should see the following:

![image](https://user-images.githubusercontent.com/111533671/228665826-02e52c0a-5052-4fca-ab3a-0830ac37bcbd.png)

As mentioned in the previous [FAQ bullet point](https://github.com/microsoft/mona-saas/tree/main/docs#how-can-i-test-my-marketplace-integration-logic-before-going-live-with-an-offer), you can use CURL or Postman to test your webhook.  

The following parameters can be used when testing the webhook.  All webhooks will require SubscriptionID and ActionType.
- id
- activityId
- ***subscriptionId***
- publisherId
- offerId
- planId
- quantity
- timestamp
- status
- ***action***
   - action types:
      - ChangePlan
      - ChangeQuantity
      - Suspend
      - Unsubscribe
      - Reinstate
      - Renew

> ***If you are doing a seat quantity change you must include the quantity paramater.  If you are doing a plan change you must provide the planId paramater.***

The following examples show JSON payloads for each one of the available actions:
 
```json
{
    "subscriptionId": "e8e69216-745e-4f0c-91d2-886bb04343e3",
    "action": "ChangePlan",
    "planId": "Josh's Plan"
}
```

```json
{
    "subscriptionId": "e8e69216-745e-4f0c-91d2-886bb04343e3",
    "action": "ChangeQuantity",
    "quantity": 6
}
```
```json
{
    "subscriptionId": "e8e69216-745e-4f0c-91d2-886bb04343e3",
    "action": "Suspend"
}
```
```json
{
    "subscriptionId": "e8e69216-745e-4f0c-91d2-886bb04343e3",
    "action": "Unsubscribe"
}
```
```json
{
    "subscriptionId": "e8e69216-745e-4f0c-91d2-886bb04343e3",
    "action": "Reinstate"
}
```
```json
{
    "subscriptionId": "e8e69216-745e-4f0c-91d2-886bb04343e3",
    "action": "Renew"
}
```

Using one of the example JSON payloads, do a HTTP POST request and you should receive back a 200 OK indicating the request was successful. We can verify this by visiting the Mona Resource group you deployed and finding the corresponding Logic App.  For the above example, we tested the subscription plan changing so we could go to the mona-on-subscription-plan-changed-contoso Logic App and see that the run was successful.  

![image](https://user-images.githubusercontent.com/111533671/228671265-7859c798-936b-4f3f-8368-79598ae0d2e6.png)

For further verification, you can click on the successful run and find the exact parameters you used in your request.

![image](https://user-images.githubusercontent.com/111533671/234479164-141251d3-18ce-42c4-bad6-881631e2f257.png)

## Can I customize the test subscription that Mona generates?

Yes you can. Through the test landing page (`/test`) you can use query string parameters to override properties on the test subscription that is generated allowing you to test very specific subscription scenarios.

The table below contains the query string parameters that are available for you to use.

### Parameters

| Parameter | Description |
| --- | --- |
| `subscriptionId` | Subscription ID |
| `subscriptionName` | Subscription name |
| `offerId` | Offer ID |
| `planId` | Plan ID |
| `isFreeTrial` | Is this a free trial subscription? |
| `seatQuantity` | The total number of seats available in this subscription |
| `term_startDate` | The current subscription term start date |
| `term_endDate` | The current subscription term end date |
| `term_termUnit` | `P1M` for monthly term; `P1Y` for annual term |
| `beneficiary_aadObjectId` | The Azure Active Directoy object ID (`sub` claim) of the user that the subscription was created for |
| `beneficiary_aadTenantId` | The Azure Active Directory tenant ID (`tid` claim) of the user that the subscription was created for |
| `beneficiary_userEmail` | The email address of the user that the subscription was created for |
| `purchaser_aadObjectId` | The Azure Active Directory object ID (`sub` claim) of the user that purchased this subscription (e.g., in a CSP scenario) |
| `purchaser_aadTenantId` | The Azure Active Directory tenant ID (`tid` claim) of the user that purchased this subscription (e.g., in a CSP scenario) |
| `purchaser_userEmail` | The email address of the user that purchased the subscription |

### Example

`/test?offerId=Offer1&seatQuantity=40`

This set of query string parameters will generate a test subscription with offer ID `Offer1` and `40` total seats.

## What is passthrough mode?

Normally, when a customer is routed from the commercial Marketplace to the Mona landing page, Mona authenticates the user and presents them with a pre-built landing page that prompts the user to complete their purchase.

When configured for passthrough mode, the customer never sees Mona's landing page and, consequently, is never forced to authenticate. Instead, the landing page simply resolves the subscription, publishes the `Mona.SaaS.Marketplace.SubscriptionPurchased` event, and redirects the user to [the purchase confirmation page](#what-is-the-subscription-purchase-confirmation-page) that you configured during setup (`/setup`). If configured, Mona continues to [provide subscription details to the purchase confirmation page using the `_sub` query string parameter](#can-i-retrieve-subscription-details-from-the-purchase-confirmation-page). Mona's webhook endpoint is not affected by passthrough mode.

To enable passthrough mode, [navigate to Mona's configuration settings](#how-can-i-modify-monas-configuration-settings) and set `Deployment:IsPassthroughModeEnabled` to `true`. This will force a restart of the Mona web app.

Passthrough mode can also be enabled [during setup by using the `-m` flag](https://github.com/microsoft/mona-saas/blob/main/README.md/#3-set-up-mona-saas).

> ⚠️ __Warning!__ It's important that you give the customer an opportunity to confirm their purchase. Since Mona doesn't provide a landing page UI in passthrough mode, it's your responsibility to confirm that the customer indeed wants to purchase your offer before you [notify the Marketplace that the subscription has been activated](https://github.com/microsoft/mona-saas/tree/main/docs#how-do-i-notify-the-marketplace-that-a-subscription-has-been-activated).

## How can I modify Mona's configuration settings?

See [this doc](config-settings.md).

## Why did I receive a warning about needing admin approval when logging into Mona?

After doing the initial setup of Mona you may receive a warning that the app registration is unverified and it needs admin approval.  You can return to the application without granting consent but if you would like to address the warning please see these docs:
* [Publisher verification overview](https://learn.microsoft.com/en-us/azure/active-directory/develop/publisher-verification-overview)
* [Mark your app as publisher verified](https://learn.microsoft.com/en-us/azure/active-directory/develop/mark-app-as-publisher-verified)

## Where can I find Mona's diagnostic logs?

By default, Mona pulishes telemetry (including logs) to an [Application Insights](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview) resource automatically deployed into your Mona resource group on setup. From here, you can search logs and configure alerts as needed.

## How do I debug Mona?

Given the numerous dependencies that Mona takes on Azure, the easiest way to debug it is to deploy it into your Azure environment (using the provided setup script) and [use Visual Studio to remotely debug the Mona web app](https://docs.microsoft.com/azure/app-service/troubleshoot-dotnet-visual-studio#remotedebug) directly. This will require you to open the Mona solution that you previously cloned in Visual Studio and attach the debugger to your running Mona web app.

Remotely debugging the Mona web app using the method described above requires that the app be deployed in [`debug` configuration](https://docs.microsoft.com/visualstudio/debugger/how-to-set-debug-and-release-configurations). By default, the Mona web app is deployed in `release` configuration for performance reasons. When debugging, we recommend that you [use Visual Studio to publish the Mona web app in `debug` configuration](https://docs.microsoft.com/azure/app-service/quickstart-dotnetcore?tabs=netcore31&pivots=development-environment-vs#publish-your-web-app) to its own [App Service deployment slot](https://docs.microsoft.com/azure/app-service/deploy-staging-slots). Doing so will prevent your production customers from being interrupted and give you a clean, easily disposed environment for your debugging needs.

## Does Mona have a health check endpoint?

Yes it does. Mona exposes a public health check `HTTP GET` endpoint at `/health`. If the Mona deployment is healthy and able to access all of its service dependencies, the endpoint will return a `HTTP 200 OK`. Any other status code indicates that Mona is unhealthy. [The Mona upgrade script uses the endpoint to ensure that the deployment is healthy before committing the upgrade to production.](#how-do-i-upgrade-mona) You can also use this health check endpoint to deploy Mona in a highly available configuration using [Azure Front Door](https://docs.microsoft.com/azure/frontdoor/health-probes), [Application Gateway](https://docs.microsoft.com/azure/application-gateway/application-gateway-probe-overview), [Kubernetes](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/), etc. [You can also leverage the Application Insights resource deployed with Mona to configure availability tests using the health check endpoint.](https://docs.microsoft.com/azure/azure-monitor/app/availability-standard-tests)
