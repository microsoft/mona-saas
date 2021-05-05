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