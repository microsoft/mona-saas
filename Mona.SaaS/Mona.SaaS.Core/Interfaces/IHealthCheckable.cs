// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Interfaces
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a common interface for checking the health of a service.
    /// </summary>
    public interface IHealthCheckable
    {
        Task<bool> IsHealthyAsync();
    }
}
