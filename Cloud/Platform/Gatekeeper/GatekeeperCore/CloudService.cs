// -
// <copyright file="CloudService.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

namespace HomeOS.Cloud.Platform.Gatekeeper
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using HomeOS.Shared;
    using HomeOS.Shared.Gatekeeper;

    /// <summary>
    /// Represents the HomeOS.Cloud.Platform.Gatekeeper.CloudServiceCore cloud service.
    /// </summary>
    public class CloudService
    {
        /// <summary>
        /// Time this service instance was started.
        /// </summary>
        private DateTime startTime;

        /// <summary>
        /// Authentication module for this service.
        /// </summary>
        private Keymaster keymaster;

        /// <summary>
        /// Collection of services and clients available for forwarding.
        /// </summary>
        private Matchmaker matchmaker;

        /// <summary>
        /// Agent listening for incoming service connection requests.
        /// </summary>
        private ConnectionListener serviceListener;

        /// <summary>
        /// Agent listening for incoming client requests.
        /// </summary>
        private ConnectionListener clientListener;

        /// <summary>
        /// Initializes a new instance of the CloudService class.
        /// </summary>
        public CloudService()
        {
            this.startTime = DateTime.UtcNow;
            this.keymaster = new Keymaster();
            this.matchmaker = new Matchmaker();
            this.serviceListener = new ConnectionListener(
                Settings.ServicePort,
                this.ServiceConnectionAccepted);
            this.clientListener = new ConnectionListener(
                Settings.ClientPort,
                this.ClientConnectionAccepted);
        }

        public static void Main()
        {
            var cloudService = new CloudService();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Gets the time this service instance was started.
        /// </summary>
        public DateTime StartTime
        {
            get { return this.startTime; }
        }

        /// <summary>
        /// Gets the matchmaker instance the service is using.
        /// </summary>
        public Matchmaker Matchmaker
        {
            get { return this.matchmaker; }
        }

        /// <summary>
        /// Handles successfully accepted service connections.
        /// </summary>
        /// <param name="accepted">
        /// The socket for the newly accepted connection.
        /// </param>
        private void ServiceConnectionAccepted(Socket accepted)
        {
            ServiceConnection connection = new ServiceConnection(
                accepted,
                this.keymaster,
                this.matchmaker);
        }

        /// <summary>
        /// Handles successfully accepted client connections.
        /// </summary>
        /// <param name="accepted">
        /// The socket for the newly accepted connection.
        /// </param>
        private void ClientConnectionAccepted(Socket accepted)
        {
            ClientConnection connection = new ClientConnection(
                accepted,
                this.matchmaker);
        }
    }
}
