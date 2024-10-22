// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Mona.AutoIntegration.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Mona.AutoIntegration.Interrogators
{
    public abstract class BaseResourcePluginInterrogator<TResource> : IResourcePluginInterrogator<TResource>
    {
        public static class KnownTags
        {
            public const string DisplayName = "mona:name";
            public const string Description = "mona:description";
            public const string Version = "mona:version";
            public const string TriggerEventType = "mona:event-type";
            public const string TriggerEventVersion = "mona:event-version";
        }

        protected BaseResourcePluginInterrogator() { }

        protected BaseResourcePluginInterrogator(string resourceType) => ResourceType = resourceType;

        public string ResourceType { get; protected set; }

        public abstract Task<Plugin> InterrogateResourceAsync(AzureCredentials credentials, TResource resource);

        public abstract Task<IEnumerable<Plugin>> InterrogateResourceGroupAsync(AzureCredentials credentials, string subscriptionId, string resourceGroupName);

        protected virtual Plugin CreateBasePlugin(string resourceId, IDictionary<string, string> resourceTags)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                throw new ArgumentNullException(nameof(resourceId));
            }

            // TODO: Come back and revisit this whole auto-integration thing in the future. This approach worked just fine
            // when it was just Mona logic apps to worry about but now that we have Turnstile in the picture, the logic behind
            // this original idea starts to fall apart. I honestly wonder if we shouldn't just rip all of this out and have the user
            // navigate to the resource group to find the appropriate workflow. This is going to become even more unwieldy with integration
            // packs so we need to step back and rethink all of this at some point in the future. For now, we'll just return a null Plugin
            // if we can't parse this into a Mona workflow.

            if (resourceTags != null)
            {
                var plugin = new Plugin { Id = resourceId, PluginType = ResourceType };

                plugin.DisplayName = CreateLocalizableProperty(KnownTags.DisplayName, resourceTags);
                plugin.Description = CreateLocalizableProperty(KnownTags.Description, resourceTags);
                plugin.TriggerEventType = resourceTags.FirstOrDefault(t => (t.Key == KnownTags.TriggerEventType)).Value;
                plugin.TriggerEventVersion = resourceTags.FirstOrDefault(t => (t.Key == KnownTags.TriggerEventVersion)).Value;
                plugin.Version = resourceTags.FirstOrDefault(t => (t.Key == KnownTags.Version)).Value;

                return plugin;
            }
            else
            {
                return null;
            }
        }

        private LocalizableProperty CreateLocalizableProperty(string baseTagName, IDictionary<string, string> resourceTags)
        {
            var localProperty = new LocalizableProperty();
            var baseTagNameSegmentCount = baseTagName.Split(':').Length;

            foreach (var tagName in resourceTags.Keys.Where(k => k.StartsWith(baseTagName)))
            {
                if (baseTagName == tagName)
                {
                    localProperty.DefaultValue = resourceTags[tagName];
                }
                else
                {
                    var tagNameSegments = tagName.Split(':');

                    if (tagNameSegments.Length == (baseTagNameSegmentCount + 1))
                    {
                        var cultureInfo = TryParseLocaleCode(tagNameSegments.Last());

                        if (cultureInfo != null)
                        {
                            localProperty.LocalizedValues[cultureInfo.Name] = resourceTags[tagName];
                        }
                    }
                }
            }

            return localProperty;
        }

        private CultureInfo TryParseLocaleCode(string localeCode)
        {
            try
            {
                return CultureInfo.GetCultureInfo(localeCode);
            }
            catch (CultureNotFoundException)
            {
                return null;
            }
        }
    }
}