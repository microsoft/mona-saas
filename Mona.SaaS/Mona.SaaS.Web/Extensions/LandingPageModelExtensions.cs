// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Mona.SaaS.Core;
using Mona.SaaS.Core.Models;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Web.Models;
using System;
using System.Linq;
using System.Security.Claims;

namespace Mona.SaaS.Web.Extensions
{
    public static class LandingPageModelExtensions
    {
        /// <summary>
        /// Applies claims-based user information to the provided landing page model.
        /// </summary>
        /// <param name="landingPageModel">The landing page model.</param>
        /// <param name="claimsPrincipal">The claims-based user information.</param>
        /// <returns>The updated landing page model.</returns>
        public static LandingPageModel WithCurrentUserInformation(this LandingPageModel landingPageModel, ClaimsPrincipal claimsPrincipal)
        {
            if (landingPageModel == null)
            {
                throw new ArgumentNullException(nameof(landingPageModel));
            }

            if (claimsPrincipal != null)
            {
                landingPageModel.UserFriendlyName = claimsPrincipal.GetPreferredClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "name");
            }

            return landingPageModel;
        }

        /// <summary>
        /// Applies general publisher information to the provided landing page model.
        /// </summary>
        /// <param name="landingPageModel">The landing page model.</param>
        /// <param name="publisherInfo">The general publisher information.</param>
        /// <returns>The updated landing page model.</returns>
        public static LandingPageModel WithPublisherInformation(this LandingPageModel landingPageModel, PublisherConfiguration publisherInfo)
        {
            if (landingPageModel == null)
            {
                throw new ArgumentNullException(nameof(landingPageModel));
            }

            if (publisherInfo == null)
            {
                throw new ArgumentNullException(nameof(publisherInfo));
            }

            landingPageModel.PublisherContactPageUrl = publisherInfo.PublisherContactPageUrl;
            landingPageModel.PublisherCopyrightNotice = publisherInfo.PublisherCopyrightNotice;
            landingPageModel.PublisherDisplayName = publisherInfo.PublisherDisplayName;
            landingPageModel.PublisherHomePageUrl = publisherInfo.PublisherHomePageUrl;
            landingPageModel.PublisherPrivacyNoticePageUrl = publisherInfo.PublisherPrivacyNoticePageUrl;

            return landingPageModel;
        }

        /// <summary>
        /// Applies subscription information to the provided landing page model.
        /// </summary>
        /// <param name="landingPageModel">The landing page model.</param>
        /// <param name="subscription">The subscription information.</param>
        /// <returns>The updated landing page model.</returns>
        public static LandingPageModel WithSubscriptionInformation(this LandingPageModel landingPageModel, Subscription subscription)
        {
            if (landingPageModel == null)
            {
                throw new ArgumentNullException(nameof(landingPageModel));
            }

            if (subscription == null)
            {
                throw new ArgumentNullException(nameof(subscription));
            }

            landingPageModel.BeneficiaryEmailAddress = subscription.Beneficiary.UserEmail;
            landingPageModel.PurchaserEmailAddress = subscription.Purchaser.UserEmail;
            landingPageModel.IsFreeTrial = subscription.IsFreeTrial;
            landingPageModel.OfferId = subscription.OfferId;
            landingPageModel.PlanId = subscription.PlanId;
            landingPageModel.SeatQuantity = subscription.SeatQuantity;
            landingPageModel.SubscriptionId = subscription.SubscriptionId;
            landingPageModel.SubscriptionName = subscription.SubscriptionName;

            return landingPageModel;
        }

        /// <summary>
        /// Applies an error code to the provided landing page model.
        /// </summary>
        /// <param name="landingPageModel">The landing page model.</param>
        /// <param name="errorCode">The error code.</param>
        /// <returns>The updated landing page model.</returns>
        public static LandingPageModel WithErrorCode(this LandingPageModel landingPageModel, string errorCode)
        {
            if (landingPageModel == null)
            {
                throw new ArgumentNullException(nameof(landingPageModel));
            }

            if (string.IsNullOrEmpty(errorCode))
            {
                throw new ArgumentNullException(nameof(errorCode));
            }

            landingPageModel.ErrorCode = errorCode;

            return landingPageModel;
        }
    }
}