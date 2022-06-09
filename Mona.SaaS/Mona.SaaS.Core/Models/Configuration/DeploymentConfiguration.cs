// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core.Constants;
using System.ComponentModel.DataAnnotations;

namespace Mona.SaaS.Core.Models.Configuration
{
    /// <summary>
    /// Provides information regarding the deployment of this Mona application.
    /// </summary>
    public class DeploymentConfiguration
    {
        /// <summary>
        /// Gets/sets the name of this Mona deployment.
        /// </summary>
        [Required]
        public string Name { get; set; }

        public string EventVersion { get; set; } = EventVersions.V_2021_10_01;

        /// <summary>
        /// Gets/sets this deployment's Mona version.
        /// </summary>
        [Required]
        public string MonaVersion { get; set; }

        /// <summary>
        /// Gets/sets the instrumentation key needed to push events and metrics to App Insights.
        /// </summary>
        [Required]
        public string AppInsightsInstrumentationKey { get; set; }

        /// <summary>
        /// Gets/sets the Azure subscription ID that Mona has been deployed to.
        /// </summary>
        [Required]
        public string AzureSubscriptionId { get; set; }

        /// <summary>
        /// Gets/sets the Azure resource group name that Mona has been deployed to.
        /// </summary>
        [Required]
        public string AzureResourceGroupName { get; set; }

        /// <summary>
        /// Gets/sets the Azure resource group name where Mona core components reside.
        /// </summary>
        public string MonaResourceGroupName { get; set; }

        /// <summary>
        /// Gets/sets the Azure resource group name where Mona integrations reside.
        /// </summary>
        public string IntegrationResourceGroupName { get; set; }

        /// <summary>
        /// Gets/sets whether or not this Mona deployment currently supports test mode.
        /// </summary>
        public bool IsTestModeEnabled { get; set; }

        /// <summary>
        /// Gets/sets whether or not to share subscription details with the purchase confirmation page upon purchase completion.
        /// </summary>
        public bool SendSubscriptionDetailsToPurchaseConfirmationPage { get; set; } = true;

        /// <summary>
        /// Gets/sets whether or not to share subscription details with the subscription configuration page.
        /// </summary>
        public bool SendSubscriptionDetailsToSubscriptionConfigurationPage { get; set; }
    }
}