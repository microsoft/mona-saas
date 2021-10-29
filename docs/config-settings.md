# Configuration settings reference

These configuration settings are part of the Mona web app. For more information on how these settings are managed, see [this article](https://docs.microsoft.com/azure/app-service/configure-common#configure-app-settings). You can access these settings at any time by navigating to the Mona admin center (`/admin`), opening the __Mona SaaS configuration settings__ tab, and clicking __Manage configuration settings__.

## ⚠️ Warning

These configuration settings control nearly every aspect of how Mona functions. Most of these settings are automatically configured during Mona setup. Exercise extreme caution when modifying these settings (especially in production).

> Updating any of these settings will automatically restart the Mona web app.

## `Deployment` settings

These settings are automatically configured during setup.

| Name | Description |
| --- | --- |
| `Deployment:AppInsightsInstrumentationKey` | This is the [instrumentation key](https://docs.microsoft.com/azure/azure-monitor/app/create-new-resource#copy-the-instrumentation-key) that Mona uses to publish telemetry to Application Insights. |
| `Deployment:AzureResourceGroupName` | This is the resource group that Mona has been deployed into. |
| `Deployment:AzureSubscriptionId` | The is the Azure subscription that Mona has been deployed into. |
| `Deployment:EventVersion ` | This is the identifier for the version of subscription events that Mona will publish to Event Grid. Supported event model versions are [`2021-05-01`](https://github.com/microsoft/mona-saas/tree/main/Mona.SaaS/Mona.SaaS.Core/Models/Events/V_2021_05_01) and [`2021-10-01`](https://github.com/microsoft/mona-saas/tree/main/Mona.SaaS/Mona.SaaS.Core/Models/Events/V_2021_10_01) (current). |
| `Deployment:IsTestModeEnabled ` | Are the test landing page and webhook endpoints enabled? |
| `Deployment:MonaVersion ` | This is the version of Mona that you're running. |
| `Deployment:Name ` | This is the name of your Mona deployment as configured via the `-n` flag during setup. This name must be globally unique, contain only lower-case alphanumeric characters, and be between 3 and 13 characters long. |

## `Identity` settings

These settings control how Mona is secured using [Azure Active Directory (AAD)](https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-whatis).

| Name | Description |
| --- | --- |
| `Identity:AdminIdentity:AadTenantId` | This is the AAD [tenant ID](https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-how-to-find-tenant) that Mona Administrators must belong to. By default, this is the Azure Active Directory tenant that the user that set up Mona belongs to and is the same as `Identity:AppIdentity:AadTenantId`. |
| `Identity:AdminIdentity:RoleName` | This is the name of the AAD [app role](https://docs.microsoft.com/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps#declare-roles-for-an-application) that Mona administrators must belong to. By default, the role name is `Mona Administrators`. |
| `Identity:AppIdentity:AadClientId` | This is Mona's AAD [app registration](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals#application-object) [client ID](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app#register-an-application). Mona uses this information to securely authenticate to the various Marketplace APIs on your SaaS app's behalf. |
| `Identity:AppIdentity:AadClientSecret` | This is Mona's AAD [app registration](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals#application-object) [client secret](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app#add-a-client-secret). Mona uses this information to securely authenticate to the various Marketplace APIs on your SaaS app's behalf. |
| `Identity:AppIdentity:AadPrincipalId` | This is Mona's AAD [enterprise application/service principal](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals#service-principal-object) object ID. Mona uses this information to securely authenticate to the various Marketplace API's on your SaaS app's behalf. |
| `Identity:AppIdentity:AadTenantId` | This is Mona's AAD [app registration](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals#application-object) [tenant ID](https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-how-to-find-tenant). Mona uses this information to securely authenticate to the various Marketplace APIs on your SaaS app's behalf. By default, this is the same as `Identity:AdminIdentity:AadTenantId`. |

## `PublisherConfig` settings

These settings control how Mona accesses publisher configuration information initially configured through the Mona setup wizard (`/setup`). These settings are stored within Mona's dedicated [blob storage account](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-overview).

| Name | Description |
| --- | --- |
| `PublisherConfig:Store:BlobStorage:ConnectionString` | This is the [connection string needed to access the storage account](https://docs.microsoft.com/azure/storage/common/storage-configure-connection-string) where the publisher configuration [blob](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction#blobs) (`publisher-config.json`) is stored. |
| `PublisherConfig:Store:BlobStorage:ContainerName` | This is the [name of the blob storage container](https://docs.microsoft.com/1azure/storage/blobs/storage-blobs-introduction#containers) where the publisher configuration [blob](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction#blobs) (`publisher-config.json`) is stored. By default, the container name is `configuration`. |

## `Subscriptions` settings

These settings control how Mona accesses its underlying Azure resources to manage SaaS subscriptions.

| Name | Description |
| --- | --- |
| `Subscriptions:Events:EventGrid:TopicEndpoint` | This is the [event grid topic](https://docs.microsoft.com/azure/event-grid/custom-topics) endpoint that Mona should publish subscription events to. |
| `Subscriptions:Events:EventGrid:TopicKey` | This is the [event grid topic](https://docs.microsoft.com/azure/event-grid/custom-topics) access key that Mona uses to publish subscription events. |
| `Subscriptions:Staging:Cache:BlobStorage:ConnectionString` | This is the [connection string needed to access the storage account](https://docs.microsoft.com/azure/storage/common/storage-configure-connection-string) where staged subscripton [blobs](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction#blobs) are [temporarily cached and passed synchronously to your SaaS app's purchase confirmation page](./faq.md#can-i-retrieve-subscription-details-from-the-purchase-confirmation-page). |
| `Subscriptions:Staging:Cache:BlobStorage:ContainerName` | This is the [name of the blob storage container](https://docs.microsoft.com/1azure/storage/blobs/storage-blobs-introduction#containers) where staged subscripton [blobs](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction#blobs) are [temporarily cached and passed synchronously to your SaaS app's purchase confirmation page](./faq.md#can-i-retrieve-subscription-details-from-the-purchase-confirmation-page). |
| `Subscriptions:Testing:Cache:BlobStorage:ConnectionString` | |
| `Subscriptions:Testing:Cache:BlobStorage:ContainerName` | |



