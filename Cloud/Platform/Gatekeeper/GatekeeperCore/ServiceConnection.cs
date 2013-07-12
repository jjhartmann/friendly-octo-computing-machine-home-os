// -
// <copyright file="ServiceConnection.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

namespace HomeOS.Cloud.Platform.Gatekeeper
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using HomeOS.Shared;
    using HomeOS.Shared.Gatekeeper;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a connection with a home service.
    /// </summary>
    public class ServiceConnection
    {
        /// <summary>
        /// The version number for our protocol.
        /// </summary>
        private const byte ProtocolVersion = 1;

        /// <summary>
        /// The socket for this connection.
        /// </summary>
        private Socket socket;

        /// <summary>
        /// The authentication authority for this connection.
        /// </summary>
        private Keymaster keymaster;

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
        /// The current state of this connection.
        /// </summary>
        private ConnectionState state;

        /// <summary>
        /// The version number of the protocol our peer is using.
        /// </summary>
        private byte peerProtocolVersion;

        /// <summary>
        /// The identifier claimed by our peer.
        /// </summary>
        private string peerIdentifier;

        /// <summary>
        /// The simple authentication value presented by our peer.
        /// </summary>
        private uint peerSimpleAuthentication;

        private Queue<byte[]> outgoingMessages = new Queue<byte[]>();

        private enum SocketState { Idle, Sending, Receiving };

        private SocketState socketState = SocketState.Idle;

#if false
        /// <summary>
        /// The instance token when this connection is registered with
        /// the matchmaker.
        /// </summary>
        private uint token;
#endif

        /// <summary>
        /// Initializes a new instance of the ServiceConnection class.
        /// </summary>
        /// <param name="connected">The socket for the new connection.</param>
        /// <param name="keymaster">The authentication authority to use.</param>
        /// <param name="matchmaker">The matchmaker service to use.</param>
        public ServiceConnection(
            Socket connected,
            Keymaster keymaster,
            Matchmaker matchmaker)
        {
            // -
            // Set up the connection state.
            // -
            this.socket = connected;
            this.keymaster = keymaster;
            this.matchmaker = matchmaker;
            this.state = ConnectionState.PeerNotVerified;

#if true
            // -
            // We use keep-alives on the home <-> cloud service
            // connection in an attempt to prevent NAT/firewall
            // state from timing out and dropping our connection.
            // -
            StaticUtilities.SetKeepAlive(this.socket, 50000, 1000);
#endif

            // -
            // Prepare our buffer space and asynchronous state holder.
            // This is currently just a simplistic single buffer system.
            // Note that this code assumes that the buffer is larger than
            // the largest possible single message (currently 257 bytes).
            // -
            this.buffer = new byte[1500];
            this.bufferOffset = 0;
            this.eventArgs = new SocketAsyncEventArgs();
            this.eventArgs.Completed +=
                new EventHandler<SocketAsyncEventArgs>(this.IOCompleted);
            this.eventArgs.SetBuffer(this.buffer, 0, this.buffer.Length);
            this.eventArgs.UserToken = this;  // Keep GC from collecting us.

            // -
            // Start the dialog with our peer.
            // -
            this.AppendMessage(
                MessageType.Version,
                ServiceConnection.ProtocolVersion);
            this.AppendMessage(MessageType.PleaseIdentify);
            //this.SendMessage();
        }

        /// <summary>
        /// The possible states a connection may be in.
        /// </summary>
        private enum ConnectionState : byte
        {
            /// <summary>
            /// Peer's identification has not been verified.
            /// </summary>
            PeerNotVerified,

            /// <summary>
            /// Peer's identification has been verified.
            /// </summary>
            PeerIdVerified,

            /// <summary>
            /// Peer has registered as a home service.
            /// </summary>
            ServiceRegistered,

            /// <summary>
            /// Peer has requested to be forwarded to a service.
            /// </summary>
            ForwardingRequested,

            /// <summary>
            /// Peer has been handed off to the forwarding agent.
            /// </summary>
            Forwarding,
        }

        /// <summary>
        /// The types of messages in this protocol.
        /// All messages (except Pad1) consist of three parts:
        ///  Type - One of these MessageType codes.
        ///  Length - The length of the message (in bytes).
        ///  Value - The data (0 to 255 bytes).
        /// </summary>
        private enum MessageType : byte
        {
            /// <summary>
            /// Special one-byte pad message.
            /// </summary>
            Pad1 = 0,

            /// <summary>
            /// Variable length (2-257 bytes) pad message.
            /// </summary>
            PadN = 1,

            /// <summary>
            /// Request to echo data back to sender.
            /// </summary>
            EchoRequest = 2,

            /// <summary>
            /// Reply to an echo request message.
            /// </summary>
            EchoReply = 3,

            /// <summary>
            /// Protocol version number.
            /// </summary>
            Version = 4,

            /// <summary>
            /// Request for identification.
            /// </summary>
            PleaseIdentify = 5,

            /// <summary>
            /// Identification information.
            /// </summary>
            Identification = 6,

            /// <summary>
            /// Request for (simple) authentication.
            /// </summary>
            PleaseAuthenticate = 7,

            /// <summary>
            /// Simple authentication (i.e. password).
            /// </summary>
            SimpleAuthentication = 8,

            /// <summary>
            /// Challenge portion of a challenge/reponse authentication.
            /// </summary>
            AuthenticationChallenge = 9,

            /// <summary>
            /// Reponse portion of a challenge/reponse authentication.
            /// </summary>
            AuthenticationChallengeReponse = 10,

            /// <summary>
            /// Authentication acknowledged.
            /// </summary>
            Authenticated = 11,

            /// <summary>
            /// Register a service with the HomeOS.Cloud.Platform.Gatekeeper.CloudServiceCore service.
            /// </summary>
            RegisterService = 12,

            /// <summary>
            /// Request list of available services be sent.
            /// </summary>
            SendServiceList = 13,

            /// <summary>
            /// List of available services.
            /// </summary>
            ServiceList = 14,

            /// <summary>
            /// Request connection forwarding to the specified home service.
            /// </summary>
            ForwardToService = 15,

            /// <summary>
            /// Notification that a client is waiting for connection to a
            /// home service.
            /// </summary>
            ClientAwaits = 16,

            /// <summary>
            /// Request connection forwarding to the specified client.
            /// </summary>
            ForwardToClient = 17,
        }

        /// <summary>
        /// Types of authentication.
        /// </summary>
        private enum AuthenticationType : byte
        {
            /// <summary>
            /// No authentication required.
            /// </summary>
            None = 0,

            /// <summary>
            /// Use simple authentication (i.e. password).
            /// </summary>
            Simple = 1,
        }

        /// <summary>
        /// Gets the socket for this connection.
        /// </summary>
        public Socket Socket
        {
            get { return this.socket; }
        }

        /// <summary>
        /// Send a request for client forwarding to a service.
        /// </summary>
        /// <param name="token">
        /// An unique identifier for the client instance.
        /// </param>
        /// <returns>True if request sent, false otherwise.</returns>
        public bool SendClientForwardingRequest(uint token)
        {
            // -
            // We can only send a request to a service if our peer is in fact
            // a registered service.
            // -
            if (this.state != ConnectionState.ServiceRegistered)
            {
                return false;
            }

            //// -
            //// ToDo: We need to guard against having multiple forwarding
            //// requests outstanding at once and corrupting our send buffer.
            //// Should have a queue and pull off a new one as each asynch
            //// send completes.
            //// -
            //this.bufferOffset = 0;
            //this.AppendMessage(
            //    MessageType.ClientAwaits,
            //    BitConverter.GetBytes(token));
            //this.SendMessage();

            this.AppendMessage(MessageType.ClientAwaits, 
                                BitConverter.GetBytes(token));

            return true;
        }

        /// <summary>
        /// Append a message to our send buffer.
        /// </summary>
        /// <param name="type">The message type.</param>
        private void AppendMessage(MessageType type)
        {
            //if (this.bufferOffset + 2 > this.buffer.Length)
            //{
            //    return;
            //}

            //this.buffer[this.bufferOffset++] = (byte)type;
            //this.buffer[this.bufferOffset++] = 0;

            byte[] message = new byte[2];
            message[0] = (byte)type;
            message[1] = 0;

            TrySend(message);
        }

        /// <summary>
        /// Append a message to our send buffer.
        /// </summary>
        /// <param name="type">The message type.</param>
        /// <param name="data">The message data.</param>
        private void AppendMessage(MessageType type, byte data)
        {
            //if (this.bufferOffset + 3 > this.buffer.Length)
            //{
            //    return;
            //}

            //this.buffer[this.bufferOffset++] = (byte)type;
            //this.buffer[this.bufferOffset++] = 1;
            //this.buffer[this.bufferOffset++] = data;

            byte[] message = new byte[3];
            message[0] = (byte)type;
            message[1] = 1;
            message[2] = data;

            TrySend(message);
        }

        /// <summary>
        /// Append a message to our send buffer.
        /// </summary>
        /// <param name="type">The message type.</param>
        /// <param name="data">The message data.</param>
        private void AppendMessage(MessageType type, int data)
        {
            byte[] dataBytes = BitConverter.GetBytes(data);
            this.AppendMessage(type, dataBytes);
        }

        /// <summary>
        /// Append a message to our send buffer.
        /// </summary>
        /// <param name="type">The message type.</param>
        /// <param name="data">The message data.</param>
        private void AppendMessage(MessageType type, byte[] data)
        {
            //if (this.bufferOffset + 2 + data.Length > this.buffer.Length)
            //{
            //    return;
            //}

            //this.buffer[this.bufferOffset++] = (byte)type;
            //this.buffer[this.bufferOffset++] = (byte)data.Length;
            //Array.Copy(data, 0, this.buffer, this.bufferOffset, data.Length);
            //this.bufferOffset += data.Length;

            byte[] message = new byte[2 + data.Length];
            message[0] = (byte)type;
            message[1] = (byte) data.Length;
            Array.Copy(data, 0, message, 2, data.Length);

            TrySend(message);
        }

        /// <summary>
        /// Append a message to our send buffer.
        /// </summary>
        /// <param name="type">The message type.</param>
        /// <param name="data">The message data.</param>
        private void AppendMessage(MessageType type, ArraySegment<byte> data)
        {
            //if (this.bufferOffset + 2 + data.Count > this.buffer.Length)
            //{
            //    return;
            //}

            //this.buffer[this.bufferOffset++] = (byte)type;
            //this.buffer[this.bufferOffset++] = (byte)data.Count;
            //Array.Copy(
            //    data.Array,
            //    data.Offset,
            //    this.buffer,
            //    this.bufferOffset,
            //    data.Count);
            //this.bufferOffset += data.Count;

            byte[] message = new byte[2 + data.Count];
            message[0] = (byte)type;
            message[1] = (byte)data.Count;
            Array.Copy(data.Array, data.Offset, message, 2, data.Count);

            TrySend(message);
        }

        private void TrySend(byte[] message)
        {
            lock (this)
            {
                //first, lets queue this message
                outgoingMessages.Enqueue(message);

                TrySend();
            }
        }

        private void TrySend()
        {
            lock (this)
            {
                if (socketState == SocketState.Idle)
                {
                    var messageToSend = outgoingMessages.Dequeue();

                    socketState = SocketState.Sending;

                    this.eventArgs.SetBuffer(messageToSend, 0, messageToSend.Length);
                    bool asynchronous = this.socket.SendAsync(this.eventArgs);
                    if (!asynchronous)
                    {
                        this.IOCompleted(this, this.eventArgs);
                    }
                }
            }
        }

        private void SendRemainingBytes(int offset, int count)
        {
            lock (this)
            {
                if (socketState == SocketState.Idle)
                {
                    socketState = SocketState.Sending;

                    this.eventArgs.SetBuffer(offset, count);
                    bool asynchronous = this.socket.SendAsync(this.eventArgs);
                    if (!asynchronous)
                    {
                        this.IOCompleted(this, this.eventArgs);
                    }
                }
                else
                {
                    Console.WriteLine("Error: we shouldn't have come here 2");
                }
            }
        }

        ///// <summary>
        ///// Send the message currently in our send buffer, starting at
        ///// the given byte offset and continuing for the given byte count.
        ///// </summary>
        ///// <param name="offset">The offset to start at in bytes.</param>
        ///// <param name="count">The number of bytes to send.</param>
        //private void SendMessage(int offset, int count)
        //{
        //    this.eventArgs.SetBuffer(offset, count);
        //    bool asynchronous = this.socket.SendAsync(this.eventArgs);
        //    if (!asynchronous)
        //    {
        //        this.IOCompleted(this, this.eventArgs);
        //    }
        //}

        ///// <summary>
        ///// Send the message currently in our send buffer.
        ///// </summary>
        //private void SendMessage()
        //{
        //    this.SendMessage(0, this.bufferOffset);
        //}

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
            lock (this)
            {
                if (socketState == SocketState.Idle)
                {

                    socketState = SocketState.Receiving;

                    this.eventArgs.SetBuffer(buffer, offset, max);
                    bool asynchronous = this.socket.ReceiveAsync(this.eventArgs);
                    if (!asynchronous)
                    {
                        this.IOCompleted(this, this.eventArgs);
                    }
                }
                else
                {
                    Console.WriteLine("Error: we shouldn't have come here!");
                }
            }
        }

        /// <summary>
        /// Handler for all message types.
        /// </summary>
        /// <remarks>
        /// Note we must not operate on the passed-in 'data' parameter after
        /// this routine returns, as it just references the receive buffer.
        /// </remarks>
        /// <param name="type">The message type.</param>
        /// <param name="data">The message data.</param>
        /// <returns>
        /// True if we entered send mode as a result of receiving this message,
        /// false if we're staying in receive mode and should re-start receive.
        /// </returns>
        private bool HandleMessage(MessageType type, ArraySegment<byte> data)
        {
            switch (type)
            {
                case MessageType.PadN:
                    break;

                case MessageType.EchoRequest:
                    //this.bufferOffset = 0;
                    this.AppendMessage(MessageType.EchoReply, data);
                    //this.SendMessage();
                    return true;

                case MessageType.EchoReply:
                    break;

                case MessageType.Version:
                    if (data.Count != sizeof(byte))
                    {
                        this.ShutdownAndClose();
                        return true;
                    }

                    this.peerProtocolVersion = data.Array[data.Offset];
                    break;

                case MessageType.Identification:
                    //if (!HomeId.TryParse(data, out this.peerIdentifier))
                    //{
                    //    this.ShutdownAndClose();
                    //    return true;
                    //}
                    byte[] temp = new byte[data.Count];
                    Array.Copy(data.Array, data.Offset, temp, 0, data.Count);
                    this.peerIdentifier = System.Text.Encoding.ASCII.GetString(temp);

                    if (this.peerSimpleAuthentication != 0)
                    {
                        // -
                        // We already have the simple auth info,
                        // so we can attempt peer authentication.
                        // -
                        if (this.keymaster.VerifySimpleAuthentication(
                            this.peerIdentifier,
                            this.peerSimpleAuthentication))
                        {
                            this.state = ConnectionState.PeerIdVerified;
                            //this.bufferOffset = 0;
                            this.AppendMessage(MessageType.Authenticated);
                            //this.SendMessage();
                        }
                        else
                        {
                            this.ShutdownAndClose();
                        }
                    }
                    else
                    {
                        // -
                        // We need our peer's authentication info.
                        // -
                        //this.bufferOffset = 0;
                        this.AppendMessage(
                            MessageType.PleaseAuthenticate,
                            (byte)AuthenticationType.Simple);
                        //this.SendMessage();
                    }

                    return true;

                case MessageType.SimpleAuthentication:
                    if (data.Count != sizeof(uint))
                    {
                        this.ShutdownAndClose();
                        return true;
                    }

                    this.peerSimpleAuthentication = BitConverter.ToUInt32(
                        data.Array,
                        data.Offset);
                    if (this.peerIdentifier != null)
                    {
                        // -
                        // Attempt peer authentication.
                        // -
                        if (this.keymaster.VerifySimpleAuthentication(
                            this.peerIdentifier,
                            this.peerSimpleAuthentication))
                        {
                            this.state = ConnectionState.PeerIdVerified;
                            //this.bufferOffset = 0;
                            this.AppendMessage(MessageType.Authenticated);
                            //this.SendMessage();
                        }
                        else
                        {
                            this.ShutdownAndClose();
                        }
                    }
                    else
                    {
                        // -
                        // Allow simple authentication to be sent pro-actively
                        // prior to identification.
                        // -
                        return false;
                    }

                    return true;

                default:
                    // -
                    // All other message types require an authenticated client.
                    // -
                    if (this.state < ConnectionState.PeerIdVerified)
                    {
                        this.ShutdownAndClose();
                    }

                    switch (type)
                    {
                        case MessageType.RegisterService:
                            if (this.matchmaker.RegisterService(
                                this.peerIdentifier,
                                this))
                            {
                                this.state = ConnectionState.ServiceRegistered;

                                // -
                                // We no longer expect to receive anything on
                                // this connection -- it is "send only" (we
                                // will still send ClientAwaits messages on it).
                                //
                                // But we want to know if the remote end (i.e.
                                // the home service) goes away -- so we post a
                                // bogus receive on it so we will get notified
                                // if TCP tears down the connection.
                                // -
                                this.WatchForRemoteClose();
                            }
                            else
                            {
                                this.ShutdownAndClose();
                            }

                            return true;

                        case MessageType.SendServiceList:
                            //this.bufferOffset = 0;
                            this.AppendMessage(
                                MessageType.ServiceList,
                                this.GetServiceList());
                            //this.SendMessage();
                            return true;

#if false
                        case MessageType.ForwardToService:
                            if (data.Count != sizeof(uint))
                            {
                                this.ShutdownAndClose();
                                return true;
                            }

                            this.token = this.matchmaker.RegisterClient(this);
                            this.state = ConnectionState.ForwardingRequested;
                            uint serviceId = BitConverter.ToUInt32(
                                data.Array,
                                data.Offset);
                            Connection service = this.matchmaker.LookupService(
                                serviceId);
                            if (service != null)
                            {
                                service.SendClientForwardingRequest(this.token);
                            }
                            else
                            {
                                this.ShutdownAndClose();
                            }

                            return true;
#endif

                        case MessageType.ForwardToClient:
                            if (data.Count != sizeof(uint))
                            {
                                this.ShutdownAndClose();
                                return true;
                            }

                            uint clientToken = BitConverter.ToUInt32(
                                data.Array,
                                data.Offset);
                            if (this.matchmaker.MatchToClient(
                                this,
                                clientToken))
                            {
                                this.state = ConnectionState.Forwarding;
                            }
                            else
                            {
                                this.ShutdownAndClose();
                            }

                            return true;
                    }

                    break;
            }

            return false;
        }

        /// <summary>
        /// Gets a list of currently registered services.
        /// </summary>
        /// <returns>A list of currently registered services.</returns>
        private ArraySegment<byte> GetServiceList()
        {
            string[] registeredServices = this.matchmaker.RegisteredServices;

            int bufferNeeded = 0;
            foreach (string homeId in registeredServices)
            {
                //bufferNeeded += homeId.ByteLength + 1;
                bufferNeeded += System.Text.Encoding.ASCII.GetBytes(homeId).Length + 1;
            }

            byte[] listBuffer = new byte[bufferNeeded];
            int index = 0;
            foreach (string homeId in registeredServices)
            {
                byte[] toAdd = System.Text.Encoding.ASCII.GetBytes(homeId);
                toAdd.CopyTo(listBuffer, index);
                index += toAdd.Length;
                listBuffer[index] = 0;
            }

            return new ArraySegment<byte>(listBuffer, 0, index);
        }

        /// <summary>
        /// Handler for the Completed event of an asynchronous socket operation.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="ea">Arguments pertaining to this event.</param>
        private void IOCompleted(object sender, SocketAsyncEventArgs ea)
        {
            lock (this)
            {
                socketState = SocketState.Idle;

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
                        //this.SendMessage(
                        //    ea.Offset + ea.BytesTransferred,
                        //    outstanding);

                        SendRemainingBytes(ea.Offset, ea.BytesTransferred);
                        return;
                    }

                    // -
                    // still have more messages to send
                    // -
                    if (outgoingMessages.Count > 0)
                    {
                        TrySend();
                        return;
                    }

                    if (this.state == ConnectionState.ServiceRegistered)
                    {
                        // -
                        // Once a peer has registered as a service, we only send it
                        // match-up requests and never receive anything from it.
                        // -


                        return;
                    }

                    // -
                    // Switch to receive mode.
                    // -
                    this.bufferOffset = 0;
                    this.StartReceive(0, this.buffer.Length);
                }
                else
                {
                    // -
                    // Receive completed.
                    // -
                    if (ea.BytesTransferred == 0 ||
                        this.state == ConnectionState.ServiceRegistered)
                    {
                        // -
                        // Our peer closed the connection.  Reciprocate.
                        // -
                        this.ShutdownAndClose();
                        return;
                    }

                    // -
                    // We have three cases to deal with at this point:
                    //  1. We have a complete message from our peer.
                    //  2. We have a partial message,
                    //     (a) and our receive buffer is full.
                    //     (b) and still have room in our receive buffer.
                    // -
                    int have = ea.Offset + ea.BytesTransferred - this.bufferOffset;
                    int parsePoint = this.bufferOffset;
                    while (have != 0)
                    {
                        MessageType type;
                        byte length;

                        // -
                        // Check for special-case of a Pad1 message.
                        // -
                        type = (MessageType)this.buffer[parsePoint++];
                        have--;
                        if (type == MessageType.Pad1)
                        {
                            this.bufferOffset = parsePoint;
                            continue;
                        }

                        if (have > 0)
                        {
                            // -
                            // We could potentially have a complete message.
                            // -
                            length = this.buffer[parsePoint++];
                            have--;
                            if (have >= length)
                            {
                                // -
                                // We have a complete message (as self-described).
                                // -
                                ArraySegment<byte> data;
                                if (length == 0)
                                {
                                    data = new ArraySegment<byte>(this.buffer, 0, 0);
                                }
                                else
                                {
                                    data = new ArraySegment<byte>(
                                        this.buffer,
                                        parsePoint,
                                        length);
                                    parsePoint += length;
                                    have -= length;
                                }

                                if (this.HandleMessage(type, data))
                                {
                                    // -
                                    // We've switched out of receive mode.
                                    // Note: If 'have' is non-zero at this point,
                                    // our peer has violated the protocol.
                                    // -
                                    if (have != 0)
                                    {
                                        this.ShutdownAndClose();
                                    }

                                    return;
                                }

                                // -
                                // Still in receive mode, but handled a message.
                                // -
                                this.bufferOffset = parsePoint;
                                continue;
                            }
                        }

                        // -
                        // We have a partial message.
                        // -
                        if (ea.Count == ea.BytesTransferred)
                        {
                            // -
                            // Our receive buffer is full.  Shift the start of
                            // the current partial message down to the zero index.
                            // -
                            int partialLength = this.buffer.Length
                                - this.bufferOffset;
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
                    // We had an integral number of messages (no partial messages).
                    // We're expecting another message, so restart receive.
                    // -
                    this.bufferOffset = 0;
                    this.StartReceive(0, this.buffer.Length);
                }
            }
        }

        /// <summary>
        /// Watches to see if the remote (i.e. Home Service) end closes
        /// the connection.
        /// </summary>
        private void WatchForRemoteClose()
        {
            // -
            // We post a bogus receive that will only complete if the remote
            // end closes the connection.  Our standard completion handler
            // will just clean up in that case.
            // -
            SocketAsyncEventArgs watcherEventArgs = new SocketAsyncEventArgs();
            watcherEventArgs.Completed +=
                new EventHandler<SocketAsyncEventArgs>(this.IOCompleted);
            watcherEventArgs.SetBuffer(new byte[1], 0, 1);
            bool asynchronous = this.socket.ReceiveAsync(watcherEventArgs);
            if (!asynchronous)
            {
                this.IOCompleted(this, watcherEventArgs);
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
        /// Cleanup any state in external components that is related to
        /// this object.
        /// </summary>
        private void CleanupExternalState()
        {
            switch (this.state)
            {
                case ConnectionState.ServiceRegistered:
                    this.matchmaker.RemoveServiceRegistration(
                        this.peerIdentifier);
                    break;

#if false
                case ConnectionState.ForwardingRequested:
                    this.matchmaker.RemoveClientRegistration(this.token);
                    break;
#endif
            }
        }
    }
}
