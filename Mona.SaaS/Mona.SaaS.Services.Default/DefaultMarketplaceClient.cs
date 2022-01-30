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

using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mona.SaaS.Core.Enumerations;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models;
using Mona.SaaS.Core.Models.Configuration;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Mona.SaaS.Services.Default
{
    public class DefaultMarketplaceClient : IMarketplaceOperationService, IMarketplaceSubscriptionService
    {
        // [Originally] a thin wrapper around https://github.com/Azure/commercial-marketplace-client-dotnet.

        private const int maxRetries = 3; // Max # of retries for exponential backoff retry policy.

        private static readonly HttpClient httpClient;

        private readonly ILogger logger;
        private readonly IdentityConfiguration identityConfig;

        static DefaultMarketplaceClient()
        {
            httpClient = new HttpClient { BaseAddress = new Uri("https://marketplaceapi.microsoft.com") };

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public DefaultMarketplaceClient(
            ILogger<DefaultMarketplaceClient> logger,
            IOptionsSnapshot<IdentityConfiguration> identityConfig)
        {
            this.logger = logger;
            this.identityConfig = identityConfig.Value;
        }

        public async Task<SubscriptionOperation> GetSubscriptionOperationAsync(string subscriptionId, string operationId)
        {
            if (string.IsNullOrEmpty(subscriptionId))
            {
                throw new ArgumentNullException(nameof(subscriptionId));
            }

            if (string.IsNullOrEmpty(operationId))
            {
                throw new ArgumentNullException(nameof(operationId));
            }

            try
            {
                var retryPolicy = CreateAsyncHttpRetryPolicy();
                var relativeUrl = $"api/saas/subscriptions/{subscriptionId}/operations/{operationId}?api-version=2018-08-31";

                var pollyResult = await retryPolicy.ExecuteAndCaptureAsync(async () =>
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl))
                    {
                        var bearerToken = await GetMarketplaceApiBearerToken();

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                        return await httpClient.SendAsync(request);
                    }
                });

                var response = GetResult(pollyResult);

                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var apiOperation = JsonConvert.DeserializeObject<Core.Models.MarketplaceAPI.V_2018_08_31.SubscriptionOperation>(jsonString);
                var coreOperation = ToCoreModel(apiOperation);

                return coreOperation;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    $"An error occurred while trying to get subscription [{subscriptionId}] " +
                    $"operation [{operationId}] from the Marketplace.", ex);

                throw;
            }
        }

        public async Task<Subscription> GetSubscriptionAsync(string subscriptionId)
        {
            if (string.IsNullOrEmpty(subscriptionId))
            {
                throw new ArgumentNullException(nameof(subscriptionId));
            }

            try
            {
                var retryPolicy = CreateAsyncHttpRetryPolicy();
                var relativeUrl = $"api/saas/subscriptions/{subscriptionId}?api-version=2018-08-31";

                var pollyResult = await retryPolicy.ExecuteAndCaptureAsync(async () =>
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl))
                    {
                        var bearerToken = await GetMarketplaceApiBearerToken();

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                        return await httpClient.SendAsync(request);
                    }
                });

                var response = GetResult(pollyResult);

                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var apiSubscription = JsonConvert.DeserializeObject<Core.Models.MarketplaceAPI.V_2018_08_31.Subscription>(jsonString);
                var coreSubscription = ToCoreModel(apiSubscription);

                return coreSubscription;
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while attempting to resolve a subscription.", ex);

                throw;
            }
        }

        public async Task<Subscription> ResolveSubscriptionTokenAsync(string subscriptionToken)
        {
            if (string.IsNullOrEmpty(subscriptionToken))
            {
                throw new ArgumentNullException(nameof(subscriptionToken));
            }

            try
            {
                var retryPolicy = CreateAsyncHttpRetryPolicy();
                var relativeUrl = $"api/saas/subscriptions/resolve?api-version=2018-08-31";

                var pollyResult = await retryPolicy.ExecuteAndCaptureAsync(async () =>
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, relativeUrl))
                    {
                        var bearerToken = await GetMarketplaceApiBearerToken();

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                        request.Headers.Add("x-ms-marketplace-token", subscriptionToken);

                        return await httpClient.SendAsync(request);
                    }
                });

                var response = GetResult(pollyResult);

                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var apiSubscription = JsonConvert.DeserializeObject<Core.Models.MarketplaceAPI.V_2018_08_31.ResolvedSubscription>(jsonString);
                var coreSubscription = ToCoreModel(apiSubscription);

                return coreSubscription;
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while attempting to resolve a subscription.", ex);

                throw;
            }
        }

        private Subscription ToCoreModel(Core.Models.MarketplaceAPI.V_2018_08_31.ResolvedSubscription mpResolvedSubscription) =>
            ToCoreModel(mpResolvedSubscription?.Subscription);

        private Subscription ToCoreModel(Core.Models.MarketplaceAPI.V_2018_08_31.Subscription mpSubscription)
        {
            if (mpSubscription == null)
            {
                return null;
            }
            else
            {
                return new Subscription
                {
                    Beneficiary = ToCoreModel(mpSubscription.Beneficiary),
                    IsFreeTrial = mpSubscription.IsFreeTrial.GetValueOrDefault(),
                    IsTest = mpSubscription.IsTest.GetValueOrDefault(),
                    OfferId = mpSubscription.OfferId,
                    PlanId = mpSubscription.PlanId,
                    Purchaser = ToCoreModel(mpSubscription.Purchaser),
                    SeatQuantity = mpSubscription.Quantity,
                    Status = ToCoreStatus(mpSubscription.Status),
                    SubscriptionId = mpSubscription.Id.ToString(),
                    SubscriptionName = mpSubscription.Name,
                    Term = ToCoreModel(mpSubscription.Term)
                };
            }
        }

        private MarketplaceUser ToCoreModel(Core.Models.MarketplaceAPI.V_2018_08_31.MarketplaceUser mpUser)
        {
            if (mpUser == null)
            {
                return null;
            }
            else
            {
                return new MarketplaceUser
                {
                    AadObjectId = mpUser.ObjectId,
                    AadTenantId = mpUser.TenantId,
                    UserEmail = mpUser.EmailId,
                    UserId = mpUser.Puid
                };
            }
        }

        private MarketplaceTerm ToCoreModel(Core.Models.MarketplaceAPI.V_2018_08_31.MarketplaceTerm mpTerm)
        {
            if (mpTerm == null)
            {
                return null;
            }
            else
            {
                return new MarketplaceTerm
                {
                    EndDate = mpTerm.EndDate,
                    StartDate = mpTerm.StartDate,
                    TermUnit = mpTerm.TermUnit
                };
            }
        }

        private SubscriptionOperation ToCoreModel(Core.Models.MarketplaceAPI.V_2018_08_31.SubscriptionOperation operation)
        {
            if (operation == null)
            {
                return null;
            }
            else
            {
                return new SubscriptionOperation
                {
                    OperationDateTimeUtc = operation.TimeStamp.Value,
                    OperationId = operation.Id.ToString(),
                    OperationType = ToCoreSubscriptionOperationType(operation.Action),
                    PlanId = operation.PlanId,
                    SeatQuantity = operation.Quantity,
                    SubscriptionId = operation.SubscriptionId.ToString()
                };
            }
        }

        private SubscriptionOperationType ToCoreSubscriptionOperationType(string mpActionType) =>
            mpActionType switch
            {
                "ChangePlan" => SubscriptionOperationType.ChangePlan,
                "ChangeQuantity" => SubscriptionOperationType.ChangeSeatQuantity,
                "Reinstate" => SubscriptionOperationType.Reinstate,
                "Renew" => SubscriptionOperationType.Renew,
                "Suspend" => SubscriptionOperationType.Suspend,
                "Unsubscribe" => SubscriptionOperationType.Cancel,

                _ => throw new ArgumentException($"Operation action type [{mpActionType}] is unknown.")
            };

        private SubscriptionStatus ToCoreStatus(string mpStatus) =>
            mpStatus switch
            {
                "PendingFulfillmentStart" => SubscriptionStatus.PendingActivation,
                "Subscribed" => SubscriptionStatus.Active,
                "Suspended" => SubscriptionStatus.Suspended,
                "Unsubscribed" => SubscriptionStatus.Cancelled,

                _ => throw new ArgumentException($"Subscription status [{mpStatus}] is unknown.")
            };

        private T GetResult<T>(PolicyResult<T> policyResult)
        {
            if (policyResult.Outcome == OutcomeType.Successful)
            {
                // If the operation was successful, just return the result...

                return policyResult.Result;
            }
            else
            {
                // If the operation failed but no exception was thrown, throw a generic one...

                if (policyResult.FinalException == null)
                {
                    throw new ApplicationException($"An error occurred while attempting to contact the Marketplace API.");
                }
                else
                {
                    // Otherwise throw the actual one...

                    throw policyResult.FinalException;
                }
            }
        }

        private AsyncRetryPolicy<HttpResponseMessage> CreateAsyncHttpRetryPolicy() => Policy
            .HandleResult<HttpResponseMessage>(r => ShouldRetry(r.StatusCode))
            .WaitAndRetryAsync(maxRetries, a => TimeSpan.FromSeconds(Math.Pow(2, a)));

        private bool ShouldRetry(HttpStatusCode httpStatusCode) =>
            httpStatusCode == HttpStatusCode.TooManyRequests ||   // 429 -- Too many requests. We're being throttled.
            httpStatusCode >= HttpStatusCode.InternalServerError; // 5xx -- Internal server error. Let's try again.

        private async Task<string> GetMarketplaceApiBearerToken()
        {
            var tokenRequestContext = new TokenRequestContext(
                new string[] { "20e940b3-4c77-4b0b-9a53-9e16a1b010a7/.default" });

            var credential = new ClientSecretCredential(
                identityConfig.AppIdentity.AadTenantId,
                identityConfig.AppIdentity.AadClientId,
                identityConfig.AppIdentity.AadClientSecret);

            var tokenResponse = await credential.GetTokenAsync(tokenRequestContext);

            return tokenResponse.Token;
        }
    }
}