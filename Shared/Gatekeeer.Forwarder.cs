using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace HomeOS.Shared.Gatekeeper
{

    /// <summary>
    /// Represents a forwarder that bi-directionally forwards all
    /// data from one connection to another.
    /// </summary>
    public class Forwarder
    {
        /// <summary>
        /// Maintains stream buffer usage state during asynchronous read/writes
        /// </summary>
        private class StreamBufferState
        {
            public void SetBuffer(Byte[] buf, int offset, int length)
            {
                this.Buffer = buf;
                this.Offset = offset;
                this.Length = length;
            }

            public void SetBuffer(int offset, int length)
            {
                Offset = offset;
                Length = length;
            }

            public byte[] Buffer { get; private set; }
            public int Offset { get; private set; }
            public int Length { get; private set; }
        }


        /// <summary>
        /// External handler for close event.
        /// </summary>
        private ForwarderCloseHandler closeHandler;

        /// <summary>
        /// The streams representing the local end of each connection.
        /// </summary>
        private Stream[] streams;

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
        /// Helper for diagnostic tracing. Supports both standalone as well as webrole in azure.
        /// </summary>
        private DiagnosticsHelper wd;

        /// <summary>
        /// Initializes a new instance of the Forwarder class.
        /// </summary>
        /// <param name="a">The stream for one connection.</param>
        /// <param name="b">The stream for the other connection.</param>
        /// <param name="closeHandler">
        /// An optional close callback handler.
        /// </param>
        public Forwarder(Stream a, Stream b, ForwarderCloseHandler closeHandler, DiagnosticsHelper webRoleDiagnostics)
        {
            if (null == webRoleDiagnostics)
                this.wd = new StandaloneDiagnosticHost();
            else
                this.wd = webRoleDiagnostics;

#region ENTER
using (AutoEnterExitTrace aeet = new AutoEnterExitTrace(wd, wd.WebTrace, "Forwarder_ctor()"))
{
#endregion ENTER
            this.streams = new Stream[2] { a, b };
            this.closeHandler = closeHandler;
            this.halfOpen = false;
            this.closing = 0;
            this.directions = new PerDirection[2];
            this.directions[0] = new PerDirection(this, a, b, this.wd);
            this.directions[1] = new PerDirection(this, b, a, this.wd);
#region LEAVE
}
#endregion LEAVE
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
#region ENTER
using (AutoEnterExitTrace aeet = new AutoEnterExitTrace(wd, wd.WebTrace, "Forwarder_Close()"))
{
#endregion ENTER
            if (Interlocked.Exchange(ref this.closing, 1) == 1)
            {
                // -
                // Already closing.
                // -
                return;
            }

            foreach (Stream stream in this.streams)
            {
                try
                {
                    stream.Flush();
                }
                catch
                {
                    // -
                    // Ignore any Shutdown exceptions.
                    // -
                }

                stream.Close();
            }

            if (this.closeHandler != null)
            {
                this.closeHandler(this);
            }
#region LEAVE
}
#endregion LEAVE
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
            /// The streams representing the local end of each connection.
            /// </summary>
            private Stream inbound, outbound;

            /// <summary>
            /// The buffer used to hold data being forwarded in this direction.
            /// </summary>
            private byte[] buffer;

            /// <summary>
            /// Keeps the stream buffer state information.
            /// </summary>
            private StreamBufferState streamBufState;

            /// <summary>
            /// Helper for tracing / diagnostics. Supports both standalone and azure webroles.
            /// </summary>
            private DiagnosticsHelper wd;

            /// <summary>
            /// Initializes a new instance of the PerDirection class.
            /// </summary>
            /// <param name="forwarder">
            /// The forwarder object to which this instance belongs.
            /// </param>
            /// <param name="from">The connection to read from.</param>
            /// <param name="to">The connection to write to.</param>
            public PerDirection(Forwarder forwarder, Stream from, Stream to, DiagnosticsHelper wd)
            {
                this.wd = wd;
#region ENTER
using (AutoEnterExitTrace aeet = new AutoEnterExitTrace(wd, wd.WebTrace, "Forwarder_PerDirection_ctor()"))
{
#endregion ENTER

                aeet.WriteDiagnosticInfo(System.Diagnostics.TraceEventType.Information, TraceEventID.traceFlow, "PerDirection Forwarder object: {0} was created for streams:  from:{1} to:{2}", this.GetHashCode(), from.GetHashCode(), to.GetHashCode());
                
                this.forwarder = forwarder;
                this.inbound = from;
                this.outbound = to;
                this.buffer = new byte[1500];
                this.streamBufState = new StreamBufferState();
                this.streamBufState.SetBuffer(this.buffer, 0, this.buffer.Length);

                // -
                // Start things going by issuing a receive on the inbound side.
                // -
                this.StartReceive();
#region LEAVE
}
#endregion LEAVE
            }

            /// <summary>
            /// Starts a receive operation on our inbound stream.
            /// </summary>
            private void StartReceive()
            {
#region ENTER
using (AutoEnterExitTrace aeet = new AutoEnterExitTrace(wd, wd.WebTrace, "Forwarder_PerDirection_StartReceive()"))
{
#endregion ENTER    
                this.streamBufState.SetBuffer(this.buffer, 0, this.buffer.Length);
                IAsyncResult result = null;
                try
                {
                    result = this.inbound.BeginRead(this.streamBufState.Buffer, streamBufState.Offset, streamBufState.Length, this.ReadAsyncCallback, null);
                }
                catch (Exception e)
                {
                    // -
                    // If something failed, close the connection.
                    // -
                    aeet.WriteDiagnosticInfo(System.Diagnostics.TraceEventType.Error, TraceEventID.traceException, "BeginRead threw an exception:{0}", e.Message);
                    this.HandleReceiveError();
                    return;
                }
#region LEAVE
}
#endregion LEAVE
            }

        /// <summary>
        /// Handler for the callback for the asynchronous Stream Read operation. 
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="ea">Arguments pertaining to this event.</param>
            private void ReadAsyncCallback(IAsyncResult result)
            {
#region ENTER
using (AutoEnterExitTrace aeet = new AutoEnterExitTrace(wd, wd.WebTrace, "Forwarder_PerDirection_ReadAsyncCallback"))
{
#endregion ENTER
                int BytesTransferred = 0;
                try
                {
                    BytesTransferred = this.inbound.EndRead(result);
                }
                catch (Exception e)
                {
                    aeet.WriteDiagnosticInfo(System.Diagnostics.TraceEventType.Error, TraceEventID.traceException, "EndRead threw an exception:{0}", e.Message);
                    this.HandleReceiveError();
                    return;
                }
                if (BytesTransferred == 0)
                {
                    // -
                    // Our inbound side quit sending.
                    // -
                    this.HandleReceiveError();
                    return;
                }

                // -
                // Switch to send mode and forward the data we received.
                // -
                this.StartSend(0, BytesTransferred);
#region LEAVE
}
#endregion LEAVE
            }

            /// <summary>
            /// Starts a send operation on our outbound stream.
            /// </summary>
            /// <param name="offset">
            /// The offset into the buffer from which to start sending.
            /// </param>
            /// <param name="count">
            /// The amount (in bytes) of data to send.
            /// </param>
            private void StartSend(int offset, int count)
            {
#region ENTER
using (AutoEnterExitTrace aeet = new AutoEnterExitTrace(wd, wd.WebTrace, "Forwarder_PerDirection_StartSend()"))
{
#endregion ENTER
                IAsyncResult result = null;
                this.streamBufState.SetBuffer(offset, count);
                try
                {
                    result = this.outbound.BeginWrite(this.streamBufState.Buffer, this.streamBufState.Offset, this.streamBufState.Length, this.WriteAsyncCallback, null);
                }
                catch (Exception e)
                {
                    aeet.WriteDiagnosticInfo(System.Diagnostics.TraceEventType.Error, TraceEventID.traceException, "BeginWrite threw an exception:{0}", e.Message);
                    this.HandleSendError(SocketError.SocketError);
                    return;
                }
#region LEAVE
}
#endregion LEAVE
            }

            /// <summary>
            /// Handler for the callback for the asynchronous Stream Write operation.
            /// </summary>
            /// <param name="result"></param>
            private void WriteAsyncCallback(IAsyncResult result)
            {
#region ENTER
using (AutoEnterExitTrace aeet = new AutoEnterExitTrace(wd, wd.WebTrace, "ClientConnection_WriteAsyncCallback"))
{
#endregion ENTER
                try
                {
                    this.outbound.EndWrite(result);
                }
                catch (Exception e)
                {
                    // -
                    // If something failed, close the connection.
                    // -
                    aeet.WriteDiagnosticInfo(System.Diagnostics.TraceEventType.Error, TraceEventID.traceException, "EndWrite threw an exception:{0}", e.Message);
                    this.HandleSendError(SocketError.SocketError);
                    return;
                }

                // -
                // Switch to receive mode and wait for more data to forward.
                // -
                this.StartReceive();
#region LEAVE
}
#endregion LEAVE
            }


            /// <summary>
            /// Handles various receive errors.
            /// </summary>
            /// <param name="error">The error to handle.</param>
            private void HandleReceiveError()
            {
#region ENTER
using (AutoEnterExitTrace aeet = new AutoEnterExitTrace(wd, wd.WebTrace, "Forwarder_PerDirection_HandleReceiveError()"))
{
#endregion ENTER
                if (this.forwarder.halfOpen == false)
                {
                    try
                    {
                        this.outbound.Flush();
                        this.outbound.Close();
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
#region LEAVE
}
#endregion LEAVE
            }

            /// <summary>
            /// Handles various send errors.
            /// </summary>
            /// <param name="error">The error to handle.</param>
            private void HandleSendError(SocketError error)
            {
#region ENTER
using (AutoEnterExitTrace aeet = new AutoEnterExitTrace(wd, wd.WebTrace, "Forwarder_PerDirection_HandleSendError()"))
{
#endregion ENTER
                this.forwarder.Close();
#region LEAVE
}
#endregion LEAVE
            }
        }
    }
}
