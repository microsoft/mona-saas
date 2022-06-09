// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Mona.SaaS.Core.Constants
{
    /// <summary>
    /// Common Mona subscription event versions (with notes).
    /// </summary>
    public static class EventVersions
    {
        // TODO: Bump this when we release new event versions.
        public const string CurrentEventVersion = V_2021_10_01;

        /// <remarks>
        /// Hierarchical subscription model; 
        /// camel-cased JSON property names; 
        /// </remarks>
        public const string V_2021_05_01 = "2021-05-01";

        /// <summary>
        /// Flattened subscription model (for simpler Logic Apps consumption); 
        /// human-readable, pascal-cased JSON property names;
        /// </summary>
        public const string V_2021_10_01 = "2021-10-01";
    }
}
