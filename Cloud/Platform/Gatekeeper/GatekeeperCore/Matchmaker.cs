// -
// <copyright file="Matchmaker.cs" company="Microsoft Corporation">
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
    /// Represents an agent for matching clients to available services.
    /// </summary>
    public class Matchmaker
    {
        /// <summary>
        /// A random number generator.
        /// </summary>
        private static Random randomNumberGenerator;

        /// <summary>
        /// Collection of services registered.
        /// </summary>
        private Dictionary<string, ServiceConnection> registeredServices;

        /// <summary>
        /// Collection of clients awaiting service forwarding.
        /// </summary>
        private Dictionary<uint, ClientConnection> waitingClients;

        /// <summary>
        /// Collection of matches undergoing forwarding.
        /// </summary>
        private HashSet<Forwarder> matches;

        /// <summary>
        /// Initializes a new instance of the Matchmaker class.
        /// </summary>
        public Matchmaker()
        {
            Matchmaker.randomNumberGenerator = new Random();
            this.registeredServices =
                new Dictionary<string, ServiceConnection>();
            this.waitingClients = new Dictionary<uint, ClientConnection>();
            this.matches = new HashSet<Forwarder>();
        }

        /// <summary>
        /// Gets a list of the currently registered services.
        /// </summary>
        public string[] RegisteredServices
        {
            get
            {
                string[] servicesList;
                lock (this.registeredServices)
                {
                    servicesList =
                        new string[this.registeredServices.Keys.Count];
                    this.registeredServices.Keys.CopyTo(servicesList, 0);
                }

                return servicesList;
            }
        }

        /// <summary>
        /// Gets the number of currently registered services.
        /// </summary>
        public int RegisteredServiceCount
        {
            get { return this.registeredServices.Count; }
        }

        /// <summary>
        /// Gets the number of currently active matches.
        /// </summary>
        public int ActiveMatchCount
        {
            get { return this.matches.Count; }
        }

        /// <summary>
        /// Registers a service with the matchmaker.
        /// </summary>
        /// <param name="id">An identifier for the service.</param>
        /// <param name="connection">
        /// The control connection to this service.
        /// </param>
        /// <returns>True if successfully registered, false otherwise.</returns>
        public bool RegisterService(string id, ServiceConnection connection)
        {
            try
            {
                lock (this.registeredServices)
                {
                    this.registeredServices.Add(id, connection);
                }
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Removes a service registration.
        /// </summary>
        /// <param name="id">An identifier for the service.</param>
        /// <returns>
        /// True if successfully found and removed, false otherwise.
        /// </returns>
        public bool RemoveServiceRegistration(string id)
        {
            lock (this.registeredServices)
            {
                return this.registeredServices.Remove(id);
            }
        }

        /// <summary>
        /// Looks up a service with the matchmaker.
        /// </summary>
        /// <param name="id">The service's identifier.</param>
        /// <returns>The control connection for the specified service.</returns>
        public ServiceConnection LookupService(string id)
        {
            ServiceConnection found;
            lock (this.registeredServices)
            {
                this.registeredServices.TryGetValue(id, out found);
            }

            return found;
        }

        /// <summary>
        /// Registers a client connection with the matchmaker.
        /// </summary>
        /// <param name="client">The client's connection.</param>
        /// <returns>
        /// An unique identifier token for this client instance.
        /// </returns>
        public uint RegisterClient(ClientConnection client)
        {
            uint token;
            while (true)
            {
                token = (uint)Matchmaker.randomNumberGenerator.Next(
                    int.MinValue,
                    int.MaxValue);
                try
                {
                    lock (this.waitingClients)
                    {
                        this.waitingClients.Add(token, client);
                    }

                    break;
                }
                catch (ArgumentException)
                {
                    continue;
                }
            }

            return token;
        }

        /// <summary>
        /// Removes a client instance registration.
        /// </summary>
        /// <param name="token">An identifier for the client instance.</param>
        /// <returns>
        /// True if successfully found and removed, false otherwise.
        /// </returns>
        public bool RemoveClientRegistration(uint token)
        {
            lock (this.waitingClients)
            {
                return this.waitingClients.Remove(token);
            }
        }

        /// <summary>
        /// Matches a service connection to a waiting client and
        /// initiates forwarding operations.
        /// </summary>
        /// <param name="connection">
        /// Service connection for forwarding to client.
        /// </param>
        /// <param name="token">
        /// An unique identifier for this client instance.
        /// </param>
        /// <returns>
        /// True if forwarding was established, false otherwise.
        /// </returns>
        public bool MatchToClient(ServiceConnection connection, uint token)
        {
            ClientConnection found;
            lock (this.waitingClients)
            {
                this.waitingClients.TryGetValue(token, out found);
                if ((found == null) || !this.waitingClients.Remove(token))
                {
                    return false;
                }
            }

            // -
            // We first need to forward found's outstanding receive buffer to
            // this connection.  After that completes, it will call us back to
            // initiate perpetual forwarding.
            // -
            found.ForwardBufferTo(connection, this.InitiateForwarding);

            return true;
        }

        /// <summary>
        /// Starts a new forwarding agent to bi-directionally transfer data
        /// between an existing client connection and an existing service
        /// connection.
        /// </summary>
        /// <param name="client">The client connection.</param>
        /// <param name="service">The service connection.</param>
        public void InitiateForwarding(
            ClientConnection client,
            ServiceConnection service)
        {
            // -
            // Initiate bi-directional forwarding.
            // -
            Forwarder forwarder = new Forwarder(
                client.Socket,
                service.Socket,
                this.MatchEnding);
            lock (this.matches)
            {
                this.matches.Add(forwarder);
            }
        }

        /// <summary>
        /// Removes a forwarding pair from our list of matches.
        /// </summary>
        /// <param name="forwarder">The forwarder to remove.</param>
        private void MatchEnding(Forwarder forwarder)
        {
            lock (this.matches)
            {
                this.matches.Remove(forwarder);
            }
        }
    }
}
