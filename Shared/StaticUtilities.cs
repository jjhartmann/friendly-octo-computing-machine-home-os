// -
// <copyright file="StaticUtilities.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

namespace HomeOS.Shared
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// Holds various static utility functions.
    /// </summary>
    public static class StaticUtilities
    {
        /// <summary>
        /// Creates a socket and connects it to the desired service.
        /// </summary>
        /// <param name="host">The name of the service's host.</param>
        /// <param name="port">The number of the service's port.</param>
        /// <returns>A socket connected to the supplied host and port.</returns>
        public static Socket CreateConnectedSocket(string host, int port)
        {
            IPAddress[] addresses = null;
            Socket socket = null;

            try
            {
                addresses = Dns.GetHostAddresses(host);
            }
#if DEBUG
            catch (Exception except)
            {
                Console.WriteLine(
                    "GetHostAddresses for {0} failed with error: {1}",
                    host,
                    except.Message);
#else
            catch (Exception)
            {
#endif
                return null;
            }

            foreach (IPAddress address in addresses)
            {
                socket = CreateConnectedSocket(new IPEndPoint(address, port));
                if (socket != null)
                {
                    break;
                }
            }

            return socket;
        }

        /// <summary>
        /// Creates a socket and connects it to the desired service.
        /// </summary>
        /// <param name="endpoint">The remote endpoint to connect to.</param>
        /// <returns>A socket connected to the supplied endpoint.</returns>
        public static Socket CreateConnectedSocket(EndPoint endpoint)
        {
            Socket socket = null;

            try
            {
                socket = new Socket(
                    endpoint.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);
                socket.Connect(endpoint);
            }
#if DEBUG
            catch (Exception except)
            {
                Console.WriteLine(
                    "Socket create/connect failed with error: {0}",
                    except.Message);
#else 
            catch (Exception)
            {
#endif
                return null;
            }

            return socket;
        }

        /// <summary>
        /// Sets the TCP keep-alive settings on a given socket.
        /// </summary>
        /// <param name="socket">The socket in question.</param>
        /// <param name="timeout">The keep-alive timeout (in ms).</param>
        /// <param name="interval">The keep-alive interval (in ms).</param>
        public static void SetKeepAlive(
            Socket socket,
            uint timeout,
            uint interval)
        {
            // -
            // Create a byte array containing the option settings.
            // In Winsock, this would be a "tcp_keepalive" struct:
            // three 32-bit values, the first is on/off,
            // the second is the timeout in milliseconds,
            // the third is the interval in milliseconds.
            byte[] option = new byte[3 * sizeof(uint)];
            Array.Copy(
                BitConverter.GetBytes((uint)1),
                option,
                sizeof(uint));
            Array.Copy(
                BitConverter.GetBytes(timeout),
                0,
                option,
                sizeof(uint),
                sizeof(uint));
            Array.Copy(
                BitConverter.GetBytes(interval),
                0,
                option,
                2 * sizeof(uint),
                sizeof(uint));

            // -
            // This "result" should really be an uninitialized out parameter
            // from the IOControl call, but the implementors of the Socket
            // class didn't see it that way.  So we pass in a dummy array.
            // -
            byte[] result = new byte[sizeof(uint)];

            socket.IOControl(IOControlCode.KeepAliveValues, option, result);
        }

        public static string ConvertMemoryInBytesToUnitBytes(string byteString)
        {
            double memInUnits = 0.0;
            if (!double.TryParse(byteString, out memInUnits))
                return null;

            if ((int)(memInUnits / (1024.0 * 1024.0 * 1024.0 * 1024.0 * 1024.0)) > 0) // TB
            {
                return String.Format("{0:0.##} PB", memInUnits / (1024.0 * 1024.0 * 1024.0 * 1024.0 * 1024.0));
            }
            else if ((int)(memInUnits / (1024.0 * 1024.0 * 1024.0 * 1024.0)) > 0) // TB
            {
                return String.Format("{0:0.##} TB", memInUnits / (1024.0 * 1024.0 * 1024.0 * 1024.0));
            }
            else if ((int)(memInUnits / (1024.0 * 1024.0 * 1024.0)) > 0) // GB
            {
                return String.Format("{0:0.##} GB", memInUnits / (1024.0 * 1024.0 * 1024.0));
            }
            else if ((int)(memInUnits / (1024.0 * 1024.0)) > 0) // MB
            {
                return String.Format("{0:0.##} MB", memInUnits / (1024.0 * 1024.0));
            }
            else if ((int)(memInUnits / (1024.0)) > 0)
            {
                return String.Format("{0:0.##} KB", memInUnits / (1024.0));
            }
            else
            {
                return String.Format("{0}", (int)memInUnits);
            }
        }
    }
}
