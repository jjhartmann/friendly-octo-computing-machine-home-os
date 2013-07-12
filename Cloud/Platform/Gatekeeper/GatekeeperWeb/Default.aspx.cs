// -
// <copyright file="Default.aspx.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

namespace HomeOS.Cloud.Platform.Gatekeeper.GatekeeperWeb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using HomeOS.Cloud.Platform.Gatekeeper;

    /// <summary>
    /// Representation of the default page for this web site.
    /// </summary>
    public partial class _Default : System.Web.UI.Page
    {
        /// <summary>
        /// Handles the page load event.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="ea">The parameter is not used.</param>
        protected void Page_Load(object sender, EventArgs ea)
        {
            // -
            // Set properties of our response.
            // -
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            this.Response.Cache.SetNoStore();
            this.Response.Cache.SetExpires(DateTime.MinValue);

            // -
            // Fill in the current server time.
            // -
            this.CurrentTimeLabel.Text = DateTime.UtcNow.ToString();

            CloudService cloudService = Global.CloudService;
            if (cloudService == null)
            {
                this.ServiceStatusLabel.Text = "Service is not running.";
                return;
            }

            this.ServiceStatusLabel.Text = "Service running.";

            this.ServiceStats.Visible = true;
            this.ServiceStartTimeLabel.Text = cloudService.StartTime.ToString();
            this.RegisteredServicesCountLabel.Text = cloudService.Matchmaker.RegisteredServiceCount.ToString();
            this.ActiveForwardingCount.Text = cloudService.Matchmaker.ActiveMatchCount.ToString();
        }
    }
}
