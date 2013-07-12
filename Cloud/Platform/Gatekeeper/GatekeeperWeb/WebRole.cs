// -
// <copyright file="WebRole.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

namespace HomeOS.Cloud.Platform.Gatekeeper.GatekeeperWeb
{
    using System;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;

    /// <summary>
    /// Representation of this web role in Azure.
    /// </summary>
    /// <remarks>
    /// Note that when run in Azure's "Full IIS" mode, this code runs
    /// in a different process/AppDomain than the website(s)!  So any
    /// static variables set here won't be visible to the website code.
    /// </remarks>
    public class WebRole : RoleEntryPoint
    {
        /// <summary>
        /// Called upon service start.
        /// </summary>
        /// <returns>
        /// True if service started successfully, false otherwise.
        /// </returns>
        public override bool OnStart()
        {
            return base.OnStart();
        }
    }
}
