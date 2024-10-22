// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mona.AutoIntegration.Interfaces
{
    public interface IResourcePluginInterrogator<TResource>
    {
        string ResourceType { get; }

        Task<Plugin> InterrogateResourceAsync(AzureCredentials credentials, TResource resource);
        Task<IEnumerable<Plugin>> InterrogateResourceGroupAsync(AzureCredentials credentials, string subscriptionId, string resourceGroupName);
    }
}