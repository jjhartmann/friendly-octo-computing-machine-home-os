// -
// <copyright file="ConnectionListener.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

namespace HomeOS.Cloud.Platform.Gatekeeper
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Represents a listening agent that handles incoming connection requests.
    /// </summary>
    public class ConnectionListener
    {
        /// <summary>
        /// The array of listening sockets.
        /// </summary>
        private Socket[] listeners;

        /// <summary>
        /// The number of incoming connections accepted to date.
        /// </summary>
        private int connectionsAccepted;

        /// <summary>
        /// The handler routine to call upon each accepted connection.
        /// </summary>
        private ConnectionAcceptHandler handler;

        /// <summary>
        /// Initializes a new instance of the ConnectionListener class.
        /// </summary>
        /// <param name="port">The port number to listen on.</param>
        /// <param name="handler">
        /// The handler routine to call upon each accepted connection.
        /// </param>
        public ConnectionListener(int port, ConnectionAcceptHandler handler)
        {
            this.handler = handler;
            this.connectionsAccepted = 0;

            this.listeners = new Socket[2];

            this.listeners[0] = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            this.listeners[1] = new Socket(
                AddressFamily.InterNetworkV6,
                SocketType.Stream,
                ProtocolType.Tcp);

            this.listeners[0].Bind(new IPEndPoint(IPAddress.Any, port));
            this.listeners[1].Bind(new IPEndPoint(IPAddress.IPv6Any, port));

            this.listeners[0].Listen(5);
            this.listeners[1].Listen(5);

            this.StartAccept(
                this.listeners[0],
                this.CreateSocketAsyncEventArgsForAccept());
            this.StartAccept(
                this.listeners[1],
                this.CreateSocketAsyncEventArgsForAccept());
        }

        /// <summary>
        /// A handler for incoming connections.
        /// </summary>
        /// <param name="accepted">
        /// The socket for the accepted connection.
        /// </param>
        public delegate void ConnectionAcceptHandler(Socket accepted);

        /// <summary>
        /// Gets the number of incoming connections accepted to date.
        /// </summary>
        public int ConnectionsAccepted
        {
            get { return this.connectionsAccepted; }
        }

        /// <summary>
        /// Closes the ConnectionListener and releases all resources.
        /// </summary>
        public void Close()
        {
            foreach (Socket listener in this.listeners)
            {
                listener.Close();
            }
        }

        /// <summary>
        /// Creates a SocketAsyncEventArgs object with our AcceptCompleted
        /// routine as the handler for the Completed event.
        /// </summary>
        /// <returns>An initialized SocketAsyncEventArgs object.</returns>
        private SocketAsyncEventArgs CreateSocketAsyncEventArgsForAccept()
        {
            SocketAsyncEventArgs eventArgsForAccept = new SocketAsyncEventArgs();
            eventArgsForAccept.Completed +=
                new EventHandler<SocketAsyncEventArgs>(this.AcceptCompleted);

            return eventArgsForAccept;
        }

        /// <summary>
        /// Initiate an asynchronous accept operation.
        /// </summary>
        /// <param name="listener">
        /// The listening socket on which to operate.
        /// </param>
        /// <param name="eventArgs">
        /// The asynchronous state object to use.
        /// </param>
        private void StartAccept(
            Socket listener,
            SocketAsyncEventArgs eventArgs)
        {
            eventArgs.UserToken = listener;

            bool asynchronous = listener.AcceptAsync(eventArgs);
            if (!asynchronous)
            {
                this.AcceptCompleted(this, eventArgs);
            }
        }

        /// <summary>
        /// Handler for the Completed event of an AsyncAccept socket operation.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="ea">Arguments pertaining to this event.</param>
        private void AcceptCompleted(object sender, SocketAsyncEventArgs ea)
        {
            // -
            // Extract the accepted socket from the event args.
            // -
            Socket accepted = ea.AcceptSocket;

            if (ea.SocketError != SocketError.Success)
            {
                // -
                // Something failed.  Clean up as best we can.
                // We currently don't re-issue the accept operation
                // as it is hard to know whether it is safe to do so.
                // Ideally, we'd re-create our listening socket, but
                // even that can be problematic.
                // -
                if (accepted != null)
                {
                    accepted.Close();
                }

                return;
            }

            // -
            // Clean up the event args object, and re-issue the accept.
            // -
            ea.AcceptSocket = null;
            this.StartAccept((Socket)ea.UserToken, ea);

            // -
            // Call the supplied handler, passing it the newly accepted
            // connection.
            // -
            Interlocked.Increment(ref this.connectionsAccepted);
            this.handler(accepted);
        }
    }
}