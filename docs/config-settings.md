# Configuration settings

Mona configuration settings are, by default, [stored within the Mona web app's settings](https://docs.microsoft.com/azure/app-service/configure-common#configure-app-settings). 

## ⚠️ Warning

Exercise __extreme caution__ when modifying these configuration settings. Updating these settings will automatically restart the Mona web app. These settings are automatically configured during setup.

## Manage settings

1. Navigate to your Mona admin center (`/admin`).
2. Open the __Mona SaaS configuration settings__ tab.
3. Click __Manage configuration settings__.

## Reference

### `Deployment` settings

| Name | Notes |
| --- | --- |
| `Deployment:AppInsightsInstrumentationKey` | [Instrumentation key](https://docs.microsoft.com/azure/azure-monitor/app/create-new-resource#copy-the-instrumentation-key) used to publish telemetry to Application Insights (deployed, by default, within `Deployment:AzureResourceGroupName`). |
| `Deployment:AzureResourceGroupName` | |
| `Deployment:AzureSubscriptionId` | |
| `Deployment:EventVersion ` | Subscription event version Mona should publish; supported versions are [`2021-05-01`](https://github.com/microsoft/mona-saas/tree/main/Mona.SaaS/Mona.SaaS.Core/Models/Events/V_2021_05_01) and [`2021-10-01`](https://github.com/microsoft/mona-saas/tree/main/Mona.SaaS/Mona.SaaS.Core/Models/Events/V_2021_10_01) (current) |
| `Deployment:IsTestModeEnabled ` | |
| `Deployment:MonaVersion ` | |
| `Deployment:Name ` | Configured via the name (`-n`) flag during setup; must contain only lowercase alphanumeric customers and be between 3-13 characters in length |

### `Identity` settings

These settings control how Mona is secured using [Azure Active Directory (AAD)](https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-whatis).

| Name | Description |
| --- | --- |
| `Identity:AdminIdentity:AadTenantId` | AAD [tenant ID](https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-how-to-find-tenant) Mona administrators must belong to (along with being a member of `Identity:AdminIdentity:RoleName`); by default, same as `Identity:AppIdentity:TenantId` |
| `Identity:AdminIdentity:RoleName` | AAD [app role name](https://docs.microsoft.com/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps#declare-roles-for-an-application) Mona administrators must belong to (along with belonging to AAD tenant `Identity:AdminIdentity:AadTenantId`); by default, `Mona Administrators` |
| `Identity:AppIdentity:AadClientId` | AAD Mona web [app registration](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals#application-object) [client ID](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app#register-an-application) |
| `Identity:AppIdentity:AadClientSecret` | AAD Mona web [app registration](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals#application-object) [client secret](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app#add-a-client-secret); used to authenticate to Marketplace API on your app's behalf |
| `Identity:AppIdentity:AadPrincipalId` | AAD Mona web [enterprise app (service principal)](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals#service-principal-object) object ID |
| `Identity:AppIdentity:AadTenantId` | AAD Mona web [app registration](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals#application-object) [tenant ID](https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-how-to-find-tenant); by default, same as `Identity:AdminIdentity:AadTenantId`  |

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
| `Subscriptions:Staging:Cache:BlobStorage:ConnectionString` | This is the [connection string needed to access the storage account](https://docs.microsoft.com/azure/storage/common/storage-configure-connection-string) where staged subscription [blobs](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction#blobs) are [temporarily cached and passed synchronously to your SaaS app's purchase confirmation page](./faq.md#can-i-retrieve-subscription-details-from-the-purchase-confirmation-page). |
| `Subscriptions:Staging:Cache:BlobStorage:ContainerName` | This is the [name of the blob storage container](https://docs.microsoft.com/1azure/storage/blobs/storage-blobs-introduction#containers) where staged subscription [blobs](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction#blobs) are [temporarily cached and passed synchronously to your SaaS app's purchase confirmation page](./faq.md#can-i-retrieve-subscription-details-from-the-purchase-confirmation-page). |
| `Subscriptions:Testing:Cache:BlobStorage:ConnectionString` | This is the [connection string needed to access the storage account](https://docs.microsoft.com/azure/storage/common/storage-configure-connection-string) where test subscription [blobs](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction#blobs) (created and manipulated via the test landing page and webhook endpoints) are temporarily cached. |
| `Subscriptions:Testing:Cache:BlobStorage:ContainerName` | This is the [name of the blob storage container](https://docs.microsoft.com/1azure/storage/blobs/storage-blobs-introduction#containers) where test subscription [blobs](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction#blobs) (created and manipulated via the test landing page and webhook endpoints) are temporarily cached. |


