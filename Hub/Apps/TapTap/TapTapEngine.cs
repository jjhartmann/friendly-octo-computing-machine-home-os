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
        private StateObject mState;

        

        public TapTapEngine() { }

        public TapTapEngine(StateObject state)
        {
            Console.WriteLine("TapTapEngine");
            mState = state;
        }

        ~TapTapEngine()
        {
            if (mState != null)
            {
                mState.sslStream.Close();
                mState.workClient.Close();
                mState = null;
            }
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
                SendDebug("Error Occurred: Unable to parse command from client\n");
                return false;
            }

            SendDebug("Success in parsingCommand \n");
            return true;
        }


        public bool SendFormatedClientResponse(string friendlyName, string state, string status)
        {
            string payload = "<ReturnFormat><friendlyName>" + friendlyName + "</friendlyName>" +
                                "<state>" + state + "</state>" +
                                "<status>" + status + "</status>" +
                            "</ReturnFormat>";

            return Send(payload); 
        }



        // Debug Send
        public bool SendDebug(string data)
        {
            if (TapTapConstants.DEBUG_CLIENT_SERVER)
            {
                return Send(data);
            }

            return false;
        }

        private bool Send(String data)
        {
            // Convert data into byte stream
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Send to client. 
            try
            {
                SocketError error;
                //int byteSent =  mState.sslStream.Write(byteData, 0, byteData.Length, 0, out error);
                mState.sslStream.Write(byteData, 0, byteData.Length);
                Console.WriteLine("Send {0} bytes to client.", byteData.Length);
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
            mState.sslStream.Close();
            mState.workClient.Close();
            mState = null;
        }

    }
}
