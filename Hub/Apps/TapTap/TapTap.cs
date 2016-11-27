using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Xml;
using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;
using HomeOS.Hub.Common.Bolt.DataStore;
using HomeOS.Hub.Platform;
using System.IO;

namespace HomeOS.Hub.Apps.TapTap
{
    public enum SwitchType { Binary, Multi };

    class SwitchInfo
    {
        public VCapability Capability { get; set; }
        public double Level { get; set; }
        public SwitchType Type { get; set; }

        public bool IsColored { get; set; }
        public Color Color { get; set; }
    }

    public interface IXMLParsable
    {
        string GetXMLString();
    }



    /// <summary>
    /// Configuration class for taptap. Integrates with XML Flat file.
    /// </summary>
    public class TapTapConfig : IXMLParsable
    {
        private VLogger logger;

        // Config Settings
        public const string mName = "TapTapConfig";
        public string mPath;
        public string mFile;

        private Dictionary<string, string> mDevices = new Dictionary<string, string>();
        private Dictionary<string, string> mThingsRev = new Dictionary<string, string>();
        private Dictionary<string, string> mThings = new Dictionary<string, string>();
        
        public Dictionary<string, string> Devices {
            get { return mDevices;  }
            set { mDevices = value; }
        }
        public Dictionary<string, string> Things {
            get { return mThings; }
            set
            {
                mThings = value;
                foreach (string key in mThings.Keys)
                {
                    mThingsRev[mThings[key]] = key;
                }
            }
        }

        /// <summary>
        /// Create a serilizable string of XML
        /// </summary>
        /// <returns>XML String</returns>
        string IXMLParsable.GetXMLString()
        {
            
            string xml = "<TapTapConfig><Devices>";

            foreach ( KeyValuePair<string, string> e in mDevices)
            {
                xml += "<Device><Id>" + e.Key + "</Id><Name>" + e.Value + "</Name></Device>";
            }
            xml += "</Devices><Things>";

            foreach (KeyValuePair<string, string> e in mThings)
            {
                xml += "<Thing><Id>" + e.Key + "</Id><NFCTag>" + e.Value + "</NFCTag></Thing>";
            }
            xml += "</Things></TapTapConfig>";


            return xml;

        }


        // Add device to the config file
        public bool AddDevice(string id, string name)
        {
            try
            {
                mDevices[id] = name;
                WriteToDisk();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in creating Device entry: {0}", e);
                
            }

            return false;
        }

        // Remove device to the Config file.
        public bool RemoveDevice(string id)
        {
            try
            {
                mDevices.Remove(id);
                WriteToDisk();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in creating Device entry: {0}", e);

            }

            return false;
        }

        // Add thing to the config file
        public bool AddThing(string id, string name)
        {
            try
            {
                mThings[id] = name;
                mThingsRev[name] = id;

                WriteToDisk();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in creating Device entry: {0}", e);

            }

            return false;
        }

        // Remove thing to the Config file.
        public bool RemoveThing(string id)
        {
            try
            {
                mThingsRev.Remove(mDevices[id]);
                mThings.Remove(id);
                WriteToDisk();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in creating Device entry: {0}", e);

            }

            return false;
        }

        public string GetThingFriendlyName(string tag)
        {
            if (mThingsRev.ContainsKey(tag))
                return mThingsRev[tag];

            return "NULL";
        }


        public bool VerifyDevice(string dName)
        {
            return mDevices.ContainsKey(dName);
        }

        public bool VerifyNFCTag(string tag)
        {
            return mThingsRev.ContainsKey(tag);
        }

        public void WriteToDisk()
        {
            TapTapConfig temp =  (TapTapConfig) this.MemberwiseClone();
            SafeThread w = new SafeThread(delegate {
                TapTapParser parser = new TapTapParser(mPath, mFile, mName);
                parser.CreateXml(temp);
            }, "taptapconfig-writetodisk", logger);
            w.Start();
            
        }
    }

    /// <summary>
    /// Simple class to set up device with Taptap server.
    /// </summary>
    class TapTapDeviceRequest
    {
        // Setting up device
        private string mDeviceId = "NULL";
        private string mDevicePassPharse = "NULL";
        private TapTapEngine mEngine = null;

        public string DeviceId { get { return mDeviceId; } set { mDeviceId = value; } }
        public string DevicePassPharse { get { return mDevicePassPharse; } set { mDevicePassPharse = value; } }


        public TapTapDeviceRequest(string indevice, string inpass)
        {
            mDeviceId = indevice;
            mDevicePassPharse = inpass;
        }

        public bool Verify(string id, string pass)
        {
            if (id == mDeviceId && pass == mDevicePassPharse)
            {
                mEngine.Send("Device Verify Success");
                return true;
            }

            mEngine.Send("Device Verify Failed");
            return false;
        }

