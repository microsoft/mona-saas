// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Models.Configuration;

namespace Mona.SaaS.Web.Models
{
    public class SetupModel
    {
        public SetupModel() =>
            Publisher = new PublisherConfiguration();

        public SetupModel(PublisherConfiguration publisherConfig) =>
            Publisher = publisherConfig;

        /// <summary>
        /// Gets/sets the publisher to be set up.
        /// </summary>
        public PublisherConfiguration Publisher { get; set; }

        public string MonaVersion { get; set; }
    }
}