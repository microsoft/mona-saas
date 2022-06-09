// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Extensions
{
    public static class IntExtensions
    {
        /// <summary>
        /// Converts the nullable <paramref name="seatQuantity"/> to human-readable text.
        /// </summary>
        /// <param name="seatQuantity">The seat quantity.</param>
        /// <returns>Human-readable text representing <paramref name="seatQuantity"/>.</returns>
        public static string ToSeatQuantityText(this int? seatQuantity) =>
            seatQuantity.HasValue ? seatQuantity.ToString() : "N/A";
    }
}