// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Constants;

namespace Mona.SaaS.Core.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Merges [<paramref name="originalString"/>] with the provided [<paramref name="subscriptionId"/>] 
        /// using the [<see cref="ConfigurationFields.SubscriptionId"/>] field.
        /// </summary>
        /// <param name="originalString">The original string.</param>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <returns>The merged string.</returns>
        public static string WithSubscriptionId(this string originalString, string subscriptionId) =>
            originalString.Replace($"{{{ConfigurationFields.SubscriptionId}}}", subscriptionId);
    }
}