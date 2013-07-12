// -
// <copyright file="ClientConnection.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

namespace HomeOS.Cloud.Platform.Gatekeeper
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using HomeOS.Shared;

    /// <summary>
    /// Represents a connection with a client.
    /// </summary>
    public class ClientConnection
    {
        /// <summary>
        /// The socket for this connection.
        /// </summary>
        private Socket socket;

        /// <summary>
        /// The collection of services and clients available for forwarding.
        /// </summary>
        private Matchmaker matchmaker;

        /// <summary>
        /// The I/O buffer for this connection.
        /// </summary>
        private byte[] buffer;

        /// <summary>
        /// The current byte offset into the buffer.
        /// </summary>
        private int bufferOffset;

        /// <summary>
        /// Arguments for an asychronous socket operation completion event.
        /// </summary>
        private SocketAsyncEventArgs eventArgs;

        /// <summary>
        /// The instance token when this connection is registered with the
        /// matchmaker.
        /// </summary>
        private uint token;

        /// <summary>
        /// The identifier of the home service we've requested forwarding to.
        /// </summary>
        private string homeServiceId;

        /// <summary>
        /// The service connection to forward our intial buffer of data to.
        /// </summary>
        private ServiceConnection service;

        /// <summary>
        /// The routine to call to initiate bi-directional forwarding.
        /// </summary>
        private InitiateForwardingCallback forwardingCallback;

        /// <summary>
        /// Initializes a new instance of the ClientConnection class.
        /// </summary>
        /// <param name="connected">The socket for the new connection.</param>
        /// <param name="matchmaker">The matchmaker service to use.</param>
        public ClientConnection(Socket connected, Matchmaker matchmaker)
        {
            // -
            // Set up the connection state.
            // -
            this.socket = connected;
            this.matchmaker = matchmaker;

            // -
            // Prepare our buffer space and asynchronous state holder.
            // This is currently just a simplistic single buffer system.
            // Note that this code assumes that the buffer is larger than
            // the largest possible HTTP Request-Line we want to handle.
            // -
            this.buffer = new byte[1500];
            this.bufferOffset = 0;
            this.eventArgs = new SocketAsyncEventArgs();
            this.eventArgs.UserToken = this;
            this.eventArgs.Completed +=
                new EventHandler<SocketAsyncEventArgs>(this.IOCompleted);
            this.eventArgs.SetBuffer(this.buffer, 0, this.buffer.Length);

            // -
            // Look for incoming data from the client.
            // -
            this.StartReceive(0, this.buffer.Length);
        }

        /// <summary>
        /// Handler to initiate bi-directional forwarding.
        /// </summary>
        /// <param name="client">The client connection to forward.</param>
        /// <param name="service">The service connection to forward.</param>
        public delegate void InitiateForwardingCallback(ClientConnection client, ServiceConnection service);

        /// <summary>
        /// Gets the socket for this connection.
        /// </summary>
        public Socket Socket
        {
            get { return this.socket; }
        }

        /// <summary>
        /// Forwards the current contents of this connection's buffer to the
        /// specified service.
        /// </summary>
        /// <param name="service">The service to forward to.</param>
        /// <param name="callback">
        /// A callback to notify when current buffer forwarding has completed.
        /// </param>
        public void ForwardBufferTo(
            ServiceConnection service,
            InitiateForwardingCallback callback)
        {
            this.service = service;
            this.forwardingCallback = callback;
            this.SendMessage();
        }

        /// <summary>
        /// Send the message currently in our send buffer, starting at
        /// the given byte offset and continuing for the given byte count.
        /// </summary>
        /// <param name="offset">The offset to start at in bytes.</param>
        /// <param name="count">The number of bytes to send.</param>
        private void SendMessage(int offset, int count)
        {
            this.eventArgs.SetBuffer(offset, count);
            bool asynchronous = this.service.Socket.SendAsync(this.eventArgs);
            if (!asynchronous)
            {
                this.IOCompleted(this, this.eventArgs);
            }
        }

        /// <summary>
        /// Send the message currently in our send buffer.
        /// </summary>
        private void SendMessage()
        {
            this.SendMessage(0, this.bufferOffset);
        }

        /// <summary>
        /// Start a receive operation.
        /// </summary>
        /// <param name="offset">
        /// The offset into the receive buffer at which to start receiving.
        /// </param>
        /// <param name="max">
        /// The maximum number of bytes to receive in this operation.
        /// </param>
        private void StartReceive(int offset, int max)
        {
            this.eventArgs.SetBuffer(offset, max);
            bool asynchronous = this.socket.ReceiveAsync(this.eventArgs);
            if (!asynchronous)
            {
                this.IOCompleted(this, this.eventArgs);
            }
        }

        /// <summary>
        /// Parses a potential HTTP Request-Line (per RFC 2616).
        /// </summary>
        /// <remarks>
        /// Has the side effect of setting homeServiceId, if found.
        /// </remarks>
        /// <param name="input">The prospective request line.</param>
        /// <returns>
        /// True if we parsed a valid HTTP Request-Line, false otherwise.
        /// </returns>
        private bool ParseHttpRequestLine(ArraySegment<byte> input)
        {
            ASCIIEncoding ascii = new ASCIIEncoding();
            string line = ascii.GetString(
                input.Array,
                input.Offset,
                input.Count);

            // -
            // The HTTP Request-Line should contain three fields
            // delimited by spaces: Method, Request-URI, HTTP-Version.
            // -
            char[] space = new char[1] { ' ' };
            string[] tokens = line.Split(
                space,
                3,
                StringSplitOptions.RemoveEmptyEntries);
            if ((tokens.Length < 3) ||
                (Array.IndexOf(Constants.ValidHttpMethods, tokens[0]) < 0))
            {
                return false;
            }

            // -
            // We should now have the Uri in tokens[1].
            // While it probably is a relative one, we also handle
            // the case where it is an absolute one, as the RFC allows for it.
            // -
            string got = null;
            try
            {
                Uri uri = new Uri(tokens[1], UriKind.RelativeOrAbsolute);
                if (uri.IsAbsoluteUri == false)
                {
                    uri = new Uri(new Uri("http://bogus"), uri);
                }

                if (uri.Segments.Length < 3)
                {
                    // -
                    // Not enough segments.
                    // We didn't parse out a home service id.
                    // -
                    return false;
                }

                char[] slash = new char[1] { '/' };
                got = uri.Segments[1].TrimEnd(slash);
            }
            catch
            {
                return false;
            }

            //return HomeId.TryParse(got, out this.homeServiceId);

            this.homeServiceId = got;
            return true;

        }

        /// <summary>
        /// Handler for the Completed event of an asynchronous socket operation.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="ea">Arguments pertaining to this event.</param>
        private void IOCompleted(object sender, SocketAsyncEventArgs ea)
        {
            if (ea.SocketError != SocketError.Success)
            {
                // -
                // If something failed, close the connection.
                // -
                this.ShutdownAndClose();
                return;
            }

            if (ea.LastOperation == SocketAsyncOperation.Send)
            {
                // -
                // Send completed.
                // -
                int outstanding = ea.Count - ea.BytesTransferred;
                if (outstanding > 0)
                {
                    // -
                    // Still have data to send.
                    // -
                    this.SendMessage(
                        ea.Offset + ea.BytesTransferred,
                        outstanding);
                    return;
                }

                // -
                // Completed forwarding first buffer.
                // Initiate perpetual forwarding.
                // -
                this.forwardingCallback(this, this.service);
            }
            else
            {
                // -
                // Receive completed.
                // -
                if (ea.BytesTransferred == 0)
                {
                    // -
                    // Our peer closed the connection.  Reciprocate.
                    // -
                    this.ShutdownAndClose();
                    return;
                }

                // -
                // Check for a HTTP Request-Line from the client.
                // We ignore any empty line(s) preceeding the Request-Line.
                // Note that the Request-Line ends with a CRLF.
                //
                // We have three cases to deal with at this point:
                //  1. We have a complete Request-Line from the client.
                //  2. We have a partial Request-Line,
                //     (a) and our receive buffer is full.
                //     (b) and still have room in our receive buffer.
                // -
                int have = ea.Offset + ea.BytesTransferred - this.bufferOffset;
                while (have != 0)
                {
                    if (have > 1)
                    {
                        // -
                        // Check for extraneous blank lines.
                        // -
                        if ((this.buffer[this.bufferOffset] == 13) &&
                            (this.buffer[this.bufferOffset + 1] == 10))
                        {
                            have -= 2;
                            this.bufferOffset += 2;
                            continue;
                        }

                        // -
                        // Check for complete Request-Line.
                        // -
                        for (int index = 1; index < have; index++)
                        {
                            if ((this.buffer[this.bufferOffset + index] == 13) &&
                                (this.buffer[this.bufferOffset + index + 1] == 10))
                            {
                                // -
                                // We have a complete line.
                                // -
                                ArraySegment<byte> line;
                                line = new ArraySegment<byte>(
                                    this.buffer,
                                    this.bufferOffset,
                                    index);
                                if (this.ParseHttpRequestLine(line))
                                {
                                    this.bufferOffset = ea.Offset +
                                        ea.BytesTransferred;
                                    this.token = this.matchmaker.RegisterClient(
                                        this);
                                    ServiceConnection service =
                                        this.matchmaker.LookupService(
                                        this.homeServiceId);
                                    if (service != null)
                                    {
                                        service.SendClientForwardingRequest(
                                            this.token);
                                        return;
                                    }
                                }

                                this.ShutdownAndClose();
                                return;
                            }
                        }
                    }

                    // -
                    // We have a partial Request-Line.
                    // -
                    if (ea.Count == ea.BytesTransferred)
                    {
                        // -
                        // Our receive buffer is full.
                        // If current partial message starts at zero index
                        // (i.e. there are no blank lines in the buffer),
                        // give up as the Request-Line is unreasonably long.
                        // -
                        if (this.bufferOffset == 0)
                        {
                            this.ShutdownAndClose();
                            return;
                        }

                        // -
                        // Shift the start of current partial message down to
                        // zero index.
                        // -
                        int partialLength = this.buffer.Length -
                            this.bufferOffset;
                        Array.Copy(
                            this.buffer,
                            this.bufferOffset,
                            this.buffer,
                            0,
                            partialLength);

                        this.bufferOffset = 0;
                        this.StartReceive(
                            partialLength,
                            this.buffer.Length - partialLength);
                        return;
                    }

                    // -
                    // Start another receive to fill in the buffer from where
                    // the last one left off.
                    // -
                    this.StartReceive(
                        ea.Offset + ea.BytesTransferred,
                        ea.Count - ea.BytesTransferred);
                    return;
                }

                // -
                // We saw some number of blank lines and nothing else.
                // We're still expecting a Request-Line, so restart receive.
                // -
                this.bufferOffset = 0;
                this.StartReceive(0, this.buffer.Length);
            }
        }

        /// <summary>
        /// Completes outstanding operations on, and then closes, our socket.
        /// </summary>
        private void ShutdownAndClose()
        {
            try
            {
                this.socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                // -
                // Shutdown will throw if the socket is already closed.
                // This is benign, so we just swallow the exception.
                // -
            }

            this.socket.Close();

            this.CleanupExternalState();
        }

        /// <summary>
        /// Cleanup any state in external components this is related to this
        /// connection.
        /// </summary>
        private void CleanupExternalState()
        {
            if (this.token != 0)
            {
                this.matchmaker.RemoveClientRegistration(this.token);
            }
        }
    }
}
