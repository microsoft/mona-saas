using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.EventProcessing.Interfaces;
using Mona.SaaS.Web.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Mona.SaaS.Web.Tests
{
    public class SubscriptionControllerTests
    {
        [Fact]
        public async Task GetLiveLandingPage_WithNoMarketplaceToken_WithConfiguredMarketingPage_ShouldRedirectToMarketingPage()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionRepo = new Mock<ISubscriptionRepository>();

            var offerConfig = GetDefaultOfferConfiguration();

            var controllerUt = new SubscriptionController(
                mockDeployConfig.Object, offerConfig, mockLogger.Object, mockMpOperationService.Object,
                mockMpSubscriptionService.Object, mockEventPublisher.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.GetLiveLandingPageAsync();

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<RedirectResult>();
            (actionResult as RedirectResult).Url.Should().Be(offerConfig.OfferMarketingPageUrl);
        }

        [Fact]
        public async Task GetLiveLandingPage_WithNoMarketplaceToken_WithoutConfiguredMarketingPage_ShouldRespondNotFound()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionRepo = new Mock<ISubscriptionRepository>();

            var offerConfig = GetDefaultOfferConfiguration();

            offerConfig.OfferMarketingPageUrl = null;

            var controllerUt = new SubscriptionController(
                mockDeployConfig.Object, offerConfig, mockLogger.Object, mockMpOperationService.Object,
                mockMpSubscriptionService.Object, mockEventPublisher.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.GetLiveLandingPageAsync();

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetLiveLandingPage_WithIncompleteSetup_ShouldRedirectToSetup()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionRepo = new Mock<ISubscriptionRepository>();

            var offerConfig = GetDefaultOfferConfiguration();

            offerConfig.IsSetupComplete = false;

            var controllerUt = new SubscriptionController(
                mockDeployConfig.Object, offerConfig, mockLogger.Object, mockMpOperationService.Object,
                mockMpSubscriptionService.Object, mockEventPublisher.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.GetLiveLandingPageAsync();

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<RedirectToRouteResult>();
            (actionResult as RedirectToRouteResult).RouteName.Should().Be("setup");
        }

        private Mock<IOptionsSnapshot<T>> GetOptionsSnapshotMock<T>(T options) where T : class
        {
            var mockConfig = new Mock<IOptionsSnapshot<T>>();
            mockConfig.SetupGet(s => s.Value).Returns(options);

            return mockConfig;
        }

        private DeploymentConfiguration GetDefaultDeploymentConfiguration() =>
            new DeploymentConfiguration
            {
                AppInsightsConnectionString = $"InstrumentationKey={Guid.NewGuid()}",
                AzureResourceGroupName = "mona-test-rg",
                AzureSubscriptionId = Guid.NewGuid().ToString(),
                IsTestModeEnabled = true,
                MonaVersion = "1.0",
                Name = "Mona SaaS Testing"
            };

        private OfferConfiguration GetDefaultOfferConfiguration() =>
            new OfferConfiguration
            {
                IsSetupComplete = true,
                OfferDisplayName = "Mona SaaS Testing",
                OfferMarketingPageUrl = "https://github.com/microsoft/mona-saas",
                OfferMarketplaceListingUrl = "https://azure.microsoft.com",
                PublisherContactPageUrl = "https://support.microsoft.com/contactus",
                PublisherCopyrightNotice = $"© Microsoft {DateTime.UtcNow.Year}",
                PublisherDisplayName = "Microsoft",
                PublisherHomePageUrl = "https://microsoft.com",
                PublisherPrivacyNoticePageUrl = "https://privacy.microsoft.com/en-us/privacystatement",
                SubscriptionConfigurationUrl = "https://azure.microsoft.com/mona/configure/{subscription-id}",
                SubscriptionPurchaseConfirmationUrl = "https://azure.microsoft.com/mona/purchase/{subscription-id}"
            };
    }
}
