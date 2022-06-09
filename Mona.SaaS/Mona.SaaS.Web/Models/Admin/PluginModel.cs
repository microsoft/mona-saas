// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.AutoIntegration;

namespace Mona.SaaS.Web.Models.Admin
{
    public class PluginModel
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string EditorUrl { get; set; }
        public string ManagementUrl { get; set; }
        public string PluginType { get; set; }
        public string TriggerEventType { get; set; }
        public string TriggerEventVersion { get; set; }

        public PluginStatus Status { get; set; }
    }
}