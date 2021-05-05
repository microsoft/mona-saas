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