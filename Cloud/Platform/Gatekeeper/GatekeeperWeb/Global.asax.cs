// -
// <copyright file="Global.asax.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

namespace HomeOS.Cloud.Platform.Gatekeeper.GatekeeperWeb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Security;
    using System.Web.SessionState;
    using HomeOS.Cloud.Platform.Gatekeeper;

    /// <summary>
    /// Holds the global state of this web application.
    /// </summary>
    public class Global : System.Web.HttpApplication
    {
        /// <summary>
        /// The container for all the service-specific logic.
        /// </summary>
        private static CloudService cloudService;

        /// <summary>
        /// Gets the current CloudService instance.
        /// </summary>
        public static CloudService CloudService
        {
            get { return Global.cloudService; }
        }

        /// <summary>
        /// Handler for the Application Start event.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="ea">The parameter is not used.</param>
        public void Application_Start(object sender, EventArgs ea)
        {
            // -
            // Start the cloud service.
            // -
            Global.cloudService = new CloudService();
        }

        /// <summary>
        /// Handler for the Application End event.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="ea">The parameter is not used.</param>
        public void Application_End(object sender, EventArgs ea)
        {
            // Code that runs on application shutdown.
        }

        /// <summary>
        /// Handler for the Application Error event.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="ea">The parameter is not used.</param>
        public void Application_Error(object sender, EventArgs ea)
        {
            // Code that runs when an unhandled error occurs.
        }

        /// <summary>
        /// Handler for the Session Start event.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="ea">The parameter is not used.</param>
        public void Session_Start(object sender, EventArgs ea)
        {
            // Code that runs when a new session is started.
        }

        /// <summary>
        /// Handler for the Session End event.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="ea">The parameter is not used.</param>
        public void Session_End(object sender, EventArgs ea)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.
        }
    }
}
