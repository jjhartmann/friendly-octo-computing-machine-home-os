using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace HomeOS.Hub.Apps.TapTap
{
    public class ProtocolFormat
    {
        public string actionType;
        public string actionValue;
        public string clientID;
        public int deviceID;
        
    }
    class TapTapEngine
    {
        // Variables
        private string mData;
        ProtocolFormat mMsg;
        XmlSerializer serializer;

        public TapTapEngine() { }

        public TapTapEngine(string data)
        {
            Console.WriteLine("TapTapEngine");
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
                return false;
            }

            return true;
        }



    }
}
