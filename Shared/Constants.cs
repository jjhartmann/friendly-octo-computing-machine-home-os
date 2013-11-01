// -
// <copyright file="Constants.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

namespace HomeOS.Shared
{
    using System;
    using System.Net;

    /// <summary>
    /// Defines various constants used in the HomeOS.Cloud.Platform.Gatekeeper services.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Valid values for the HTTP Method field (part of the Request-Line).
        /// </summary>
        public static readonly string[] ValidHttpMethods = new string[]
        {
            "POST", "HEAD", "GET", "PUT", "DELETE", "TRACE", "OPTIONS"
        };

        /// <summary>
        /// Heartbeat service end point port
        /// </summary>
        public static readonly UInt32 HeartbeatServicePort = 5003;

        /// <summary>
        /// Heartbeat service end point port
        /// </summary>
        public static readonly UInt32 HeartbeatServiceSecurePort = 5443;

        /// <summary>
        /// Heartbeat service listener WCf service end point Url suffix
        /// </summary>
        public static string HeartbeatServiceWcfListenerEndPointUrlSuffix = "HeartbeatListenerService.svc";

        /// <summary>
        /// Heartbeat service monitor WCf service end point Url suffix
        /// </summary>
        public static string HeartbeatServiceWcftMonitorEndPointUrlSuffix = "HeartbeatMonitorService.svc";

        /// <summary>
        /// This is the minimum value that can be set for the heart beat interval
        /// </summary>
        public static readonly UInt32 MinHeartbeatIntervalInMins = 1;

        /// <summary>
        /// This is the maximum value that can be set for the heart beat interval
        /// </summary>
        public static readonly UInt32 MaxHeartbeatIntervalInMins = 15;

        /// <summary>
        /// Email service end point Https port
        /// </summary>
        public static readonly UInt32 EmailServiceSecurePort = 7443;

        /// <summary>
        /// Email service end point Url suffix
        /// </summary>
        public static string EmailServiceWcfEndPointUrlSuffix = "EmailService.svc";

    }
}
