﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace HomeOS.Hub.Apps.TapTap
{
    class TapTapServer
    {
    }

    public class StateObject
    {
        // The Clients Socket
        public Socket workSocket = null;

        // Buffer size
        public const int bufferSize = 1024;

        // Receiving Buffer
        public byte[] buffer = new Byte[bufferSize];

        // Data
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousSocketListener
    {
        // Notifies threads when event occors
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        // Constructor
        public AsynchronousSocketListener()
        {

        }


        public static void StartListening()
        {
            // Data buffer
            byte[] bytes = new Byte[1024];

            // Init local endpoint socket
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddres = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddres, 11003);


            // Create TCP/IP Socket
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Set up listener
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while(true)
                {
                    // Set thread event 
                    allDone.Reset();

                    // Start async listener
                    Console.WriteLine("Pending Connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallBack), listener);

                    // Thread Wait
                    allDone.WaitOne();            
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }




        }


        // Call Back functions
        public static void AcceptCallBack(IAsyncResult ar)
        {
            // Signal Main thread to continue
            allDone.Set();

            // Get socket handles
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create State Object and handle recieve
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallBack), state);

        }

        // Handle the connection to the server
        public static void ReadCallBack(IAsyncResult ar)
        {
            StateObject state = (StateObject) ar.AsyncState;
            Socket handler = state.workSocket;

            Console.WriteLine("CHecking Read Callback");

            String data = String.Empty;
            int bytesRecieved = handler.EndReceive(ar);

            if (bytesRecieved > 0) {

                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRecieved));

                data = state.sb.ToString();
                if (data.IndexOf("<EOF>") > -1)
                {
                    Console.WriteLine("Read {0} Bytes. \nData: {1}", data.Length, data);
                    Send(handler, data);
                }
                else
                {
                    handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallBack), state);
                }

            }
        }


        private static void Send(Socket handler, String data)
        {
            // Convert data into byte stream
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Send to client. 
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);

        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Get handler for socket
                Socket handler = (Socket)ar.AsyncState;

                // Send data
                int byteSent = handler.EndSend(ar);
                Console.WriteLine("Send {0} bytes to client.", byteSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


    }

}
