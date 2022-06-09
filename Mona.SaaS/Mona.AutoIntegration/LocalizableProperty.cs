// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Mona.AutoIntegration
{
    public class LocalizableProperty
    {
        public IDictionary<string, string> LocalizedValues { get; set; } = new Dictionary<string, string>();

        public string DefaultValue { get; set; }

        public string GetLocalPropertyValue(string localeCode = null)
        {
            if (!string.IsNullOrEmpty(localeCode))
            {
                var matchingKey = LocalizedValues.Keys.FirstOrDefault(k => k.Equals(localeCode, StringComparison.InvariantCultureIgnoreCase));

                if (matchingKey == null)
                {
                    var localeCodeParts = localeCode.Split('-');

                    if (localeCodeParts.Length == 2)
                    {
                        matchingKey = LocalizedValues.Keys.FirstOrDefault(k => k.Equals(localeCodeParts[0], StringComparison.InvariantCultureIgnoreCase));
                    }
                }

                if (matchingKey != null)
                {
                    return LocalizedValues[matchingKey];
                }
            }

            return DefaultValue;
        }
    }
}