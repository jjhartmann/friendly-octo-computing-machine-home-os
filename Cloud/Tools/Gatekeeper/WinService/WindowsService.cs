// -
// <copyright file="WindowsService.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

namespace HomeOS.Cloud.Platform.Gatekeeper
{
    using System;
    using System.ServiceProcess;

    /// <summary>
    /// Represents the gatekeeper cloud service when run as a Windows service.
    /// </summary>
    public partial class WindowsService : ServiceBase
    {
        /// <summary>
        /// The container for all the service-specific logic.
        /// </summary>
        private CloudService cloudService;

        /// <summary>
        /// Initializes a new instance of the WindowsService class.
        /// </summary>
        public WindowsService()
        {
            this.InitializeComponent();
        }

#if DEBUG
        /// <summary>
        /// Hander for the fake Start command our "Program" code issues when
        /// compiled with DEBUG flag.  Basically, this is to get around the
        /// "protected" access on the real OnStart.
        /// </summary>
        /// <param name="args">Arguments for service Start command.</param>
        public void DebugStart(string[] args)
        {
            this.OnStart(args);
        }
#endif

        /// <summary>
        /// Handler for the Start command.
        /// </summary>
        /// <param name="args">Arguments for service Start command.</param>
        protected override void OnStart(string[] args)
        {
            this.cloudService = new CloudService();
        }

        /// <summary>
        /// Handler for the Stop command.
        /// </summary>
        protected override void OnStop()
        {
            this.cloudService = null;
        }
    }
}