        public void AddEngine(TapTapEngine engine)
        {
            mEngine = engine;
        }


        public void Dispose()
        {
            mEngine.shutDown();
            mDeviceId = "NULL";
            mDevicePassPharse = "NULL";
        }


    }



    /// <summary>
    /// A taptap a module that 
    /// 1. sends ping messages to all active taptap ports
    /// </summary>

    [System.AddIn.AddIn("HomeOS.Hub.Apps.TapTap")]
    public class TapTap :  ModuleBase
    {
        //list of accessible taptap ports in the system
        List<VPort> accessibleTapTapPorts;

        Dictionary<VPort, SwitchInfo> switchRegistered = new Dictionary<VPort, SwitchInfo>();
        Dictionary<string, VPort> switchFriendlyName = new Dictionary<string, VPort>();

        private SafeServiceHost serviceHost;

        private WebFileServer appServer;

        List<string> receivedMessageList;
        SafeThread worker = null;

        IStream datastream;

        // Config 
        TapTapConfig config = null;
        TapTapDeviceRequest deviceRequest = null;



        public override void Start()
        {
            logger.Log("Started: {0} ", ToString());

            TapTapService taptapService = new TapTapService(logger, this);
            serviceHost = new SafeServiceHost(logger, typeof(ITapTapContract), taptapService, this, Constants.AjaxSuffix, moduleInfo.BaseURL());
            serviceHost.Open();

            appServer = new WebFileServer(moduleInfo.BinaryDir(), moduleInfo.BaseURL(), logger);

            //// Read configuration file
            string taptapConfigDirector = moduleInfo.WorkingDir() + "\\Config";
            string taptapCertificate = moduleInfo.WorkingDir() + "\\Cert\\certificate.cer";


            // Parser
            TapTapParser parser = new TapTapParser(taptapConfigDirector, "taptapconfig.xml", "TapTapConfig");

            config = parser.GenObject<TapTapConfig>();
            config.mPath = taptapConfigDirector;
            config.mFile = "taptapconfig.xml";




            //........... instantiate the list of other ports that we are interested in
            accessibleTapTapPorts = new List<VPort>();

            //..... get the list of current ports from the platform
            IList<VPort> allPortsList = GetAllPortsFromPlatform();

            if (allPortsList != null)
                ProcessAllPortsList(allPortsList);

            this.receivedMessageList = new List<string>();

            // remoteSync flag can be set to true, if the Platform Settings has the Cloud storage
            // information i.e., DataStoreAccountName, DataStoreAccountKey values
            datastream = base.CreateValueDataStream<StrKey, StrValue>("test", false /* remoteSync */);

            worker = new SafeThread(delegate()
            {
                Work(taptapCertificate);
            }, "AppTapTap-worker", logger);
            worker.Start();
        }

        /// <summary>
        /// Call back delegate for communication with server. Will determine case based on protocol from client. 
        /// </summary>
        /// <param name="engine"></param>
        // 
        private void EngineCallback(TapTapEngine engine)
        {
            // TODO: The egnine onwner ship in conflict, who shuts it down. 
            Console.WriteLine("Parser Callback. \nData: {0}", engine.Message.actionType);
            engine.Send("Inside Taptap main!! \n");

            // Check new Add Device Request
            if (engine.Message.actionType == "addDeviceRequest")
            {
                // Get device request. 
                deviceRequest = new TapTapDeviceRequest(engine.Message.clientID, engine.Message.actionValue);
                deviceRequest.AddEngine(engine);
                return;
            }

            // Check if device is valid. 
            if (!config.VerifyDevice(engine.Message.clientID))
            {
                engine.Send("Device not Validated\n");
                engine.shutDown();
                return;
            }


            // Route Messages
            switch (engine.Message.actionType)
            {
                case "binarySwitch":
                    // Process the switch and turn device on or off. 
                    VerifySwitchInteraction(engine);
                    break;

                //case "addDeviceRequest":
                //    // Get device request. 
                //    deviceRequest = new TapTapDeviceRequest(engine.Message.clientID, engine.Message.actionValue);
                //    deviceRequest.AddEngine(engine);
                //    break;

                default:
                    break;
            }


        }

