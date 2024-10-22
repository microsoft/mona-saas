// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Mona.AutoIntegration
{
    public class Plugin
    {
        public string Id { get; set; }

        public string Version { get; set; }

        public LocalizableProperty Description { get; set; } = new LocalizableProperty();

        public LocalizableProperty DisplayName { get; set; } = new LocalizableProperty();

        public string EditorUrl { get; set; }

        public string ManagementUrl { get; set; }

        public string TriggerEventType { get; set; }

        public string TriggerEventVersion { get; set; }

        public string PluginType { get; set; }

        public PluginStatus Status { get; set; }
    }
}