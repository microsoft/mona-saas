# Mona configuration settings

Mona configuration settings are, by default, [stored within the Mona web app's settings](https://docs.microsoft.com/azure/app-service/configure-common#configure-app-settings). 

## ⚠️ Warning

Exercise __extreme caution__ when modifying these configuration settings. Updating these settings will automatically restart the Mona web app. These settings are automatically configured during setup.

## Manage settings

1. Navigate to your Mona admin center (`/admin`).
2. Open the __Mona SaaS configuration settings__ tab.
3. Click __Manage configuration settings__.

## Reference

| Name | Notes |
| --- | --- |
| `Deployment:AppInsightsInstrumentationKey` | [Instrumentation key](https://docs.microsoft.com/azure/azure-monitor/app/create-new-resource#copy-the-instrumentation-key) used to publish telemetry to Application Insights (deployed, by default, within `Deployment:AzureResourceGroupName`). |
| `Deployment:AzureResourceGroupName` | |
| `Deployment:AzureSubscriptionId` | |
| `Deployment:EventVersion ` | Subscription event version Mona should publish; supported versions are [`2021-05-01`](https://github.com/microsoft/mona-saas/tree/main/Mona.SaaS/Mona.SaaS.Core/Models/Events/V_2021_05_01) and [`2021-10-01`](https://github.com/microsoft/mona-saas/tree/main/Mona.SaaS/Mona.SaaS.Core/Models/Events/V_2021_10_01) (current and default) |
| `Deployment:IsTestModeEnabled` | Indicates whether or not [test mode](./#how-can-i-test-my-marketplace-integration-logic-before-going-live-with-an-offer) is enabled. |
| `Deployment:IsPassthroughModeEnabled` | Indicates whether or not [passthrough mode](./#what-is-passthrough-mode) is enabled. |
| `Deployment:MonaVersion ` | |
| `Deployment:Name ` | Configured via the name (`-n`) flag during setup; must contain only lowercase alphanumeric customers and be between 3-13 characters in length |
| `Identity:AdminIdentity:AadTenantId` | AAD [tenant ID](https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-how-to-find-tenant) Mona administrators must belong to (along with being a member of `Identity:AdminIdentity:RoleName`); by default, same as `Identity:AppIdentity:TenantId` |
| `Identity:AdminIdentity:RoleName` | AAD [app role name](https://docs.microsoft.com/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps#declare-roles-for-an-application) Mona administrators must belong to (along with belonging to AAD tenant `Identity:AdminIdentity:AadTenantId`); by default, `Mona Administrators` |
| `Identity:AppIdentity:AadClientId` | AAD Mona web [app registration](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals#application-object) [client ID](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app#register-an-application) |
| `Identity:AppIdentity:AadClientSecret` | AAD Mona web [app registration](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals#application-object) [client secret](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app#add-a-client-secret); used to authenticate to Marketplace API on your app's behalf |
| `Identity:AppIdentity:AadPrincipalId` | AAD Mona web [enterprise app (service principal)](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals#service-principal-object) object ID |
| `Identity:AppIdentity:AadTenantId` | AAD Mona web [app registration](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals#application-object) [tenant ID](https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-how-to-find-tenant); by default, same as `Identity:AdminIdentity:AadTenantId`  |
| `PublisherConfig:Store:BlobStorage:ConnectionString` | [Connection string for storage account](https://docs.microsoft.com/azure/storage/common/storage-configure-connection-string) where publisher configuration [blob](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction#blobs) (`publisher-config.json`) is stored; configured via setup wizard (`/setup`) |
| `PublisherConfig:Store:BlobStorage:ContainerName` | [Blob storage container name](https://docs.microsoft.com/1azure/storage/blobs/storage-blobs-introduction#containers) where publisher configuration [blob](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction#blobs) (`publisher-config.json`) is stored; configured via setup wizard (`/setup`); by default, `configuration` |
| `Subscriptions:Events:EventGrid:TopicEndpoint` | [Event grid topic](https://docs.microsoft.com/azure/event-grid/custom-topics) endpoint URL where Mona should publish subscription events |
| `Subscriptions:Events:EventGrid:TopicKey` | Access key for [event grid topic](https://docs.microsoft.com/azure/event-grid/custom-topics) where Mona should publish subscription events |
| `Subscriptions:Staging:Cache:BlobStorage:ConnectionString` | [Connection string for storage account](https://docs.microsoft.com/azure/storage/common/storage-configure-connection-string) where staged subscription [blobs](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction#blobs) are [temporarily stored and provided to your purchase confirmation page](./faq.md#can-i-retrieve-subscription-details-from-the-purchase-confirmation-page) |
| `Subscriptions:Staging:Cache:BlobStorage:ContainerName` | [Blob storage container name](https://docs.microsoft.com/1azure/storage/blobs/storage-blobs-introduction#containers) where staged subscription [blobs](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction#blobs) are [temporarily stored and provided to your purchase confirmation page](./faq.md#can-i-retrieve-subscription-details-from-the-purchase-confirmation-page) |
| `Subscriptions:Testing:Cache:BlobStorage:ConnectionString` | [Connection string for storage account](https://docs.microsoft.com/azure/storage/common/storage-configure-connection-string) where test subscription [blobs](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction#blobs) are temporarily stored |
| `Subscriptions:Testing:Cache:BlobStorage:ContainerName` | [Blob storage container name](https://docs.microsoft.com/1azure/storage/blobs/storage-blobs-introduction#containers) where test subscription [blobs](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction#blobs) are temporarily stored |