        public override void Stop()
        {
            logger.Log("AppTapTap clean up");
            lock (this) {
                if (worker != null)
                    worker.Abort();

                if (datastream != null)
                    datastream.Close();

                if (serviceHost != null)
                    serviceHost.Close();

                if (appServer != null)
                    appServer.Dispose();


            }
        }
        /// <summary>
        /// Sit in a loop and spray the Pings to all active ports
        /// </summary>
        public void Work(string serverCertivicate)
        {
            // Start the server 
            AsynchronousSocketListener.StartListening(EngineCallback, serverCertivicate);

            int counter = 0;
            while (true)
            {
                counter++;

                lock (this)
                {                    
                    foreach (VPort port in accessibleTapTapPorts)
                    {
                        SendEchoRequest(port, counter);                       
                    }
                }

                WriteToStream();
                System.Threading.Thread.Sleep(1 * 10 * 1000);
            }
        }

        public void WriteToStream()
        {
            StrKey key = new StrKey("TapTapKey");
            datastream.Append(key, new StrValue("TapTapVal"));
            logger.Log("Writing {0} to stream " , datastream.Get(key).ToString());
        }

        public void SendEchoRequest(VPort port, int counter)
        {
            try
            {
                //DateTime requestTime = DateTime.Now;

                //var retVals = Invoke(port, RoleSwitchMultiLevel.Instance, RoleSwitchMultiLevel.OpEchoName, new ParamType(counter));

                //double diffMs = (DateTime.Now - requestTime).TotalMilliseconds;

                //if (retVals[0].Maintype() != (int)ParamType.SimpleType.error)
                //{

                //    int rcvdNum = (int) retVals[0].Value();

                //    logger.Log("echo success to {0} after {1} ms. sent = {2} rcvd = {3}", port.ToString(), diffMs.ToString(), counter.ToString(), rcvdNum.ToString());
                //}
                //else
                //{
                //    logger.Log("echo failure to {0} after {1} ms. sent = {2} error = {3}", port.ToString(), diffMs.ToString(), counter.ToString(), retVals[0].Value().ToString());
                //}

            }
            catch (Exception e)
            {
                logger.Log("Error while calling echo request: {0}", e.ToString());
            }
        }

        public override void OnNotification(string roleName, string opName, IList<VParamType> retVals, VPort senderPort)
        {
            string message = "";
            lock (this)
            {
                switch (opName.ToLower())
                {
                    //case RoleSwitchMultiLevel.OpEchoSubName:
                    //    int rcvdNum = (int)retVals[0].Value();

                    //    message = String.Format("async echo response from {0}. rcvd = {1}", senderPort.ToString(), rcvdNum.ToString());
                    //    this.receivedMessageList.Add(message);
                    //    break;
                    //default:
                    //    message = String.Format("Invalid async operation return {0} from {1}", opName.ToLower(), senderPort.ToString());
                    //    break;
                }
            }
            logger.Log("{0} {1}", this.ToString(), message);

        }

        private void ProcessAllPortsList(IList<VPort> portList)
        {
            foreach (VPort port in portList)
            {
                PortRegistered(port);
            }
        }

        /// <summary>
        ///  Called when a new port is registered with the platform
        /// </summary>
        public override void PortRegistered(VPort port)
        {

            logger.Log("{0} got registeration notification for {1}", ToString(), port.ToString());

            lock (this)
            {
                if (Role.ContainsRole(port, RoleSwitchBinary.RoleName))
                {
                    if (!switchRegistered.ContainsKey(port) && GetCapabilityFromPlatform(port) != null)
                    {
                        var switchType = Role.ContainsRole(port, RoleSwitchMultiLevel.RoleName) ? SwitchType.Multi : SwitchType.Binary;
                        bool colored = Role.ContainsRole(port, RoleLightColor.RoleName);

                        InitSwitch(port, switchType, colored);

                    }
                }
            }
        }

