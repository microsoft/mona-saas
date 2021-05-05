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