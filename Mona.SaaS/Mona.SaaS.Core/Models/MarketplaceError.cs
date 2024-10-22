// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Mona.SaaS.Core.Models
{
    /// <summary>
    /// Represents an error thrown by the Azure Marketplace.
    /// </summary>
    public class MarketplaceError
    {
        [JsonProperty("errorId")]
        [JsonPropertyName("errorId")]
        public string ErrorId { get; set; }

        [JsonProperty("errorCode")]
        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty("errorType")]
        [JsonPropertyName("errorType")]
        public string ErrorType { get; set; }

        [JsonProperty("errorMessage")]
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}