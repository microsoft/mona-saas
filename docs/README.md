# Frequently asked questions

* [How do I install Mona?](#how-do-i-install-mona)
* [How do I uninstall Mona?](#how-do-i-uninstall-mona)
* [Where is the admin center?](#where-is-the-admin-center)
* [Who can access the admin center, setup wizard, and test endpoints?](#who-can-access-the-admin-center-setup-wizard-and-test-endpoints)
* [How do I manage Mona administrators?](#how-do-i-manage-mona-administrators)
* [What is the subscription purchase confirmation page?](#what-is-the-subscription-purchase-confirmation-page)
* [Can I retrieve subscription details from the purchase confirmation page?](#can-i-retrieve-subscription-details-from-the-purchase-confirmation-page)
* [What is the subscription configuration page?](#what-is-the-subscription-configuration-page)
* [How can I test my Marketplace integration logic before going live with an offer?](#what-is-the-subscription-configuration-page)
* [How do I debug Mona?](#how-do-i-debug-mona)

## How do I install Mona?

See [this doc](../README.md/#how-do-i-get-started-with-mona-saas).

## How do I uninstall Mona?

> ⚠️ __Warning!__ These actions are irreversible.

* [Delete Mona's Azure Active Directory (AAD) app registration.](https://docs.microsoft.com/azure/active-directory/develop/howto-remove-app#remove-an-application-authored-by-you-or-your-organization) Client ID can be found on Mona resource group tag `AAD App ID`.
* [Delete Mona's resource group.](https://docs.microsoft.com/azure/azure-resource-manager/management/delete-resource-group?tabs=azure-portal#delete-resource-group)

## Where is the admin center?

In your browser, navigate to `/admin` (e.g., `https://mona-web-***/admin`).

## Who can access the admin center, setup wizard, and test endpoints?

Only Mona Administrators can access the admin center, setup wizard, and test endpoints. 

## How do I manage Mona administrators?

1. Navigate to the admin center (`/admin`)
2. Open the __Mona SaaS configuration settings__ tab.
3. Click __Manage users__. 

You will be redirected to Mona's Azure Active Directory (AAD) Mona Administrators app role where you can add/remove users.

## What is the subscription purchase confirmation page?

Mona acts as a proxy between the Microsoft commercial marketplace by implenting the required landing page and webhook endpoints. When your customer confirms that they wish to purchase a subscription by clicking the __Confirm purchase__ button on the landing page, Mona redirects them to the _purchase confirmation page_. Essentially, this is where Mona hands new Microsoft commercial marketplace subscription purchases off to your app.

Mona administrators can configure the purchase confirmation page URL at any time by navigating to the setup wizard (`/setup`).

* Mona will automatically replace the URL token `{subscription-id}` with the applicable subscription ID on redirect.
* Mona provides time-limited, bearer URL access to full subscription details through the `_sub` query string parameter on redirect.

## Can I retrieve subscription details from the purchase confirmation page?

After a customer has confirmed their AppSource/Marketplace purchase through the Mona landing page, they are automatically redirected to a publisher-managed (ISV) purchase confirmation page to complete their subscription configuration.

By default, Mona will also include a partial link (via the `_sub` query parameter highilghted in the below image) that, when combined with the base storage URL (provided during Mona setup), can be used to efficiently and securely pull the subscription details. Note that the `_sub` parameter is URL encoded. In order to use it, you must first URL decode it before combining it with the base storage URL.

![Subscription details URL construction](images/complete-redirect-url.PNG)

> By default, subscription definitions are staged in Azure blob storage. The URL that you construct is actually a [shared access signature (SAS) token](https://docs.microsoft.comazure/storage/common/storage-sas-overview#sas-token) granting time-limited, read-only access to the subscription definition blob.

When you issue an HTTP GET request against the combined URL, the full subscription details will be returned as a JSON object. The URL itself is valid only for a short period of time (by default, five (5) minutes). After that time, the URL is useless.

This design prevents outside actors from either spoofing the subscription details or reading the subscription details since you need both pieces of information (the base storage URL and the `_sub` parameter value) in order to access the information.

> This functionality is enabled by default. You can disable it by setting the `Deployment:SendSubscriptionDetailsToPurchaseConfirmationPage` configuration value to `false` and restarting the web app.

## What is the subscription configuration page?

Microsoft provides a link to your subscribers allowing them to manage their subscriptions. In practice, this link redirects the user to the landing page (that Mona exposes) with a token that resolves to a subscription that already exists. As SaaS provider, it's your responsibility to check for this condition and provide a subscription management experience. Mona always checks for this condition and, if the subscription already exists, the user is redirected to the _subscription configuration page_.

* Mona will automatically replace `{subscription-id}` with the applicable subscription ID on redirect.

## How can I test my Marketplace integration logic before going live with an offer?

By default, Mona provides a set of test landing page and webhook endpoints that Mona administrators can use to test integration logic while bypassing the marketplace before going live with an offer.

You can find both test endpoints in the __Testing__ tab of the Mona admin center (`/admin`).

The test landing page (`/test`) can only be accessed by Mona administrators. The test landing page behaves and looks just like the live landing page except for a warning banner across the top of the page. You can customize every property of the test subscription that Mona generates by using [these query string parameters](https://github.com/microsoft/mona-saas/blob/357aa09039f9c8c0dfd324cdd7903b3dbdef88c6/Mona.SaaS/Mona.SaaS.Web/Controllers/SubscriptionController.cs#L591).

You can use tools like cURL or Postman and the Mona test webhook endpoint (`/webhook/test`) to test [Marketplace webhook invocations](https://docs.microsoft.com/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#implementing-a-webhook-on-the-saas-service) against subscriptions previously created through the test landing page (`/test`). These test subscriptions automatically expire (you can no longer perform webhook operations against them) after 30 days of inactivity. Like the live webhook, the test webhook requires no authentication but operations succeed only for existing test subscriptions.

## How do I debug Mona?

[Follow instructions here to remotely debug your Mona web app using Visual Studio.](https://docs.microsoft.com/en-us/azure/app-service/troubleshoot-dotnet-visual-studio#remotedebug) You can also review logs published to Mona's Application Insights resource.



