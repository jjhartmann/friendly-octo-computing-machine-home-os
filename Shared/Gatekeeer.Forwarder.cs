// -
// <copyright file="Forwarder.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

namespace HomeOS.Shared.Gatekeeper
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Represents a forwarder that bi-directionally forwards all
    /// data from one connection to another.
    /// </summary>
    public class Forwarder
    {
        /// <summary>
        /// External handler for close event.
        /// </summary>
        private ForwarderCloseHandler closeHandler;

        /// <summary>
        /// The sockets representing the local end of each connection.
        /// </summary>
        private Socket[] sockets;

        /// <summary>
        /// Indicates whether one side or the other stopped sending.
        /// </summary>
        private bool halfOpen;

        /// <summary>
        /// Indicates whether the connection is closing.
        /// </summary>
        private int closing;

        /// <summary>
        /// State kept for each direction we're forwarding.
        /// </summary>
        private PerDirection[] directions;

        /// <summary>
        /// Initializes a new instance of the Forwarder class.
        /// </summary>
        /// <param name="a">The socket for one connection.</param>
        /// <param name="b">The socket for the other connection.</param>
        /// <param name="closeHandler">
        /// An optional close callback handler.
        /// </param>
        public Forwarder(Socket a, Socket b, ForwarderCloseHandler closeHandler)
        {
            this.sockets = new Socket[2] { a, b };
            this.closeHandler = closeHandler;
            this.halfOpen = false;
            this.closing = 0;
            this.directions = new PerDirection[2];
            this.directions[0] = new PerDirection(this, a, b);
            this.directions[1] = new PerDirection(this, b, a);
        }

        /// <summary>
        /// A handler for closing forwarders.
        /// </summary>
        /// <param name="forwarder">
        /// The forwarder that is closing.
        /// </param>
        public delegate void ForwarderCloseHandler(Forwarder forwarder);

        /// <summary>
        /// Closes this forwarder.
        /// </summary>
        public void Close()
        {
            if (Interlocked.Exchange(ref this.closing, 1) == 1)
            {
                // -
                // Already closing.
                // -
                return;
            }

            foreach (Socket socket in this.sockets)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                    // -
                    // Ignore any Shutdown exceptions.
                    // -
                }

                socket.Close();
            }

            if (this.closeHandler != null)
            {
                this.closeHandler(this);
            }
        }

        /// <summary>
        /// Represents each direction (A to B, B to A) of a forwarding pair.
        /// </summary>
        private class PerDirection
        {
            /// <summary>
            /// The forwarder of which we are one direction.
            /// </summary>
            private Forwarder forwarder;

            /// <summary>
            /// The sockets representing the local end of each connection.
            /// </summary>
            private Socket inbound, outbound;

            /// <summary>
            /// The buffer used to hold data being forwarded in this direction.
            /// </summary>
            private byte[] buffer;

            /// <summary>
            /// State for asynchronous socket operations.
            /// </summary>
            private SocketAsyncEventArgs eventArgs;

            /// <summary>
            /// Initializes a new instance of the PerDirection class.
            /// </summary>
            /// <param name="forwarder">
            /// The forwarder object to which this instance belongs.
            /// </param>
            /// <param name="from">The connection to read from.</param>
            /// <param name="to">The connection to write to.</param>
            public PerDirection(Forwarder forwarder, Socket from, Socket to)
            {
                this.forwarder = forwarder;
                this.inbound = from;
                this.outbound = to;
                this.buffer = new byte[1500];
                this.eventArgs = new SocketAsyncEventArgs();
                this.eventArgs.UserToken = this;  // Keep GC from collecting us.
                this.eventArgs.SetBuffer(this.buffer, 0, this.buffer.Length);
                this.eventArgs.Completed +=
                    new EventHandler<SocketAsyncEventArgs>(this.IOCompleted);

                // -
                // Start things going by issuing a receive on the inbound side.
                // -
                this.StartReceive();
            }

            /// <summary>
            /// Starts a receive operation on our inbound socket.
            /// </summary>
            private void StartReceive()
            {
                this.eventArgs.SetBuffer(0, this.buffer.Length);
                bool asynchronous = this.inbound.ReceiveAsync(this.eventArgs);
                if (!asynchronous)
                {
                    this.IOCompleted(this, this.eventArgs);
                }
            }

            /// <summary>
            /// Starts a send operation on our outbound socket.
            /// </summary>
            /// <param name="offset">
            /// The offset into the buffer from which to start sending.
            /// </param>
            /// <param name="count">
            /// The amount (in bytes) of data to send.
            /// </param>
            private void StartSend(int offset, int count)
            {
                this.eventArgs.SetBuffer(offset, count);
                bool asynchronous = this.outbound.SendAsync(this.eventArgs);
                if (!asynchronous)
                {
                    this.IOCompleted(this, this.eventArgs);
                }
            }

            /// <summary>
            /// Handles an asynchronous socket operation Completed event.
            /// </summary>
            /// <param name="sender">The sender of this event.</param>
            /// <param name="ea">Arguments pertaining to this event.</param>
            private void IOCompleted(object sender, SocketAsyncEventArgs ea)
            {
                if (ea.LastOperation == SocketAsyncOperation.Send)
                {
                    // -
                    // Send completed.  Check for errors.
                    // -
                    if (ea.SocketError != SocketError.Success)
                    {
                        this.HandleSendError(ea.SocketError);
                        return;
                    }

                    int outstanding = ea.Count - ea.BytesTransferred;
                    if (outstanding > 0)
                    {
                        // -
                        // Still have data in the buffer to send.
                        // Start another send.
                        // -
                        this.StartSend(
                            ea.Offset + ea.BytesTransferred,
                            outstanding);
                        return;
                    }

                    // -
                    // Switch to receive mode and wait for more data to forward.
                    // -
                    this.StartReceive();
                }
                else
                {
                    // -
                    // Receive completed.  Check for errors.
                    // -
                    if (ea.SocketError != SocketError.Success)
                    {
                        this.HandleReceiveError(ea.SocketError);
                        return;
                    }

                    if (ea.BytesTransferred == 0)
                    {
                        // -
                        // Our inbound side quit sending.
                        // -
                        this.HandleReceiveError(SocketError.Disconnecting);
                        return;
                    }

                    // -
                    // Switch to send mode and forward the data we received.
                    // -
                    this.StartSend(0, ea.BytesTransferred);
                }
            }

            /// <summary>
            /// Handles various receive errors.
            /// </summary>
            /// <param name="error">The error to handle.</param>
            private void HandleReceiveError(SocketError error)
            {
                switch (error)
                {
                    case SocketError.ConnectionReset:
                        // An abortive close has occurred.
                        // We should be able to use a Linger socket option
                        // of zero combined with a shutdown call to forward
                        // this abortive closure outbound.
                        try
                        {
                            this.outbound.LingerState = new LingerOption(true, 0);
                        }
                        catch
                        {
                            //ratul put in this try catch because he encountered an exception in testing
                            //not sure if there is something we should do about it
                        }
                        break;

                    case SocketError.Disconnecting:
                        // -
                        // A graceful close has occurred.
                        // -
                        break;
                }

                if (this.forwarder.halfOpen == false)
                {
                    try
                    {
                        this.outbound.Shutdown(SocketShutdown.Send);
                    }
                    catch
                    {
                        this.forwarder.Close();
                    }

                    this.forwarder.halfOpen = true;
                }
                else
                {
                    this.forwarder.Close();
                }
            }

            /// <summary>
            /// Handles various send errors.
            /// </summary>
            /// <param name="error">The error to handle.</param>
            private void HandleSendError(SocketError error)
            {
                this.forwarder.Close();
            }
        }
    }
}
