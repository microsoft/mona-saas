// MICROSOFT CONFIDENTIAL INFORMATION
//
// Copyright © Microsoft Corporation
//
// Microsoft Corporation (or based on where you live, one of its affiliates) licenses this preview code for your internal testing purposes only.
//
// Microsoft provides the following preview code AS IS without warranty of any kind. The preview code is not supported under any Microsoft standard support program or services.
//
// Microsoft further disclaims all implied warranties including, without limitation, any implied warranties of merchantability or of fitness for a particular purpose. The entire risk arising out of the use or performance of the preview code remains with you.
//
// In no event shall Microsoft be liable for any damages whatsoever (including, without limitation, damages for loss of business profits, business interruption, loss of business information, or other pecuniary loss) arising out of the use of or inability to use the preview code, even if Microsoft has been advised of the possibility of such damages.

using Microsoft.Azure.Management.Logic;
using Microsoft.Azure.Management.Logic.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mona.AutoIntegration.Interrogators
{
    public class LogicAppPluginInterrogator : BaseResourcePluginInterrogator<Workflow>
    {
        public LogicAppPluginInterrogator() : base("Microsoft.Logic/workflows") { }

        public override Task<Plugin> InterrogateResourceAsync(AzureCredentials credentials, Workflow resource)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }

            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            var plugin = CreateBasePlugin(resource.Id, resource.Tags);

            plugin.Status = ((resource.State == "Enabled") ? PluginStatus.Enabled : PluginStatus.Disabled);
            plugin.EditorUrl = $"https://portal.azure.com/#@{credentials.TenantId}/resource{resource.Id}/designer";
            plugin.ManagementUrl = $"https://portal.azure.com/#@{credentials.TenantId}/resource{resource.Id}/logicApp";

            return Task.FromResult(plugin);
        }

        public async override Task<IEnumerable<Plugin>> InterrogateResourceGroupAsync(AzureCredentials credentials, string subscriptionId, string resourceGroupName)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }

            if (string.IsNullOrEmpty(subscriptionId))
            {
                throw new ArgumentNullException(nameof(subscriptionId));
            }

            if (string.IsNullOrEmpty(resourceGroupName))
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }

            using (var logicAppClient = new LogicManagementClient(credentials) { SubscriptionId = subscriptionId })
            {
                return (await logicAppClient.Workflows.ListByResourceGroupAsync(resourceGroupName)).Select(
                    w => InterrogateResourceAsync(credentials, w).Result).ToList();
            }
        }
    }
}