using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace HomeOS.Hub.Apps.TapTap
{
    class TapTapServer
    {
    }

    public class StateObject
    {
        // The Clients Socket
        public TcpClient workClient = null;

        // The SSLStream
        public SslStream sslStream = null;

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
        static X509Certificate serverCertificate = null;


        // Constructor
        public AsynchronousSocketListener()
        {

        }

        // Delclare delegate type
        public delegate void EngineDelegate(TapTapEngine engine);
        private static EngineDelegate eDelegate;

        public static void StartListening(EngineDelegate c, string certificate)
        {
            // Assign delegate
            eDelegate = c;


            serverCertificate = X509Certificate.CreateFromCertFile(certificate);

            // Data buffer
            byte[] bytes = new Byte[1024];

            // Init local endpoint socket
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddres = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddres, 11003);


            // Create TCP/IP Socket
            //Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TcpListener listener = new TcpListener(IPAddress.Any, 8080);
            listener.Start();


            // Set up listener
            try
            {
                //listener.Bind(localEndPoint);
                //listener.Listen(100);

                while(true)
                {
                    // Set thread event 
                    allDone.Reset();

                    // Start async listener
                    Console.WriteLine("Pending Connection...");
                    listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallBack), listener);
                    //listener.BeginAccept(new AsyncCallback(AcceptCallBack), listener);

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
            //Socket listener = (Socket)ar.AsyncState;
            //Socket handler = listener.EndAccept(ar);
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);


            // Create State Object and handle receive
            StateObject state = new StateObject();
            state.workClient = client;
            //handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallBack), state);

            ProcessClient(state);

        }


        static void ProcessClient(StateObject state)
        {
            // Get the SSL Stream
            SslStream sslStream = new SslStream(state.workClient.GetStream(), false);

            try
            {
                // Authenticate Server
                sslStream.AuthenticateAsServer(serverCertificate, false, SslProtocols.Tls12, true);

                // Set SslStream
                state.sslStream = sslStream;
                sslStream.BeginRead(state.buffer, 0, StateObject.bufferSize, new AsyncCallback(ReadCallBack), state);
            }
            catch(AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                sslStream.Close();
                state.workClient.Close();
                return;
            }
        }

        // Handle the connection to the server
        public static void ReadCallBack(IAsyncResult ar)
        {
            StateObject state = (StateObject) ar.AsyncState;
            SslStream stream = state.sslStream;

            Console.WriteLine("CHecking Read Callback");

            String data = String.Empty;
            int bytesRecieved = stream.EndRead(ar);

            if (bytesRecieved > 0) {

                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRecieved));

                data = state.sb.ToString();
                if (data.IndexOf("<EOF>") > -1)
                {
                    Console.WriteLine("Read {0} Bytes. \nData: {1}", data.Length, data);

                    // Call engine to process request. 
                    TapTapEngine engine = new TapTapEngine(state);
                    if (engine.ParseData(data)) {
                        eDelegate(engine);
                    }
                    else
                    {
                        Console.WriteLine("Error in Parsing data. Shut down stream");
                        state.sslStream.Close();
                        state.workClient.Close();
                    }
                }
                else
                {
                    stream.BeginRead(state.buffer, 0, StateObject.bufferSize, new AsyncCallback(ReadCallBack), state);
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