        public override void PortDeregistered(VPort port)
        {
            lock (this)
            {
                if (accessibleTapTapPorts.Contains(port))
                {
                    accessibleTapTapPorts.Remove(port);
                    logger.Log("{0} deregistered port {1}", this.ToString(), port.GetInfo().ModuleFacingName());
                }
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Swtich Stuff
        /// <summary>
        ///  Initilize the swtich for use in application
        /// </summary>
        /// <param name="port"></param>
        /// <param name="switchType"></param>
        /// <param name="isColored"></param>
        void InitSwitch(VPort port, SwitchType switchType, bool isColored)
        {
            logger.Log("{0} adding switch {1} {2}", this.ToString(), switchType.ToString(), port.ToString());

            SwitchInfo sinfo = new SwitchInfo();
            sinfo.Capability = GetCapability(port, Constants.UserSystem);
            sinfo.Level = 0;
            sinfo.Type = switchType;

            sinfo.IsColored = isColored;
            sinfo.Color = Color.Black;

            switchRegistered.Add(port, sinfo);

            string friendlyName = port.GetInfo().GetFriendlyName();
            switchFriendlyName.Add(friendlyName, port);

            if (sinfo.Capability != null)
            {
                IList<VParamType> retVals;

                if (switchType == SwitchType.Binary)
                {
                    retVals = port.Invoke(
                        RoleSwitchBinary.RoleName, 
                        RoleSwitchBinary.OpGetName, 
                        null, 
                        ControlPort, 
                        sinfo.Capability, 
                        ControlPortCapability);

                    port.Subscribe(
                        RoleSwitchBinary.RoleName, 
                        RoleSwitchBinary.OpGetName, 
                        ControlPort, 
                        sinfo.Capability, 
                        ControlPortCapability);

                    if (retVals[0].Maintype() < 0)
                    {
                        logger.Log("SwtichBinary could not get current level for {0}", friendlyName);
                    }
                    else
                    {
                        bool blevel = (bool)retVals[0].Value();
                        sinfo.Level = blevel ? 1 : 0;
                    }


                }
            }

        }

        /// <summary>
        /// Turn on or off the switch.
        /// </summary>
        /// <param name="switchFriendlyName"></param>
        /// <param name="level"></param>
        internal bool SetLevel(string friendlyName, double level)
        {

            // Determine the swtich'
            if (switchFriendlyName.ContainsKey(friendlyName))
            {
                VPort sport = switchFriendlyName[friendlyName];

                if (switchRegistered.First().Key != null)
                {

                    SwitchInfo sinfo = switchRegistered[sport];

                    IList<VParamType> args = new List<VParamType>();

                    //make sure that the level is between zero and 1
                    if (level < 0) level = 0;
                    if (level > 1) level = 1;

                    if (sinfo.Type == SwitchType.Binary)
                    {
                        bool blevel = (level > 0) ? true : false;

                        var retVal = Invoke(sport, RoleSwitchBinary.Instance, RoleSwitchBinary.OpSetName, new ParamType(blevel));

                        if (retVal != null && retVal.Count == 1 && retVal[0].Maintype() == (int)ParamType.SimpleType.error)
                        {
                            logger.Log("Error in setting level: {0}", retVal[0].Value().ToString());

                            throw new Exception(retVal[0].Value().ToString());
                        }
                    }

                    sinfo.Level = level;

                    return true;
                }
            } 
            else
            {
                // Throw exception
               
            }

            return false;


        }


        private bool VerifySwitchInteraction(TapTapEngine engine)
        {
            string friendlySwitchname = "NULL";
            if ( (friendlySwitchname = config.GetThingFriendlyName(engine.Message.tagID)) == "NULL")
            {
                engine.Send("Tag Not Valid \n");
                engine.shutDown();
                return false;
            }

            double level = Convert.ToDouble(engine.Message.actionValue);
            if (SetLevel(friendlySwitchname, level))
            {
                 engine.Send("Success in activating Switch\n");
            }
            else
            {
                engine.Send("Failure: in activating Switch\n");
            }

            engine.shutDown();
            return true;
        }



        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // SERVICE CALLBACK FUNCTIONS

        // Get all Devices
        public Dictionary<string, string> GetAllDevices()
        {
            Dictionary<string, string> ret = new Dictionary<string, string> (config.Devices);
            return ret;
        }

        // Get all Things
        public Dictionary<string, string> GetAllThings()  
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            foreach(string name in switchFriendlyName.Keys)
            {
                if (config.Things.ContainsKey(name))
                {
                    ret.Add(name, config.Things[name]);
                }
                else
                {
                    ret.Add(name, "Insert NFC Tag");
                }
            }

            return ret;
        }


        public List<string> GetReceivedMessages()
        {
            List<string> retList = new List<string>(this.receivedMessageList);
            retList.Reverse();
            return retList;
        }

        public bool SaveDeviceName(string id, string name)
        {
            try
            {
                config.AddDevice(id, name);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in changing device name: {0}", e);
            }
            return false;
        }

        public bool SaveThingTag(string id, string tag)
        {
            try
            {
                config.AddThing(id, tag);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in changing thing name: {0}", e);
            }
            return false;
        }

        public List<string> GetDeviceRequests()
        {
            // TODO: Change the requests to a List of requests for multiple simultaneous requests
            List<string> retVal = new List<string>();
            try
            {
                if (deviceRequest != null)
                {
                    retVal.Add(deviceRequest.DeviceId);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in getting device requests: {0}", e);
            }
            return retVal;
        }

        public bool SendDeviceVerification(string id, string pass)
        {
            bool retVal = false;
            try
            {
                if (deviceRequest != null)
                {
                    if ((retVal = deviceRequest.Verify(id, pass)))
                    {
                        config.AddDevice(id, "Friendly Name for Your device");
                    }

                    deviceRequest.Dispose();
                    deviceRequest = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in getting device requests: {0}", e);
            }
            return retVal;
        }

    }
}