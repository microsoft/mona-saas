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

namespace Mona.SaaS.Web.Models.Admin.LogicApps
{
    using Newtonsoft.Json;

    public class MarketplaceAuthenticationMetadata
    {
        [JsonProperty("audience")]
        public string Audience { get; set; } = "20e940b3-4c77-4b0b-9a53-9e16a1b010a7"; // Marketplace API static audience value

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("credentialType")]
        public string CredentialType { get; set; } = "Secret";

        [JsonProperty("secret")]
        public string Secret { get; set; }

        [JsonProperty("tenant")]
        public string TenantId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = "ActiveDirectoryOAuth";
    }
}
