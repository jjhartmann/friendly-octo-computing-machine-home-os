// -
// <copyright file="Keymaster.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

namespace HomeOS.Cloud.Platform.Gatekeeper
{
    using System;
    using System.Collections.Generic;
    using HomeOS.Shared;
    using HomeOS.Shared.Gatekeeper;

    /// <summary>
    /// Represents the authentication manager for the service.
    /// </summary>
    public class Keymaster
    {
        /// <summary>
        /// Verifies the simple authentication information for a client.
        /// </summary>
        /// <param name="id">An identifier for the client.</param>
        /// <param name="password">The client's password.</param>
        /// <returns>
        /// True if the id and password are valid, false otherwise.
        /// </returns>
        public bool VerifySimpleAuthentication(string id, uint password)
        {
            // -
            // ToDo: Make this a real check.
            // -
            if (password == Settings.HomePassword)
            {
                return true;
            }

            return false;
        }
    }
}
