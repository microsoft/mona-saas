using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mona.SaaS.Core.Enumerations;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Core.Models.Web;
using System;
using System.Threading.Tasks;

namespace Mona.SaaS.Services.Web
{
    public class TestSubscriptionWebService : BaseSubscriptionWebService
    {
        private readonly ISubscriptionTestingCache subscriptionTestingCache;

        public TestSubscriptionWebService(
            IOptionsSnapshot<DeploymentConfiguration> deploymentConfigSnap,
            ILogger<TestSubscriptionWebService> log,
            IPublisherConfigurationStore publisherConfigStore,
            ISubscriptionEventPublisher subscriptionEventPublisher,
            ISubscriptionTestingCache subscriptionTestingCache)
            : base(deploymentConfigSnap, log, publisherConfigStore, subscriptionEventPublisher) =>
            this.subscriptionTestingCache = subscriptionTestingCache;

        public override async Task<IActionResult> OnLanding(HttpContext httpContext, string subToken = null)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
            
            try
            {
                var subscription = CreateTestSubscription(httpContext);

                log.LogInformation($"Test subscription [{subscription.SubscriptionId}] created.");

                await subscriptionTestingCache.PutSubscriptionAsync(subscription);

                return await CompleteSubscriptionPurchaseJourney(subscription);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while trying to handle a test Marketplace landing.");

                throw;
            }
        }

        public override async Task<IActionResult> OnWebhookNotification(HttpContext httpContext, WebhookNotification whNotification)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
            ArgumentNullException.ThrowIfNull(whNotification, nameof(whNotification));

            try
            {
                var subscription = await subscriptionTestingCache.GetSubscriptionAsync(whNotification.SubscriptionId);

                if (subscription == null)
                {
                    // The test webhook endpoint only works for subscriptions created
                    // through the test landing endpoint.

                    log.LogWarning(
                        "Unable to process test Marketplace subscription webhook notification. " +
                        $"Subscription [{whNotification.SubscriptionId}] not found in testing cache.");

                    return new NotFoundResult();
                }
                else
                {
                    var opType = ToCoreOperationType(whNotification.ActionType);

                    log.LogInformation(
                        $"Processing subscription [{subscription.SubscriptionId}] webhook [{opType}] " +
                        $"operation [{whNotification.OperationId}]...");

                    switch (opType)
                    {
                        case SubscriptionOperationType.Activate:
                            subscription.Status = SubscriptionStatus.Active;
                            break;
                        case SubscriptionOperationType.Cancel:
                            subscription.Status = SubscriptionStatus.Cancelled;
                            break;
                        case SubscriptionOperationType.ChangePlan:
                            subscription.PlanId = whNotification.PlanId;
                            break;
                        case SubscriptionOperationType.ChangeSeatQuantity:
                            subscription.SeatQuantity = whNotification.SeatQuantity;
                            break;
                        case SubscriptionOperationType.Reinstate:
                            subscription.Status = SubscriptionStatus.Active;
                            break;
                        case SubscriptionOperationType.Suspend:
                            subscription.Status = SubscriptionStatus.Suspended;
                            break;
                    }

                    await PublishWebhookSubscriptionEvent(opType, subscription, whNotification);
                    await subscriptionTestingCache.PutSubscriptionAsync(subscription);

                    log.LogInformation(
                        $"Subscription [{subscription.SubscriptionId}] webhook [{opType}] " +
                        $"operation [{whNotification.OperationId}] processed successfully.");

                    return new OkResult();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while trying to process a test Marketplace webhook notification.");

                throw;
            }
        }

        private Subscription CreateTestSubscription(HttpContext httpContext) => new Subscription
        {
            SubscriptionId = TryGetQueryStringParameter(httpContext, TestSubscriptionParameterNames.SubscriptionId, Guid.NewGuid().ToString()),
            SubscriptionName = TryGetQueryStringParameter(httpContext, TestSubscriptionParameterNames.SubscriptionName, "Test Subscription"),
            OfferId = TryGetQueryStringParameter(httpContext,TestSubscriptionParameterNames.OfferId, "Test Offer"),
            PlanId = TryGetQueryStringParameter(httpContext,TestSubscriptionParameterNames.PlanId, "Test Plan"),
            IsTest = true,
            IsFreeTrial = TryParseBooleanQueryStringParameter(httpContext,TestSubscriptionParameterNames.IsFreeTrial, false).Value,
            SeatQuantity = TryParseIntQueryStringParameter(httpContext, TestSubscriptionParameterNames.SeatQuantity),
            Term = CreateTestMarketplaceTerm(httpContext),
            Beneficiary = CreateTestMarketplaceBeneficiary(httpContext, "beneficiary@microsoft.com"),
            Purchaser = CreateTestMarketplacePurchaser(httpContext, "purchaser@microsoft.com"),
            Status = SubscriptionStatus.PendingActivation
        };

