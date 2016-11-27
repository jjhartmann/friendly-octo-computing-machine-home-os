using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Net;
using System.Net.Sockets;


namespace HomeOS.Hub.Apps.TapTap
{
    public class ProtocolFormat
    {
        public string actionType;
        public string actionValue;
        public string clientID;
        public string tagID;
        
    }
    public class TapTapEngine
    {
        // Variables
        private string mData;
        private ProtocolFormat mMsg;
        public ProtocolFormat Message
        {
            get
            {
                return mMsg;
            }
            set
            {
                mMsg = value;
            }
        }

        private XmlSerializer serializer;
        private Socket mHandler;

        

        public TapTapEngine() { }

        public TapTapEngine(Socket handler)
        {
            Console.WriteLine("TapTapEngine");
            mHandler = handler;
        }

        public bool ParseData(string data)
        {
            serializer = new XmlSerializer(typeof(ProtocolFormat));
            mData = data.Substring(0, data.IndexOf("<EOF>"));

            try
            {
                using (var reader = new StringReader(mData))
                {
                    mMsg = (ProtocolFormat)serializer.Deserialize(reader);
                }
            } catch (Exception e)
            {
                Console.WriteLine("Error occorded in ParseData: {0}", e);
            }

            //
            if (mMsg == null || (mMsg != null && mMsg.actionType == null))
            {
                Send("Error Occurred: Unable to parse command from client\n");
                return false;
            }

            Send("Success in parsingCommand \n");
            return true;
        }

        public bool Send(String data)
        {
            // Convert data into byte stream
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Send to client. 
            try
            {
                //mHandler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), mHandler);
                SocketError error;
                int byteSent =  mHandler.Send(byteData, 0, byteData.Length, 0, out error);
                Console.WriteLine("Send {0} bytes to client.", byteSent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: TaptapEngine::SEND, \n Message: {0}", e);
                return false;
            }

            return true;
        }

        public void shutDown()
        {
            mHandler.Shutdown(SocketShutdown.Both);
            mHandler.Close();
            mHandler = null;
        }

    }
}
