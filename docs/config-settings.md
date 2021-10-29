# Configuration settings reference

These configuration settings are part of the Mona web app. For more information on how these settings are managed, see [this article](https://docs.microsoft.com/azure/app-service/configure-common#configure-app-settings). You can access these settings at any time by navigating to the Mona admin center (`/admin`), opening the __Mona SaaS configuration settings__ tab, and clicking __Manage configuration settings__.

> ⚠️ __Warning:__ These configuration settings control nearly every aspect of how Mona functions. Most of these settings are automatically configured during Mona setup. Exercise extreme caution when modifying these settings (especially in production). Updating any of these settings will automatically restart the Mona web app.

| Setting name | Description |
| --- | --- |
| `Deployment:AppInsightsInstrumentationKey` | Mona uses this key to publish telemetry to Application Insights. |
| `Deployment:AzureResourceGroupName` | Mona is deployed within this resource group. |
| `Deployment:AzureSubscriptionId` | Mona is deployed within this Azure subscription. |
| `Deployment:EventVersion ` | The subscription event model version that Mona is currently publishing to Event Grid. Supported event model versions are [`2021-05-01`](https://github.com/microsoft/mona-saas/tree/main/Mona.SaaS/Mona.SaaS.Core/Models/Events/V_2021_05_01) and [`2021-10-01`](https://github.com/microsoft/mona-saas/tree/main/Mona.SaaS/Mona.SaaS.Core/Models/Events/V_2021_10_01) (current). |
| `Deployment:IsTestModeEnabled ` | Indicates whether or not the test landing page and webhook endpoints are enabled |
| `Deployment:MonaVersion ` | This is the version of Mona you're currently running. |
| `Deployment:Name ` | This Mona deployment's name as configured via the `-n` flag during setup. |
| `Identity:AdminIdentity:AadTenantId` | The Azure Active Directory tenant ID that Mona Administrators must belong to. By default, this is the Azure Active Directory tenant that the user that set up Mona belongs to. |
| `Identity:AdminIdentity:RoleName` | The name of the Azure Active Directory application role that Mona administrators must belong to. By default, the role name is `Mona Administrators`. |
