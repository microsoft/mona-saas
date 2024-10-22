// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Mona.SaaS.Core.Models
{
    /// <summary>
    /// Represents an error thrown by the Azure Marketplace.
    /// </summary>
    public class MarketplaceError
    {
        [JsonProperty("errorId")]
        public string ErrorId { get; set; }

        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty("errorType")]
        public string ErrorType { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}