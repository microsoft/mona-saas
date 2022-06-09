// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Mona.SaaS.Web.Extensions
{
    public static class ClaimsPrincipalExtension
    {
        public static string GetPreferredClaimValue(this ClaimsPrincipal claimsPrincipal, params string[] claimTypes)
        {
            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            if (claimTypes == null)
            {
                throw new ArgumentNullException(nameof(claimTypes));
            }

            for (var i = 0; i < claimTypes.Length; i++)
            {
                var nameClaim = claimsPrincipal.Claims.FirstOrDefault(c => (c.Type == claimTypes[i]));

                if (nameClaim != null)
                {
                    return nameClaim.Value.ToString();
                }
            }

            return null;
        }
    }
}