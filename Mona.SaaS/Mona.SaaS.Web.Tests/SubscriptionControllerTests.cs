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

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Mona.SaaS.Core.Constants;
using Mona.SaaS.Core.Enumerations;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Core.Models.Events;
using V_2021_10_01 = Mona.SaaS.Core.Models.Events.V_2021_10_01;
using Mona.SaaS.Web.Controllers;
using Mona.SaaS.Web.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Mona.SaaS.Web.Tests
{
    public class SubscriptionControllerTests
    {
        // TODO: Unit testing strategy for older event models (e.g., 2021-05-01)

        private IPublisherConfigurationStore GetDefaultPublisherConfigurationStore() =>
            GetPublisherConfigurationStore(GetDefaultPublisherConfiguration());

        private IPublisherConfigurationStore GetPublisherConfigurationStore(PublisherConfiguration publisherConfig)
        {
            var mockPublisherConfigStore = new Mock<IPublisherConfigurationStore>();

            mockPublisherConfigStore
                .Setup(pcs => pcs.GetPublisherConfiguration())
                .Returns(Task.FromResult(publisherConfig));

            return mockPublisherConfigStore.Object;
        }

        [Fact]
        public async Task PostLiveLandingPage_GivenValidSubscriptionId_ShouldPublishEvent_AndRedirectToPurchaseConfirmationUrl()
        {
            var testToken = Guid.NewGuid().ToString();
            var testSubscriptionToken = Guid.NewGuid().ToString();
            var testUserName = "Clippy";

            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();     
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockHttpContext = new Mock<HttpContext>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            V_2021_10_01.SubscriptionPurchased purchasedEvent = null;

            mockEventPublisher
                .Setup(ep => ep.PublishEventAsync(It.IsAny<V_2021_10_01.SubscriptionPurchased>()))
                .Callback<V_2021_10_01.SubscriptionPurchased>(sp => purchasedEvent = sp);

            mockHttpContext.SetupGet(hc => hc.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.SetupGet(hc => hc.User.Claims).Returns(new Claim[] { new Claim("name", testUserName) });
            mockMpSubscriptionService.Setup(ss => ss.GetSubscriptionAsync(testSubscription.SubscriptionId)).Returns(Task.FromResult(testSubscription));
            mockSubscriptionStagingCache.Setup(sc => sc.PutSubscriptionAsync(testSubscription)).Returns(Task.FromResult(testSubscriptionToken));

            var controllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            var controllerUt = new SubscriptionController(
                mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
                mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object)
            { ControllerContext = controllerContext };

            var lpModel = new LandingPageModel { SubscriptionId = testSubscription.SubscriptionId };

            var actionResult = await controllerUt.PostLiveLandingPageAsync(lpModel);

            var expectedRedirectUrl =
                publisherConfig.SubscriptionPurchaseConfirmationUrl.Replace("{subscription-id}", testSubscription.SubscriptionId) +
                $"?{SubscriptionController.SubscriptionDetailQueryParameter}={testSubscriptionToken}";

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<RedirectResult>();
            (actionResult as RedirectResult).Url.Should().Be(expectedRedirectUrl);

            purchasedEvent.Should().NotBeNull();
            purchasedEvent.EventType = EventTypes.SubscriptionPurchased;
            purchasedEvent.Subscription.Should().NotBeNull();
            purchasedEvent.Subscription.Should().BeEquivalentTo(new FlatSubscription(testSubscription));
        }

        [Fact]
        public async Task PostTestLandingPage_GivenValidSubscriptionId_ShouldPublishEvent_AndRedirectToPurchaseConfirmationUrl()
        {
            var testToken = Guid.NewGuid().ToString();
            var testSubscriptionToken = Guid.NewGuid().ToString();
            var testUserName = "Clippy";

            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var mockHttpContext = new Mock<HttpContext>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            testSubscription.IsTest = true;

            V_2021_10_01.SubscriptionPurchased purchasedEvent = null;

            mockEventPublisher
                .Setup(ep => ep.PublishEventAsync(It.IsAny<V_2021_10_01.SubscriptionPurchased>()))
                .Callback<V_2021_10_01.SubscriptionPurchased>(sp => purchasedEvent = sp);

            mockHttpContext.SetupGet(hc => hc.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.SetupGet(hc => hc.User.Claims).Returns(new Claim[] { new Claim("name", testUserName) });
            mockSubscriptionRepo.Setup(sr => sr.GetSubscriptionAsync(testSubscription.SubscriptionId)).Returns(Task.FromResult(testSubscription));
            mockSubscriptionStagingCache.Setup(sc => sc.PutSubscriptionAsync(testSubscription)).Returns(Task.FromResult(testSubscriptionToken));

            var controllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            var controllerUt = new SubscriptionController(
                mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
                mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object)
            { ControllerContext = controllerContext };

            var lpModel = new LandingPageModel { SubscriptionId = testSubscription.SubscriptionId };

            var actionResult = await controllerUt.PostTestLandingPageAsync(lpModel);

            var expectedRedirectUrl =
                publisherConfig.SubscriptionPurchaseConfirmationUrl.Replace("{subscription-id}", testSubscription.SubscriptionId) +
                $"?{SubscriptionController.SubscriptionDetailQueryParameter}={testSubscriptionToken}";

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<RedirectResult>();
            (actionResult as RedirectResult).Url.Should().Be(expectedRedirectUrl);

            purchasedEvent.Should().NotBeNull();
            purchasedEvent.EventType = EventTypes.SubscriptionPurchased;
            purchasedEvent.Subscription.Should().NotBeNull();
            purchasedEvent.Subscription.Should().BeEquivalentTo(new FlatSubscription(testSubscription));
        }

        [Fact]
        public async Task PostLiveLandingPage_GivenInvalidSubscriptionId_ShouldRespondOk_WithErrorCode()
        {
            var testToken = Guid.NewGuid().ToString();
            var testSubscriptionToken = Guid.NewGuid().ToString();
            var testUserName = "Clippy";

            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var mockHttpContext = new Mock<HttpContext>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            mockHttpContext.SetupGet(hc => hc.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.SetupGet(hc => hc.User.Claims).Returns(new Claim[] { new Claim("name", testUserName) });
            mockMpSubscriptionService.Setup(ss => ss.GetSubscriptionAsync(testSubscription.SubscriptionId)).Returns(Task.FromResult(null as Subscription));

            var controllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            var controllerUt = new SubscriptionController(
                mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
                mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var lpModel = new LandingPageModel { SubscriptionId = testSubscription.SubscriptionId };

            var actionResult = await controllerUt.PostLiveLandingPageAsync(lpModel);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<ViewResult>();

            var viewResult = actionResult as ViewResult;

            viewResult.ViewName.Should().Be("Index");
            viewResult.Model.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<LandingPageModel>();

            lpModel = viewResult.Model as LandingPageModel;

            lpModel.ErrorCode.Should().Be(SubscriptionController.ErrorCodes.SubscriptionActivationFailed);
            lpModel.InTestMode.Should().Be(false);
            lpModel.PublisherContactPageUrl.Should().Be(publisherConfig.PublisherContactPageUrl);
            lpModel.PublisherCopyrightNotice.Should().Be(publisherConfig.PublisherCopyrightNotice);
            lpModel.PublisherDisplayName.Should().Be(publisherConfig.PublisherDisplayName);
            lpModel.PublisherHomePageUrl.Should().Be(publisherConfig.PublisherHomePageUrl);
            lpModel.PublisherPrivacyNoticePageUrl.Should().Be(publisherConfig.PublisherPrivacyNoticePageUrl);
        }

        [Fact]
        public async Task PostTestLandingPage_GivenInvalidSubscriptionId_ShouldRespondOk_WithErrorCode()
        {
            var testToken = Guid.NewGuid().ToString();
            var testUserName = "Clippy";

            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var mockHttpContext = new Mock<HttpContext>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            mockHttpContext.SetupGet(hc => hc.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.SetupGet(hc => hc.User.Claims).Returns(new Claim[] { new Claim("name", testUserName) });
            mockSubscriptionRepo.Setup(sr => sr.GetSubscriptionAsync(testSubscription.SubscriptionId)).Returns(Task.FromResult(null as Subscription));

            var controllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object)
            { ControllerContext = controllerContext };

            var lpModel = new LandingPageModel { SubscriptionId = testSubscription.SubscriptionId };

            var actionResult = await controllerUt.PostTestLandingPageAsync(lpModel);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<ViewResult>();

            var viewResult = actionResult as ViewResult;

            viewResult.ViewName.Should().Be("Index");
            viewResult.Model.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<LandingPageModel>();

            lpModel = viewResult.Model as LandingPageModel;

            lpModel.ErrorCode.Should().Be(SubscriptionController.ErrorCodes.SubscriptionActivationFailed);
            lpModel.InTestMode.Should().Be(true);
            lpModel.PublisherContactPageUrl.Should().Be(publisherConfig.PublisherContactPageUrl);
            lpModel.PublisherCopyrightNotice.Should().Be(publisherConfig.PublisherCopyrightNotice);
            lpModel.PublisherDisplayName.Should().Be(publisherConfig.PublisherDisplayName);
            lpModel.PublisherHomePageUrl.Should().Be(publisherConfig.PublisherHomePageUrl);
            lpModel.PublisherPrivacyNoticePageUrl.Should().Be(publisherConfig.PublisherPrivacyNoticePageUrl);
            lpModel.UserFriendlyName.Should().Be(testUserName);
        }

        [Fact]
        public async Task GetLiveLandingPage_GivenNoMarketplaceToken_AndConfiguredMarketingPage_ShouldRedirectToMarketingPage()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var publisherConfig = GetDefaultPublisherConfiguration();

            var controllerUt = new SubscriptionController(
                mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
               mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.GetLiveLandingPageAsync();

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<RedirectResult>();
            (actionResult as RedirectResult).Url.Should().Be(publisherConfig.PublisherHomePageUrl);
        }

        [Fact]
        public async Task GetLiveLandingPage_GivenNoMarketplaceToken_AndNoConfiguredMarketingPage_ShouldRespondNotFound()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();

            var publisherConfig = GetDefaultPublisherConfiguration();

            publisherConfig.PublisherHomePageUrl = null;

            var controllerUt = new SubscriptionController(
                mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
                mockMpSubscriptionService.Object, GetPublisherConfigurationStore(publisherConfig),
                mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.GetLiveLandingPageAsync();

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetLiveLandingPage_GivenUnresolvableMarketplaceToken_AndAuthedUser_ShouldRespondOk_WithErrorCode()
        {
            var testToken = Guid.NewGuid().ToString();
            var testUserName = "Clippy";

            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var mockHttpContext = new Mock<HttpContext>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            mockHttpContext.SetupGet(hc => hc.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.SetupGet(hc => hc.User.Claims).Returns(new Claim[] { new Claim("name", testUserName) });
            mockMpSubscriptionService.Setup(ss => ss.ResolveSubscriptionTokenAsync(testToken)).Returns(Task.FromResult(null as Subscription));

            var controllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            var controllerUt = new SubscriptionController(
               mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
               mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object)
            { ControllerContext = controllerContext };

            var actionResult = await controllerUt.GetLiveLandingPageAsync(testToken);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<ViewResult>();

            var viewResult = actionResult as ViewResult;

            viewResult.ViewName.Should().Be("Index");
            viewResult.Model.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<LandingPageModel>();

            var lpModel = viewResult.Model as LandingPageModel;

            lpModel.ErrorCode.Should().Be(SubscriptionController.ErrorCodes.UnableToResolveMarketplaceToken);
            lpModel.InTestMode.Should().Be(false);
            lpModel.PublisherContactPageUrl.Should().Be(publisherConfig.PublisherContactPageUrl);
            lpModel.PublisherCopyrightNotice.Should().Be(publisherConfig.PublisherCopyrightNotice);
            lpModel.PublisherDisplayName.Should().Be(publisherConfig.PublisherDisplayName);
            lpModel.PublisherHomePageUrl.Should().Be(publisherConfig.PublisherHomePageUrl);
            lpModel.PublisherPrivacyNoticePageUrl.Should().Be(publisherConfig.PublisherPrivacyNoticePageUrl);
            lpModel.UserFriendlyName.Should().Be(testUserName);
        }

        [Fact]
        public async Task GetTestLandingPage_GivenTestModeDisabled_ShouldRespondNotFound()
        {
            var deployConfig = GetDefaultDeploymentConfiguration();

            deployConfig.IsTestModeEnabled = false;

            var mockDeployConfig = GetOptionsSnapshotMock(deployConfig);
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.GetTestLandingPageAsync();

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetTestLandingPage_GivenAuthedUser_AndExistingSubscription_ShouldRedirectToSubscriptionConfigurationUrl()
        {
            var testToken = Guid.NewGuid().ToString();
            var testSubscriptionToken = Guid.NewGuid().ToString();
            var testUserName = "Clippy";

            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var mockHttpContext = new Mock<HttpContext>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            testSubscription.Status = SubscriptionStatus.Active;

            mockHttpContext.SetupGet(hc => hc.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.SetupGet(hc => hc.User.Claims).Returns(new Claim[] { new Claim("name", testUserName) });
            mockMpSubscriptionService.Setup(ss => ss.ResolveSubscriptionTokenAsync(testToken)).Returns(Task.FromResult(testSubscription));
            mockSubscriptionStagingCache.Setup(sc => sc.PutSubscriptionAsync(testSubscription)).Returns(Task.FromResult(testSubscriptionToken));

            var controllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object)
            { ControllerContext = controllerContext };

            var actionResult = await controllerUt.GetLiveLandingPageAsync(testToken);

            var expectedRedirectUrl =
                publisherConfig.SubscriptionConfigurationUrl.Replace("{subscription-id}", testSubscription.SubscriptionId) +
                $"?{SubscriptionController.SubscriptionDetailQueryParameter}={testSubscriptionToken}";

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<RedirectResult>();
            (actionResult as RedirectResult).Url.Should().Be(expectedRedirectUrl);
        }

        [Fact]
        public async Task GetLiveLandingPage_GivenResolvableMarketplaceToken_AndAuthedUser_AndExistingSubscription_ShouldRedirectToSubscriptionConfigurationUrl()
        {
            var testToken = Guid.NewGuid().ToString();
            var testSubscriptionToken = Guid.NewGuid().ToString();
            var testUserName = "Clippy";

            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var mockHttpContext = new Mock<HttpContext>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            testSubscription.Status = SubscriptionStatus.Active;

            mockHttpContext.SetupGet(hc => hc.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.SetupGet(hc => hc.User.Claims).Returns(new Claim[] { new Claim("name", testUserName) });
            mockMpSubscriptionService.Setup(ss => ss.ResolveSubscriptionTokenAsync(testToken)).Returns(Task.FromResult(testSubscription));
            mockSubscriptionStagingCache.Setup(sc => sc.PutSubscriptionAsync(testSubscription)).Returns(Task.FromResult(testSubscriptionToken));

            var controllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object)
            { ControllerContext = controllerContext };

            var actionResult = await controllerUt.GetLiveLandingPageAsync(testToken);

            var expectedRedirectUrl =
                publisherConfig.SubscriptionConfigurationUrl.Replace("{subscription-id}", testSubscription.SubscriptionId) +
                $"?{SubscriptionController.SubscriptionDetailQueryParameter}={testSubscriptionToken}";

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<RedirectResult>();
            (actionResult as RedirectResult).Url.Should().Be(expectedRedirectUrl);
        }

        [Fact]
        public async Task GetTestLandingPage_GivenAuthedUser_ShouldRespondOk_WithSubscriptionDetails()
        {
            var testUserName = "Clippy";

            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var mockHttpContext = new Mock<HttpContext>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            mockHttpContext.SetupGet(hc => hc.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.SetupGet(hc => hc.User.Claims).Returns(new Claim[] { new Claim("name", testUserName) });
            mockHttpContext.SetupGet(hc => hc.Request.Query).Returns(new QueryCollection());

            var controllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            var controllerUt = new SubscriptionController(
               mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
               mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object)
            { ControllerContext = controllerContext };

            var actionResult = await controllerUt.GetTestLandingPageAsync();

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<ViewResult>();

            var viewResult = actionResult as ViewResult;

            viewResult.ViewName.Should().Be("Index");
            viewResult.Model.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<LandingPageModel>();

            var lpModel = viewResult.Model as LandingPageModel;

            lpModel.InTestMode.Should().Be(true);
            lpModel.BeneficiaryEmailAddress.Should().NotBeNullOrEmpty();
            lpModel.PurchaserEmailAddress.Should().NotBeNullOrEmpty();
            lpModel.OfferId.Should().NotBeNullOrEmpty();
            lpModel.PlanId.Should().NotBeNullOrEmpty();
            lpModel.PublisherContactPageUrl.Should().Be(publisherConfig.PublisherContactPageUrl);
            lpModel.PublisherCopyrightNotice.Should().Be(publisherConfig.PublisherCopyrightNotice);
            lpModel.PublisherDisplayName.Should().Be(publisherConfig.PublisherDisplayName);
            lpModel.PublisherHomePageUrl.Should().Be(publisherConfig.PublisherHomePageUrl);
            lpModel.PublisherPrivacyNoticePageUrl.Should().Be(publisherConfig.PublisherPrivacyNoticePageUrl);
            lpModel.SubscriptionId.Should().NotBeNullOrEmpty();
            lpModel.SubscriptionName.Should().NotBeNullOrEmpty();
            lpModel.UserFriendlyName.Should().Be(testUserName);
        }

        [Fact]
        public async Task GetTestLandingPage_GivenAuthedUser_AndTestSubscriptionOverrides_ShouldRespondOk_WithSubscriptionDetails()
        {
            var testUserName = "Clippy";

            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var mockHttpContext = new Mock<HttpContext>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            var subOverrideParameters = CreateTestSubscriptionQueryParameters();
            var subOverrideQueryCollection = new QueryCollection(subOverrideParameters);

            mockHttpContext.SetupGet(hc => hc.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.SetupGet(hc => hc.User.Claims).Returns(new Claim[] { new Claim("name", testUserName) });
            mockHttpContext.SetupGet(hc => hc.Request.Query).Returns(subOverrideQueryCollection);

            var controllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object)
            { ControllerContext = controllerContext };

            var actionResult = await controllerUt.GetTestLandingPageAsync();

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<ViewResult>();

            var viewResult = actionResult as ViewResult;

            viewResult.ViewName.Should().Be("Index");
            viewResult.Model.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<LandingPageModel>();

            var lpModel = viewResult.Model as LandingPageModel;

            lpModel.InTestMode.Should().Be(true);
            lpModel.BeneficiaryEmailAddress.Should().Be(subOverrideParameters[SubscriptionController.TestSubscriptionParameterNames.BeneficiaryUserEmail].First());
            lpModel.PurchaserEmailAddress.Should().Be(subOverrideParameters[SubscriptionController.TestSubscriptionParameterNames.PurchaserUserEmail].First());
            lpModel.OfferId.Should().Be(subOverrideParameters[SubscriptionController.TestSubscriptionParameterNames.OfferId].First());
            lpModel.PlanId.Should().Be(subOverrideParameters[SubscriptionController.TestSubscriptionParameterNames.PlanId].First());
            lpModel.PublisherContactPageUrl.Should().Be(publisherConfig.PublisherContactPageUrl);
            lpModel.PublisherCopyrightNotice.Should().Be(publisherConfig.PublisherCopyrightNotice);
            lpModel.PublisherDisplayName.Should().Be(publisherConfig.PublisherDisplayName);
            lpModel.PublisherHomePageUrl.Should().Be(publisherConfig.PublisherHomePageUrl);
            lpModel.PublisherPrivacyNoticePageUrl.Should().Be(publisherConfig.PublisherPrivacyNoticePageUrl);
            lpModel.SubscriptionId.Should().Be(subOverrideParameters[SubscriptionController.TestSubscriptionParameterNames.SubscriptionId].First());
            lpModel.SubscriptionName.Should().Be(subOverrideParameters[SubscriptionController.TestSubscriptionParameterNames.SubscriptionName].First());
            lpModel.UserFriendlyName.Should().Be(testUserName);
        }

        [Fact]
        public async Task GetLiveLandingPage_GivenResolvableMarketplaceToken_AndAuthenticatedUser_AndNewSubscription_ShouldRespondOk_WithSubscriptionDetails()
        {
            var testToken = Guid.NewGuid().ToString();
            var testUserName = "Clippy";

            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var mockHttpContext = new Mock<HttpContext>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            mockHttpContext.SetupGet(hc => hc.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.SetupGet(hc => hc.User.Claims).Returns(new Claim[] { new Claim("name", testUserName) });
            mockMpSubscriptionService.Setup(ss => ss.ResolveSubscriptionTokenAsync(testToken)).Returns(Task.FromResult(testSubscription));

            var controllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            var controllerUt = new SubscriptionController(
               mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
               mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object)
            { ControllerContext = controllerContext };

            var actionResult = await controllerUt.GetLiveLandingPageAsync(testToken);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<ViewResult>();

            var viewResult = actionResult as ViewResult;

            viewResult.ViewName.Should().Be("Index");
            viewResult.Model.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<LandingPageModel>();

            var lpModel = viewResult.Model as LandingPageModel;

            lpModel.InTestMode.Should().Be(false);
            lpModel.BeneficiaryEmailAddress.Should().Be(testSubscription.Beneficiary.UserEmail);
            lpModel.PurchaserEmailAddress.Should().Be(testSubscription.Purchaser.UserEmail);
            lpModel.IsFreeTrial.Should().Be(testSubscription.IsFreeTrial);
            lpModel.OfferId.Should().Be(testSubscription.OfferId);
            lpModel.PlanId.Should().Be(testSubscription.PlanId);
            lpModel.PublisherContactPageUrl.Should().Be(publisherConfig.PublisherContactPageUrl);
            lpModel.PublisherCopyrightNotice.Should().Be(publisherConfig.PublisherCopyrightNotice);
            lpModel.PublisherDisplayName.Should().Be(publisherConfig.PublisherDisplayName);
            lpModel.PublisherHomePageUrl.Should().Be(publisherConfig.PublisherHomePageUrl);
            lpModel.PublisherPrivacyNoticePageUrl.Should().Be(publisherConfig.PublisherPrivacyNoticePageUrl);
            lpModel.SeatQuantity.Should().Be(testSubscription.SeatQuantity);
            lpModel.SubscriptionId.Should().Be(testSubscription.SubscriptionId);
            lpModel.SubscriptionName.Should().Be(testSubscription.SubscriptionName);
            lpModel.UserFriendlyName.Should().Be(testUserName);
        }

        [Fact]
        public async Task GetTestLandingPage_GivenUnauthedUser_ShouldChallenge()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var mockHttpContext = new Mock<HttpContext>();
            var publisherConfig = GetDefaultPublisherConfiguration();

            mockHttpContext.SetupGet(hc => hc.User.Identity.IsAuthenticated).Returns(false);

            var controllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object)
            { ControllerContext = controllerContext };

            var actionResult = await controllerUt.GetTestLandingPageAsync();

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<ChallengeResult>();
        }

        [Fact]
        public async Task GetLiveLandingPage_GivenMarketplaceToken_AndUnauthenticatedUser_ShouldChallenge()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var mockHttpContext = new Mock<HttpContext>();
            var publisherConfig = GetDefaultPublisherConfiguration();

            mockHttpContext.SetupGet(hc => hc.User.Identity.IsAuthenticated).Returns(false);

            var controllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object)
            { ControllerContext = controllerContext };

            var actionResult = await controllerUt.GetLiveLandingPageAsync(Guid.NewGuid().ToString());

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<ChallengeResult>();
        }

        [Fact]
        public async Task GetTestLandingPage_GivenIncompleteMonaSetup_ShouldRedirectToSetup()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();

            var publisherConfig = GetDefaultPublisherConfiguration();

            publisherConfig.IsSetupComplete = false;

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetPublisherConfigurationStore(null), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.GetTestLandingPageAsync();

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<RedirectToRouteResult>();
            (actionResult as RedirectToRouteResult).RouteName.Should().Be("setup");
        }

        [Fact]
        public async Task GetLiveLandingPage_GivenIncompleteMonaSetup_ShouldRedirectToSetup()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();

            var publisherConfig = GetDefaultPublisherConfiguration();

            publisherConfig.IsSetupComplete = false;

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetPublisherConfigurationStore(null), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.GetLiveLandingPageAsync();

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<RedirectToRouteResult>();
            (actionResult as RedirectToRouteResult).RouteName.Should().Be("setup");
        }

        [Fact]
        public async Task PostLiveWebhookNotification_GivenVerifiableReinstatementNotification_ShouldPublishEvent_AndRespondOk()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            var testWhNotification = new WebhookNotification
            {
                ActionType = MarketplaceActionTypes.Reinstate,
                ActivityId = Guid.NewGuid().ToString(),
                OfferId = testSubscription.OfferId,
                OperationId = Guid.NewGuid().ToString(),
                PlanId = testSubscription.PlanId,
                PublisherId = Guid.NewGuid().ToString(),
                SeatQuantity = testSubscription.SeatQuantity,
                SubscriptionId = testSubscription.SubscriptionId,
                Timestamp = DateTime.UtcNow
            };

            var testOperation = new SubscriptionOperation
            {
                OperationId = testWhNotification.OperationId,
                OperationType = SubscriptionOperationType.Reinstate,
                PlanId = testWhNotification.PlanId,
                SeatQuantity = testWhNotification.SeatQuantity,
                SubscriptionId = testWhNotification.SubscriptionId
            };

            V_2021_10_01.SubscriptionReinstated reinstatedEvent = null;

            mockMpOperationService
                .Setup(os => os.GetSubscriptionOperationAsync(testWhNotification.SubscriptionId, testWhNotification.OperationId))
                .Returns(Task.FromResult(testOperation));

            mockMpSubscriptionService
                .Setup(ss => ss.GetSubscriptionAsync(testSubscription.SubscriptionId))
                .Returns(Task.FromResult(testSubscription));

            mockEventPublisher
                .Setup(ep => ep.PublishEventAsync(It.IsAny<V_2021_10_01.SubscriptionReinstated>()))
                .Callback<V_2021_10_01.SubscriptionReinstated>(e => reinstatedEvent = e);

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.ProcessLiveWebhookNotificationAsync(testWhNotification);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkResult>();

            reinstatedEvent.Should().NotBeNull();
            reinstatedEvent.EventType.Should().Be(EventTypes.SubscriptionReinstated);
            reinstatedEvent.OperationId.Should().Be(testWhNotification.OperationId);
            reinstatedEvent.Subscription.Should().BeEquivalentTo(new FlatSubscription(testSubscription));
        }

        [Fact]
        public async Task PostLiveWebhookNotification_GivenUnverifiableNotification_ShouldRespondWith500()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            var testWhNotification = new WebhookNotification
            {
                ActionType = MarketplaceActionTypes.ChangePlan,
                ActivityId = Guid.NewGuid().ToString(),
                OfferId = testSubscription.OfferId,
                OperationId = Guid.NewGuid().ToString(),
                PlanId = testSubscription.PlanId,
                PublisherId = Guid.NewGuid().ToString(),
                SeatQuantity = testSubscription.SeatQuantity,
                SubscriptionId = testSubscription.SubscriptionId
            };

            mockMpSubscriptionService.Setup(ss => ss.GetSubscriptionAsync(testSubscription.SubscriptionId)).Returns(Task.FromResult(testSubscription));

            mockMpOperationService
                .Setup(os => os.GetSubscriptionOperationAsync(testSubscription.SubscriptionId, testWhNotification.OperationId))
                .Returns(Task.FromResult(null as SubscriptionOperation));

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.ProcessLiveWebhookNotificationAsync(testWhNotification);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<StatusCodeResult>();
            (actionResult as StatusCodeResult).StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task PostLiveWebhookNotification_GivenVerifiableSuspensionNotification_ShouldPublishEvent_AndRespondOk()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            var testWhNotification = new WebhookNotification
            {
                ActionType = MarketplaceActionTypes.Suspend,
                ActivityId = Guid.NewGuid().ToString(),
                OfferId = testSubscription.OfferId,
                OperationId = Guid.NewGuid().ToString(),
                PlanId = testSubscription.PlanId,
                PublisherId = Guid.NewGuid().ToString(),
                SeatQuantity = testSubscription.SeatQuantity,
                SubscriptionId = testSubscription.SubscriptionId,
                Timestamp = DateTime.UtcNow
            };

            var testOperation = new SubscriptionOperation
            {
                OperationId = testWhNotification.OperationId,
                OperationType = SubscriptionOperationType.Suspend,
                PlanId = testWhNotification.PlanId,
                SeatQuantity = testWhNotification.SeatQuantity,
                SubscriptionId = testWhNotification.SubscriptionId
            };

            V_2021_10_01.SubscriptionSuspended suspendedEvent = null;

            mockMpOperationService
                .Setup(os => os.GetSubscriptionOperationAsync(testWhNotification.SubscriptionId, testWhNotification.OperationId))
                .Returns(Task.FromResult(testOperation));

            mockMpSubscriptionService
                .Setup(ss => ss.GetSubscriptionAsync(testSubscription.SubscriptionId))
                .Returns(Task.FromResult(testSubscription));

            mockEventPublisher
                .Setup(ep => ep.PublishEventAsync(It.IsAny<V_2021_10_01.SubscriptionSuspended>()))
                .Callback<V_2021_10_01.SubscriptionSuspended>(e => suspendedEvent = e);

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.ProcessLiveWebhookNotificationAsync(testWhNotification);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkResult>();

            suspendedEvent.Should().NotBeNull();
            suspendedEvent.EventType.Should().Be(EventTypes.SubscriptionSuspended);
            suspendedEvent.OperationId.Should().Be(testWhNotification.OperationId);
            suspendedEvent.Subscription.Should().BeEquivalentTo(new FlatSubscription(testSubscription));
        }

        [Fact]
        public async Task PostLiveWebhookNotification_GivenVerifiableCancellationNotification_ShouldPublishEvent_AndRespondOk()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            var testWhNotification = new WebhookNotification
            {
                ActionType = MarketplaceActionTypes.Unsubscribe,
                ActivityId = Guid.NewGuid().ToString(),
                OfferId = testSubscription.OfferId,
                OperationId = Guid.NewGuid().ToString(),
                PlanId = testSubscription.PlanId,
                PublisherId = Guid.NewGuid().ToString(),
                SeatQuantity = testSubscription.SeatQuantity,
                SubscriptionId = testSubscription.SubscriptionId,
                Timestamp = DateTime.UtcNow
            };

            var testOperation = new SubscriptionOperation
            {
                OperationId = testWhNotification.OperationId,
                OperationType = SubscriptionOperationType.Cancel,
                PlanId = testWhNotification.PlanId,
                SeatQuantity = testWhNotification.SeatQuantity,
                SubscriptionId = testWhNotification.SubscriptionId
            };

            V_2021_10_01.SubscriptionCancelled cancelledEvent = null;

            mockMpOperationService
                .Setup(os => os.GetSubscriptionOperationAsync(testWhNotification.SubscriptionId, testWhNotification.OperationId))
                .Returns(Task.FromResult(testOperation));

            mockMpSubscriptionService
                .Setup(ss => ss.GetSubscriptionAsync(testSubscription.SubscriptionId))
                .Returns(Task.FromResult(testSubscription));

            mockEventPublisher
                .Setup(ep => ep.PublishEventAsync(It.IsAny<V_2021_10_01.SubscriptionCancelled>()))
                .Callback<V_2021_10_01.SubscriptionCancelled>(e => cancelledEvent = e);

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.ProcessLiveWebhookNotificationAsync(testWhNotification);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkResult>();

            cancelledEvent.Should().NotBeNull();
            cancelledEvent.EventType.Should().Be(EventTypes.SubscriptionCancelled);
            cancelledEvent.OperationId.Should().Be(testWhNotification.OperationId);
            cancelledEvent.Subscription.Should().BeEquivalentTo(new FlatSubscription(testSubscription));
        }

        [Fact]
        public async Task PostLiveWebhookNotification_GivenVerifiableSeatQuantityChangeNotification_ShouldPublishEvent_AndRespondOk()
        {
            var newSeatQty = 50;

            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            var testWhNotification = new WebhookNotification
            {
                ActionType = MarketplaceActionTypes.ChangeQuantity,
                ActivityId = Guid.NewGuid().ToString(),
                OfferId = testSubscription.OfferId,
                OperationId = Guid.NewGuid().ToString(),
                PlanId = testSubscription.PlanId,
                PublisherId = Guid.NewGuid().ToString(),
                SeatQuantity = newSeatQty,
                SubscriptionId = testSubscription.SubscriptionId,
                Timestamp = DateTime.UtcNow
            };

            var testOperation = new SubscriptionOperation
            {
                OperationId = testWhNotification.OperationId,
                OperationType = SubscriptionOperationType.ChangeSeatQuantity,
                PlanId = testWhNotification.PlanId,
                SeatQuantity = testWhNotification.SeatQuantity,
                SubscriptionId = testWhNotification.SubscriptionId
            };

            V_2021_10_01.SubscriptionSeatQuantityChanged seatQtyChangedEvent = null;

            mockMpOperationService
                .Setup(os => os.GetSubscriptionOperationAsync(testWhNotification.SubscriptionId, testWhNotification.OperationId))
                .Returns(Task.FromResult(testOperation));

            mockMpSubscriptionService
                .Setup(ss => ss.GetSubscriptionAsync(testSubscription.SubscriptionId))
                .Returns(Task.FromResult(testSubscription));

            mockEventPublisher
                .Setup(ep => ep.PublishEventAsync(It.IsAny<V_2021_10_01.SubscriptionSeatQuantityChanged>()))
                .Callback<V_2021_10_01.SubscriptionSeatQuantityChanged>(e => seatQtyChangedEvent = e);

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.ProcessLiveWebhookNotificationAsync(testWhNotification);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkResult>();

            seatQtyChangedEvent.Should().NotBeNull();
            seatQtyChangedEvent.EventType.Should().Be(EventTypes.SubscriptionSeatQuantityChanged);
            seatQtyChangedEvent.NewSeatQuantity.Should().Be(newSeatQty);
            seatQtyChangedEvent.OperationId.Should().Be(testWhNotification.OperationId);
            seatQtyChangedEvent.Subscription.Should().BeEquivalentTo(new FlatSubscription(testSubscription));
        }

        [Fact]
        public async Task PostLiveWebhookNotification_GivenVerifiablePlanChangeNotification_ShouldPublishEvent_AndRespondOk()
        {
            var newPlanId = Guid.NewGuid().ToString();

            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            var testWhNotification = new WebhookNotification
            {
                ActionType = MarketplaceActionTypes.ChangePlan,
                ActivityId = Guid.NewGuid().ToString(),
                OfferId = testSubscription.OfferId,
                OperationId = Guid.NewGuid().ToString(),
                PlanId = newPlanId,
                PublisherId = Guid.NewGuid().ToString(),
                SeatQuantity = testSubscription.SeatQuantity,
                SubscriptionId = testSubscription.SubscriptionId,
                Timestamp = DateTime.UtcNow
            };

            var testOperation = new SubscriptionOperation
            {
                OperationId = testWhNotification.OperationId,
                OperationType = SubscriptionOperationType.ChangePlan,
                PlanId = testWhNotification.PlanId,
                SeatQuantity = testWhNotification.SeatQuantity,
                SubscriptionId = testWhNotification.SubscriptionId
            };

            V_2021_10_01.SubscriptionPlanChanged planChangeEvent = null;

            mockMpOperationService.Setup(os => os.GetSubscriptionOperationAsync(testWhNotification.SubscriptionId, testWhNotification.OperationId)).Returns(Task.FromResult(testOperation));
            mockMpSubscriptionService.Setup(ss => ss.GetSubscriptionAsync(testSubscription.SubscriptionId)).Returns(Task.FromResult(testSubscription));

            mockEventPublisher
                .Setup(ep => ep.PublishEventAsync(It.IsAny<V_2021_10_01.SubscriptionPlanChanged>()))
                .Callback<V_2021_10_01.SubscriptionPlanChanged>(e => planChangeEvent = e);

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.ProcessLiveWebhookNotificationAsync(testWhNotification);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkResult>();

            planChangeEvent.Should().NotBeNull();
            planChangeEvent.EventType.Should().Be(EventTypes.SubscriptionPlanChanged);
            planChangeEvent.NewPlanId.Should().Be(newPlanId);
            planChangeEvent.OperationId.Should().Be(testWhNotification.OperationId);
            planChangeEvent.Subscription.Should().BeEquivalentTo(new FlatSubscription(testSubscription));
        }

        [Fact]
        public async Task PostLiveWebhookNotification_GivenInvalidSubscriptionId_ShouldRespondNotFound()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            var testWhNotification = new WebhookNotification
            {
                ActionType = MarketplaceActionTypes.ChangePlan,
                ActivityId = Guid.NewGuid().ToString(),
                OfferId = testSubscription.OfferId,
                OperationId = Guid.NewGuid().ToString(),
                PlanId = testSubscription.PlanId,
                PublisherId = Guid.NewGuid().ToString(),
                SeatQuantity = testSubscription.SeatQuantity,
                SubscriptionId = testSubscription.SubscriptionId
            };

            mockMpSubscriptionService.Setup(ss => ss.GetSubscriptionAsync(testSubscription.SubscriptionId)).Returns(Task.FromResult(null as Subscription));

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.ProcessLiveWebhookNotificationAsync(testWhNotification);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task PostTestWebhookNotification_GivenTestModeDisabled_ShouldRespondNotFound()
        {
            var deployConfig = GetDefaultDeploymentConfiguration();

            deployConfig.IsTestModeEnabled = false;

            var mockDeployConfig = GetOptionsSnapshotMock(deployConfig);
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var testSubscription = CreateTestSubscription();

            testSubscription.IsTest = true;

            var testWhNotification = new WebhookNotification
            {
                ActionType = MarketplaceActionTypes.Reinstate,
                ActivityId = Guid.NewGuid().ToString(),
                OfferId = testSubscription.OfferId,
                OperationId = Guid.NewGuid().ToString(),
                PlanId = testSubscription.PlanId,
                PublisherId = Guid.NewGuid().ToString(),
                SeatQuantity = testSubscription.SeatQuantity,
                SubscriptionId = testSubscription.SubscriptionId,
                Timestamp = DateTime.UtcNow
            };

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.ProcessTestWebhookNotificationAsync(testWhNotification);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task PostTestWebhookNotification_GivenReinstatementNotification_ShouldPublishEvent_AndRespondOk()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            testSubscription.IsTest = true;

            var testWhNotification = new WebhookNotification
            {
                ActionType = MarketplaceActionTypes.Reinstate,
                ActivityId = Guid.NewGuid().ToString(),
                OfferId = testSubscription.OfferId,
                OperationId = Guid.NewGuid().ToString(),
                PlanId = testSubscription.PlanId,
                PublisherId = Guid.NewGuid().ToString(),
                SeatQuantity = testSubscription.SeatQuantity,
                SubscriptionId = testSubscription.SubscriptionId,
                Timestamp = DateTime.UtcNow
            };

            var testOperation = new SubscriptionOperation
            {
                OperationId = testWhNotification.OperationId,
                OperationType = SubscriptionOperationType.Reinstate,
                PlanId = testWhNotification.PlanId,
                SeatQuantity = testWhNotification.SeatQuantity,
                SubscriptionId = testWhNotification.SubscriptionId
            };

            V_2021_10_01.SubscriptionReinstated reinstatedEvent = null;

            mockSubscriptionRepo
                .Setup(sr => sr.GetSubscriptionAsync(testSubscription.SubscriptionId))
                .Returns(Task.FromResult(testSubscription));

            mockEventPublisher
                .Setup(ep => ep.PublishEventAsync(It.IsAny<V_2021_10_01.SubscriptionReinstated>()))
                .Callback<V_2021_10_01.SubscriptionReinstated>(e => reinstatedEvent = e);

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.ProcessTestWebhookNotificationAsync(testWhNotification);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkResult>();

            reinstatedEvent.Should().NotBeNull();
            reinstatedEvent.EventType.Should().Be(EventTypes.SubscriptionReinstated);
            reinstatedEvent.OperationId.Should().Be(testWhNotification.OperationId);
            reinstatedEvent.Subscription.Should().BeEquivalentTo(new FlatSubscription(testSubscription));
        }

        [Fact]
        public async Task PostTestWebhookNotification_GivenSuspensionNotification_ShouldPublishEvent_AndRespondOk()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            testSubscription.IsTest = true;

            var testWhNotification = new WebhookNotification
            {
                ActionType = MarketplaceActionTypes.Suspend,
                ActivityId = Guid.NewGuid().ToString(),
                OfferId = testSubscription.OfferId,
                OperationId = Guid.NewGuid().ToString(),
                PlanId = testSubscription.PlanId,
                PublisherId = Guid.NewGuid().ToString(),
                SeatQuantity = testSubscription.SeatQuantity,
                SubscriptionId = testSubscription.SubscriptionId,
                Timestamp = DateTime.UtcNow
            };

            var testOperation = new SubscriptionOperation
            {
                OperationId = testWhNotification.OperationId,
                OperationType = SubscriptionOperationType.Suspend,
                PlanId = testWhNotification.PlanId,
                SeatQuantity = testWhNotification.SeatQuantity,
                SubscriptionId = testWhNotification.SubscriptionId
            };

            V_2021_10_01.SubscriptionSuspended suspendedEvent = null;

            mockSubscriptionRepo
                .Setup(sr => sr.GetSubscriptionAsync(testSubscription.SubscriptionId))
                .Returns(Task.FromResult(testSubscription));

            mockEventPublisher
                .Setup(ep => ep.PublishEventAsync(It.IsAny<V_2021_10_01.SubscriptionSuspended>()))
                .Callback<V_2021_10_01.SubscriptionSuspended>(e => suspendedEvent = e);

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.ProcessTestWebhookNotificationAsync(testWhNotification);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkResult>();

            suspendedEvent.Should().NotBeNull();
            suspendedEvent.EventType.Should().Be(EventTypes.SubscriptionSuspended);
            suspendedEvent.OperationId.Should().Be(testWhNotification.OperationId);
            suspendedEvent.Subscription.Should().BeEquivalentTo(new FlatSubscription(testSubscription));
        }

        [Fact]
        public async Task PostTestWebhookNotification_GivenCancellationNotification_ShouldPublishEvent_AndRespondOk()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            testSubscription.IsTest = true;

            var testWhNotification = new WebhookNotification
            {
                ActionType = MarketplaceActionTypes.Unsubscribe,
                ActivityId = Guid.NewGuid().ToString(),
                OfferId = testSubscription.OfferId,
                OperationId = Guid.NewGuid().ToString(),
                PlanId = testSubscription.PlanId,
                PublisherId = Guid.NewGuid().ToString(),
                SeatQuantity = testSubscription.SeatQuantity,
                SubscriptionId = testSubscription.SubscriptionId,
                Timestamp = DateTime.UtcNow
            };

            var testOperation = new SubscriptionOperation
            {
                OperationId = testWhNotification.OperationId,
                OperationType = SubscriptionOperationType.Cancel,
                PlanId = testWhNotification.PlanId,
                SeatQuantity = testWhNotification.SeatQuantity,
                SubscriptionId = testWhNotification.SubscriptionId
            };

            V_2021_10_01.SubscriptionCancelled cancelledEvent = null;

            mockSubscriptionRepo
                .Setup(sr => sr.GetSubscriptionAsync(testSubscription.SubscriptionId))
                .Returns(Task.FromResult(testSubscription));

            mockEventPublisher
                .Setup(ep => ep.PublishEventAsync(It.IsAny<V_2021_10_01.SubscriptionCancelled>()))
                .Callback<V_2021_10_01.SubscriptionCancelled>(e => cancelledEvent = e);

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.ProcessTestWebhookNotificationAsync(testWhNotification);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkResult>();

            cancelledEvent.Should().NotBeNull();
            cancelledEvent.EventType.Should().Be(EventTypes.SubscriptionCancelled);
            cancelledEvent.OperationId.Should().Be(testWhNotification.OperationId);
            cancelledEvent.Subscription.Should().BeEquivalentTo(new FlatSubscription(testSubscription));
        }

        [Fact]
        public async Task PostTestWebhookNotification_GivenSeatQuantityChangeNotification_ShouldPublishEvent_AndRespondOk()
        {
            var newSeatQty = 50;

            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            testSubscription.IsTest = true;

            var testWhNotification = new WebhookNotification
            {
                ActionType = MarketplaceActionTypes.ChangeQuantity,
                ActivityId = Guid.NewGuid().ToString(),
                OfferId = testSubscription.OfferId,
                OperationId = Guid.NewGuid().ToString(),
                PlanId = testSubscription.PlanId,
                PublisherId = Guid.NewGuid().ToString(),
                SeatQuantity = newSeatQty,
                SubscriptionId = testSubscription.SubscriptionId,
                Timestamp = DateTime.UtcNow
            };

            var testOperation = new SubscriptionOperation
            {
                OperationId = testWhNotification.OperationId,
                OperationType = SubscriptionOperationType.ChangeSeatQuantity,
                PlanId = testWhNotification.PlanId,
                SeatQuantity = testWhNotification.SeatQuantity,
                SubscriptionId = testWhNotification.SubscriptionId
            };

            V_2021_10_01.SubscriptionSeatQuantityChanged seatQtyChangedEvent = null;

            mockSubscriptionRepo
                .Setup(sr => sr.GetSubscriptionAsync(testSubscription.SubscriptionId))
                .Returns(Task.FromResult(testSubscription));

            mockEventPublisher
                .Setup(ep => ep.PublishEventAsync(It.IsAny<V_2021_10_01.SubscriptionSeatQuantityChanged>()))
                .Callback<V_2021_10_01.SubscriptionSeatQuantityChanged>(e => seatQtyChangedEvent = e);

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.ProcessTestWebhookNotificationAsync(testWhNotification);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkResult>();

            seatQtyChangedEvent.Should().NotBeNull();
            seatQtyChangedEvent.EventType.Should().Be(EventTypes.SubscriptionSeatQuantityChanged);
            seatQtyChangedEvent.NewSeatQuantity.Should().Be(newSeatQty);
            seatQtyChangedEvent.OperationId.Should().Be(testWhNotification.OperationId);
            seatQtyChangedEvent.Subscription.Should().BeEquivalentTo(new FlatSubscription(testSubscription));
        }

        [Fact]
        public async Task PostTestWebhookNotification_GivenPlanChangeNotification_ShouldPublishEvent_AndRespondOk()
        {
            var newPlanId = Guid.NewGuid().ToString();

            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            testSubscription.IsTest = true;

            var testWhNotification = new WebhookNotification
            {
                ActionType = MarketplaceActionTypes.ChangePlan,
                ActivityId = Guid.NewGuid().ToString(),
                OfferId = testSubscription.OfferId,
                OperationId = Guid.NewGuid().ToString(),
                PlanId = newPlanId,
                PublisherId = Guid.NewGuid().ToString(),
                SeatQuantity = testSubscription.SeatQuantity,
                SubscriptionId = testSubscription.SubscriptionId,
                Timestamp = DateTime.UtcNow
            };

            var testOperation = new SubscriptionOperation
            {
                OperationId = testWhNotification.OperationId,
                OperationType = SubscriptionOperationType.ChangePlan,
                PlanId = testWhNotification.PlanId,
                SeatQuantity = testWhNotification.SeatQuantity,
                SubscriptionId = testWhNotification.SubscriptionId
            };

            V_2021_10_01.SubscriptionPlanChanged planChangeEvent = null;

            mockSubscriptionRepo.Setup(sr => sr.GetSubscriptionAsync(testSubscription.SubscriptionId)).Returns(Task.FromResult(testSubscription));

            mockEventPublisher
                .Setup(ep => ep.PublishEventAsync(It.IsAny<V_2021_10_01.SubscriptionPlanChanged>()))
                .Callback<V_2021_10_01.SubscriptionPlanChanged>(e => planChangeEvent = e);

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.ProcessTestWebhookNotificationAsync(testWhNotification);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkResult>();

            planChangeEvent.Should().NotBeNull();
            planChangeEvent.EventType.Should().Be(EventTypes.SubscriptionPlanChanged);
            planChangeEvent.NewPlanId.Should().Be(newPlanId);
            planChangeEvent.OperationId.Should().Be(testWhNotification.OperationId);
            planChangeEvent.Subscription.Should().BeEquivalentTo(new FlatSubscription(testSubscription));
        }

        [Fact]
        public async Task PostTestWebhookNotification_GivenInvalidSubscriptionId_ShouldRespondNotFound()
        {
            var mockDeployConfig = GetOptionsSnapshotMock(GetDefaultDeploymentConfiguration());
            var mockLogger = new Mock<ILogger<SubscriptionController>>();
            var mockMpOperationService = new Mock<IMarketplaceOperationService>();
            var mockMpSubscriptionService = new Mock<IMarketplaceSubscriptionService>();
            var mockEventPublisher = new Mock<ISubscriptionEventPublisher>();
            var mockSubscriptionStagingCache = new Mock<ISubscriptionStagingCache>();
            var mockSubscriptionRepo = new Mock<ISubscriptionTestingCache>();
            var publisherConfig = GetDefaultPublisherConfiguration();
            var testSubscription = CreateTestSubscription();

            var testWhNotification = new WebhookNotification
            {
                ActionType = MarketplaceActionTypes.ChangePlan,
                ActivityId = Guid.NewGuid().ToString(),
                OfferId = testSubscription.OfferId,
                OperationId = Guid.NewGuid().ToString(),
                PlanId = testSubscription.PlanId,
                PublisherId = Guid.NewGuid().ToString(),
                SeatQuantity = testSubscription.SeatQuantity,
                SubscriptionId = testSubscription.SubscriptionId
            };

            mockSubscriptionRepo.Setup(sr => sr.GetSubscriptionAsync(testSubscription.SubscriptionId)).Returns(Task.FromResult(null as Subscription));

            var controllerUt = new SubscriptionController(
              mockDeployConfig.Object, mockLogger.Object, mockMpOperationService.Object,
              mockMpSubscriptionService.Object, GetDefaultPublisherConfigurationStore(), mockEventPublisher.Object, mockSubscriptionStagingCache.Object, mockSubscriptionRepo.Object);

            var actionResult = await controllerUt.ProcessTestWebhookNotificationAsync(testWhNotification);

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
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
                AppInsightsInstrumentationKey = Guid.NewGuid().ToString(),
                AzureResourceGroupName = "mona-test-rg",
                AzureSubscriptionId = Guid.NewGuid().ToString(),
                EventVersion = EventVersions.V_2021_10_01,
                IsTestModeEnabled = true,
                MonaVersion = "1.0",
                Name = "Mona SaaS Testing",
                SendSubscriptionDetailsToPurchaseConfirmationPage = true,
                SendSubscriptionDetailsToSubscriptionConfigurationPage = true
            };

        private PublisherConfiguration GetDefaultPublisherConfiguration() =>
            new PublisherConfiguration
            {
                IsSetupComplete = true,
                PublisherContactPageUrl = "https://support.microsoft.com/contactus",
                PublisherCopyrightNotice = $"© Microsoft {DateTime.UtcNow.Year}",
                PublisherDisplayName = "Microsoft",
                PublisherHomePageUrl = "https://microsoft.com",
                PublisherPrivacyNoticePageUrl = "https://privacy.microsoft.com/en-us/privacystatement",
                SubscriptionConfigurationUrl = "https://azure.microsoft.com/mona/configure/{subscription-id}",
                SubscriptionPurchaseConfirmationUrl = "https://azure.microsoft.com/mona/purchase/{subscription-id}"
            };

        private Dictionary<string, StringValues> CreateTestSubscriptionQueryParameters() => CreateTestSubscriptionQueryParameters(Guid.NewGuid().ToString());

        private Dictionary<string, StringValues> CreateTestSubscriptionQueryParameters(string subscriptionId) =>
            new Dictionary<string, StringValues>
            {
                [SubscriptionController.TestSubscriptionParameterNames.BeneficiaryAadObjectId] = new StringValues(Guid.NewGuid().ToString()),
                [SubscriptionController.TestSubscriptionParameterNames.BeneficiaryAadTenantId] = new StringValues(Guid.NewGuid().ToString()),
                [SubscriptionController.TestSubscriptionParameterNames.BeneficiaryUserEmail] = new StringValues($"beneficiary-{Guid.NewGuid()}@microsoft.com"),
                [SubscriptionController.TestSubscriptionParameterNames.BeneficiaryUserId] = new StringValues(Guid.NewGuid().ToString()),
                [SubscriptionController.TestSubscriptionParameterNames.IsFreeTrial] = new StringValues("true"),
                [SubscriptionController.TestSubscriptionParameterNames.OfferId] = new StringValues($"Offer {Guid.NewGuid()}"),
                [SubscriptionController.TestSubscriptionParameterNames.PlanId] = new StringValues($"Plan {Guid.NewGuid()}"),
                [SubscriptionController.TestSubscriptionParameterNames.PurchaserAadObjectId] = new StringValues(Guid.NewGuid().ToString()),
                [SubscriptionController.TestSubscriptionParameterNames.PurchaserAadTenantId] = new StringValues(Guid.NewGuid().ToString()),
                [SubscriptionController.TestSubscriptionParameterNames.PurchaserUserEmail] = new StringValues($"purchaser-{Guid.NewGuid()}@microsoft.com"),
                [SubscriptionController.TestSubscriptionParameterNames.PurchaserUserId] = new StringValues(Guid.NewGuid().ToString()),
                [SubscriptionController.TestSubscriptionParameterNames.SeatQuantity] = new StringValues("40"),
                [SubscriptionController.TestSubscriptionParameterNames.SubscriptionId] = new StringValues(subscriptionId),
                [SubscriptionController.TestSubscriptionParameterNames.SubscriptionName] = new StringValues($"Subscription {subscriptionId}"),
                [SubscriptionController.TestSubscriptionParameterNames.TermEndDate] = new StringValues(DateTime.UtcNow.Date.AddMonths(2).ToString("o")),
                [SubscriptionController.TestSubscriptionParameterNames.TermStartDate] = new StringValues(DateTime.UtcNow.Date.ToString("o")),
                [SubscriptionController.TestSubscriptionParameterNames.TermUnit] = new StringValues("PT2M")
            };

        private Subscription CreateTestSubscription() => CreateTestSubscription(Guid.NewGuid().ToString());

        private Subscription CreateTestSubscription(string subscriptionId) =>
            new Subscription
            {
                IsFreeTrial = false,
                IsTest = false,
                OfferId = $"Offer {Guid.NewGuid()}",
                PlanId = $"Plan {Guid.NewGuid()}",
                SeatQuantity = 25,
                Status = SubscriptionStatus.PendingActivation,
                SubscriptionId = subscriptionId,
                SubscriptionName = $"Subscription {subscriptionId}",
                Beneficiary = new MarketplaceUser
                {
                    AadObjectId = Guid.NewGuid().ToString(),
                    AadTenantId = Guid.NewGuid().ToString(),
                    UserEmail = $"beneficiary-{subscriptionId}@microsoft.com",
                    UserId = Guid.NewGuid().ToString()
                },
                Purchaser = new MarketplaceUser
                {
                    AadObjectId = Guid.NewGuid().ToString(),
                    AadTenantId = Guid.NewGuid().ToString(),
                    UserEmail = $"purchaser-{subscriptionId}@microsoft.com",
                    UserId = Guid.NewGuid().ToString()
                },
                Term = new MarketplaceTerm
                {
                    EndDate = DateTime.UtcNow.Date.AddMonths(1),
                    StartDate = DateTime.UtcNow.Date,
                    TermUnit = "PT1M"
                }
            };
    }
}