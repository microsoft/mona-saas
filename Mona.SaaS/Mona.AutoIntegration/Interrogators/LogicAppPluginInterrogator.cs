// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
                return (await logicAppClient.Workflows.ListByResourceGroupAsync(resourceGroupName))
                    .Where(w => (w.Tags?.Any() == true))
                    .Select(w => InterrogateResourceAsync(credentials, w).Result).ToList();
            }
        }
    }
}