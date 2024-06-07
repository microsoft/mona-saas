﻿// MICROSOFT CONFIDENTIAL INFORMATION
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

using Microsoft.AspNetCore.Authorization;
using System;

namespace Mona.SaaS.Web.Authorization
{
    public class AdminRoleAuthorizationRequirement : IAuthorizationRequirement
    {
        public AdminRoleAuthorizationRequirement(string adminAadTenantId)
        {
            if (string.IsNullOrEmpty(adminAadTenantId))
            {
                throw new ArgumentNullException(nameof(adminAadTenantId));
            }

            AdminAadTenantId = adminAadTenantId;
        }

        /// <summary>
        /// Gets/sets this deployment's administrative AAD tenant ID.
        /// </summary>
        /// <remarks>
        /// AAD claim is [tid].
        /// </remarks>
        public string AdminAadTenantId { get; set; }
    }
}