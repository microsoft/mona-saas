# Configuration settings reference

These configuration settings are part of the Mona web app. For more information on how these settings are managed, see [this article](https://docs.microsoft.com/azure/app-service/configure-common#configure-app-settings). You can access these settings at any time by navigating to the Mona admin center (`/admin`), opening the __Mona SaaS configuration settings__ tab, and clicking __Manage configuration settings__.

> ⚠️ __Warning:__ These configuration settings control nearly every aspect of how Mona functions. Most of these settings are automatically configured during Mona setup. Exercise extreme caution when modifying these settings (especially in production). Updating any of these settings will automatically restart the Mona web app.

| Setting name | Description |
| --- | --- |
| `Deployment:AppInsightsInstrumentationKey` | This is the instrumentation key that Mona uses to publish telemetry to Application Insights. |
| `Deployment:AzureResourceGroupName` | This is the resource group that Mona has been deployed into. |
| `Deployment:AzureSubscriptionId` | The is the Azure subscription that Mona has been deployed into. |
| `Deployment:EventVersion ` | This is the identifier for the version of subscription events that Mona will publish to Event Grid. Supported event model versions are [`2021-05-01`](https://github.com/microsoft/mona-saas/tree/main/Mona.SaaS/Mona.SaaS.Core/Models/Events/V_2021_05_01) and [`2021-10-01`](https://github.com/microsoft/mona-saas/tree/main/Mona.SaaS/Mona.SaaS.Core/Models/Events/V_2021_10_01) (current). |
| `Deployment:IsTestModeEnabled ` | Are the test landing page and webhook endpoints enabled? |
| `Deployment:MonaVersion ` | This is the version of Mona that you're running. |
| `Deployment:Name ` | This is the name of your Mona deployment as configured via the `-n` flag during setup. This name must be globally unique, contain only lower-case alphanumeric characters, and be between 3 and 13 characters long. |
| `Identity:AdminIdentity:AadTenantId` | This is the Azure Active Directory (AAD) [tenant ID](https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-how-to-find-tenant) that Mona Administrators must belong to. By default, this is the Azure Active Directory tenant that the user that set up Mona belongs to. |
| `Identity:AdminIdentity:RoleName` | This is the name of the AAD [app role](https://docs.microsoft.com/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps#declare-roles-for-an-application) that Mona administrators must belong to. By default, the role name is `Mona Administrators`. |
| `Identity:AppIdentity:AadClientId` | This is Mona's AAD app registration [client ID](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app#register-an-application). Mona uses this information to authenticate to the various Marketplace APIs on your SaaS app's behalf. |
| `Identity:AppIdentity:AadClientSecret` | This is Mona's AAD app registration [client secret](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app#add-a-client-secret). Mona uses this information to authenticate to the various Marketplace APIs on your SaaS app's behalf. |