        private MarketplaceTerm CreateTestMarketplaceTerm(HttpContext httpContext) => new MarketplaceTerm
        {
            EndDate = TryParseDateTimeQueryStringParameter(httpContext, TestSubscriptionParameterNames.TermEndDate, DateTime.UtcNow.Date.AddMonths(1)),
            StartDate = TryParseDateTimeQueryStringParameter(httpContext, TestSubscriptionParameterNames.TermStartDate, DateTime.UtcNow.Date),
            TermUnit = TryGetQueryStringParameter(httpContext, TestSubscriptionParameterNames.TermUnit, "PT1M")
        };

        private MarketplaceUser CreateTestMarketplaceBeneficiary(HttpContext httpContext, string defaultUserEmail) => new MarketplaceUser
        {
            AadObjectId = TryGetQueryStringParameter(httpContext, TestSubscriptionParameterNames.BeneficiaryAadObjectId, Guid.NewGuid().ToString()),
            AadTenantId = TryGetQueryStringParameter(httpContext, TestSubscriptionParameterNames.BeneficiaryAadTenantId, Guid.NewGuid().ToString()),
            UserEmail = TryGetQueryStringParameter(httpContext, TestSubscriptionParameterNames.BeneficiaryUserEmail, defaultUserEmail),
            UserId = TryGetQueryStringParameter(httpContext, TestSubscriptionParameterNames.BeneficiaryUserId, Guid.NewGuid().ToString())
        };

        private MarketplaceUser CreateTestMarketplacePurchaser(HttpContext httpContext, string defaultUserEmail) => new MarketplaceUser
        {
            AadObjectId = TryGetQueryStringParameter(httpContext, TestSubscriptionParameterNames.PurchaserAadObjectId, Guid.NewGuid().ToString()),
            AadTenantId = TryGetQueryStringParameter(httpContext, TestSubscriptionParameterNames.PurchaserAadTenantId, Guid.NewGuid().ToString()),
            UserEmail = TryGetQueryStringParameter(httpContext, TestSubscriptionParameterNames.PurchaserUserEmail, defaultUserEmail),
            UserId = TryGetQueryStringParameter(httpContext, TestSubscriptionParameterNames.PurchaserUserId, Guid.NewGuid().ToString())
        };

        private string TryGetQueryStringParameter(HttpContext httpContext, string key, string defaultValue = null) =>
            httpContext.Request.Query.TryGetValue(key, out var value) ? value.ToString() : defaultValue;

        private bool? TryParseBooleanQueryStringParameter(HttpContext httpContext, string key, bool? defaultValue = null) =>
            httpContext.Request.Query.TryGetValue(key, out var value) ? bool.Parse(value.ToString()) : defaultValue;

        private DateTime? TryParseDateTimeQueryStringParameter(HttpContext httpContext, string key, DateTime? defaultValue = null) =>
            httpContext.Request.Query.TryGetValue(key, out var value) ? DateTime.Parse(value.ToString()) : defaultValue;

        private int? TryParseIntQueryStringParameter(HttpContext httpContext, string key, int? defaultValue = null) =>
            httpContext.Request.Query.TryGetValue(key, out var value) ? int.Parse(value.ToString()) : defaultValue;

        public static class TestSubscriptionParameterNames
        {
            public const string SubscriptionId = "subscriptionId";
            public const string SubscriptionName = "subscriptionName";
            public const string OfferId = "offerId";
            public const string PlanId = "planId";
            public const string IsFreeTrial = "isFreeTrial";
            public const string SeatQuantity = "seatQuantity";
            public const string TermStartDate = "term_startDate";
            public const string TermEndDate = "term_endDate";
            public const string TermUnit = "term_termUnit";
            public const string BeneficiaryAadObjectId = "beneficiary_aadObjectId";
            public const string BeneficiaryAadTenantId = "beneficiary_aadTenantId";
            public const string BeneficiaryUserEmail = "beneficiary_userEmail";
            public const string BeneficiaryUserId = "beneficiary_userId";
            public const string PurchaserAadObjectId = "purchaser_aadObjectId";
            public const string PurchaserAadTenantId = "purchaser_aadTenantId";
            public const string PurchaserUserEmail = "purchaser_userEmail";
            public const string PurchaserUserId = "purchaser_userId";
        }
    }
}
