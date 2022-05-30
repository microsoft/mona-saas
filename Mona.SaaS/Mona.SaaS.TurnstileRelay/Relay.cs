// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Mona.SaaS.Core.Constants;
using Mona.SaaS.Core.Models.Events.V_2021_05_01;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static System.Environment;

namespace Mona.SaaS.TurnstileRelay
{
    public static class Relay
    {
        private const int maxRetries = 3; // Max # of retries for exponential backoff retry policy.

        private static readonly HttpClient httpClient;

        static Relay()
        {
            const string apiAccessKeyVariable = "Turnstile_ApiAccessKey";
            const string apiBaseUrlVariable = "Turnstile_ApiBaseUrl";

            httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri(
                GetEnvironmentVariable(apiBaseUrlVariable) ??
                throw new InvalidOperationException($"[{apiBaseUrlVariable}] not configured."));

            httpClient.DefaultRequestHeaders.Clear();

            httpClient.DefaultRequestHeaders.Add("x-functions-key",
                GetEnvironmentVariable(apiAccessKeyVariable) ??
                throw new InvalidOperationException($"[{apiAccessKeyVariable}] not configured."));

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mona to Turnstile Relay");
        }

        [FunctionName("Relay")]
        public static async Task Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            try
            {
                log.LogInformation($"Processing [{eventGridEvent.EventType}] event [{eventGridEvent.Id}]...");

                switch (eventGridEvent.EventType)
                {
                    case EventTypes.SubscriptionCanceled:
                        await OnSubscriptionCanceled(eventGridEvent, log);
                        return;
                    case EventTypes.SubscriptionPlanChanged:
                        await OnSubscriptionPlanChanged(eventGridEvent, log);
                        return;
                    case EventTypes.SubscriptionReinstated:
                        await OnSubscriptionReinstated(eventGridEvent, log);
                        return;
                    case EventTypes.SubscriptionSeatQuantityChanged:
                        await OnSubscriptionSeatQtyChanged(eventGridEvent, log);
                        return;
                    case EventTypes.SubscriptionSuspended:
                        await OnSubscriptionSuspended(eventGridEvent, log);
                        return;
                    default:
                        log.LogWarning($"Unable to process [{eventGridEvent.EventType}] event.");
                        return;
                }
            }
            catch (Exception ex)
            {
                log.LogError(
                    $"An error occurred while trying to process [{eventGridEvent.EventType}] event " +
                    $"[{eventGridEvent.Id}]: [{ex.Message}].");

                throw;
            }
        }

        private static async Task OnSubscriptionCanceled(EventGridEvent eventGridEvent, ILogger log)
        {
            var canceled = JsonConvert.DeserializeObject<SubscriptionCanceled>(
                eventGridEvent.Data.ToString());

            await OnSubscriptionStateChanged(canceled, "canceled");

            log.LogInformation($"Subscription [{canceled.SubscriptionId}] canceled.");
        }

        private static async Task OnSubscriptionReinstated(EventGridEvent eventGridEvent, ILogger log)
        {
            var reinstated = JsonConvert.DeserializeObject<SubscriptionReinstated>(
                eventGridEvent.Data.ToString());

            await OnSubscriptionStateChanged(reinstated, "active");

            log.LogInformation($"Subscription [{reinstated.SubscriptionId}] reinstated.");
        }

        private static async Task OnSubscriptionSuspended(EventGridEvent eventGridEvent, ILogger log)
        {
            var suspended = JsonConvert.DeserializeObject<SubscriptionSuspended>(
                eventGridEvent.Data.ToString());

            await OnSubscriptionStateChanged(suspended, "suspended");

            log.LogInformation($"Subscription [{suspended.SubscriptionId}] suspended.");
        }

        private static async Task OnSubscriptionPlanChanged(EventGridEvent eventGridEvent, ILogger log)
        {
            var planChanged = JsonConvert.DeserializeObject<SubscriptionPlanChanged>(
                eventGridEvent.Data.ToString());

            var retryPolicy = CreateAsyncHttpRetryPolicy();

            var pollyResult = await retryPolicy.ExecuteAndCaptureAsync(async () =>
                await httpClient.PatchAsync(
                    GetSubscriptionPatchUrl(planChanged),
                    JsonContent.Create(new { plan_id = planChanged.NewPlanId })));

            var response = GetResult(pollyResult);

            response.EnsureSuccessStatusCode();

            log.LogInformation($"Subscription [{planChanged.SubscriptionId}] plan changed to [{planChanged.NewPlanId}].");
        }

        private static async Task OnSubscriptionSeatQtyChanged(EventGridEvent eventGridEvent, ILogger log)
        {
            var seatQtyChanged = JsonConvert.DeserializeObject<SubscriptionSeatQuantityChanged>(
                eventGridEvent.Data.ToString());

            var retryPolicy = CreateAsyncHttpRetryPolicy();

            var pollyResult = await retryPolicy.ExecuteAndCaptureAsync(async () =>
                await httpClient.PatchAsync(
                    GetSubscriptionPatchUrl(seatQtyChanged),
                    JsonContent.Create(new { total_seats = seatQtyChanged.NewSeatQuantity })));

            var response = GetResult(pollyResult);

            response.EnsureSuccessStatusCode();

            log.LogInformation(
                $"Subscription [{seatQtyChanged.SubscriptionId}] seat quantity " +
                $"changed to [{seatQtyChanged.NewSeatQuantity}].");
        }

        private static async Task OnSubscriptionStateChanged(BaseSubscriptionEvent subEvent, string state)
        {
            var retryPolicy = CreateAsyncHttpRetryPolicy();

            var pollyResult = await retryPolicy.ExecuteAndCaptureAsync(async () =>
                await httpClient.PatchAsync(GetSubscriptionPatchUrl(subEvent), JsonContent.Create(new { state })));

            var response = GetResult(pollyResult);

            response.EnsureSuccessStatusCode();
        }

        private static T GetResult<T>(PolicyResult<T> policyResult)
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

        private static string GetSubscriptionPatchUrl(BaseSubscriptionEvent subEvent) =>
            $"api/saas/subscriptions/{subEvent.SubscriptionId}";

        private static AsyncRetryPolicy<HttpResponseMessage> CreateAsyncHttpRetryPolicy() => Policy
           .HandleResult<HttpResponseMessage>(r => ShouldRetry(r.StatusCode))
           .WaitAndRetryAsync(maxRetries, a => TimeSpan.FromSeconds(Math.Pow(2, a)));

        private static bool ShouldRetry(HttpStatusCode httpStatusCode) =>
            httpStatusCode == HttpStatusCode.TooManyRequests ||   // 429 -- Too many requests. We're being throttled.
            httpStatusCode >= HttpStatusCode.InternalServerError; // 5xx -- Internal server error. Let's try again.
    }
}
