﻿using System;
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
        public const string CONFIG_NAME = "TapTapConfig";
        public string mPath;
        public string mFile;

        // Devices (phones) <GUIID, GivenName>
        private Dictionary<string, string> mDevices = new Dictionary<string, string>();

        // ThingsRev (objects> <NFCTag, FriendlyName>
        private Dictionary<string, string> mThingsRev = new Dictionary<string, string>();

        // Things (objects) <FriendlyName, NFCTag>
        private Dictionary<string, string> mThings = new Dictionary<string, string>();

        // Authentication: <GUID, List_of_things_access>
        private Dictionary<string, HashSet<string>> mDeviceAuth = new Dictionary<string, HashSet<string>>();

        
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
        public Dictionary<string, HashSet<string>> DeviceAuth
        {
            get { return mDeviceAuth; }
            set { mDeviceAuth = value; }
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
            xml += "</Things><DeviceAuth>";

            foreach (KeyValuePair<string, HashSet<string>> e in mDeviceAuth)
            {
                xml += "<Auth><DeviceID>" + e.Key + "</DeviceID><ThingsList>";
                foreach (string th in e.Value) {
                    xml += "<Thing>" + th + "</Thing>";
                }

                xml += "</ThingsList></Auth>";
            }

            xml += "</DeviceAuth></TapTapConfig>";
            return xml;

        }


        // Add device to the config file
        public bool AddDevice(string id, string name)
        {
            try
            {
                mDevices[id] = name;

                // Assign things by default
                mDeviceAuth[id] = new HashSet<string>();
                foreach (KeyValuePair<string, string> e in mThings)
                {
                    mDeviceAuth[id].Add(e.Key);
                }


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
        public bool AddThing(string friendlyNameID, string nfctag)
        {
            try
            {
                mThings[friendlyNameID] = nfctag;
                mThingsRev[nfctag] = friendlyNameID;

                // Default behavior is to add thing permission to all devices
                foreach (KeyValuePair<string, string> e in mDevices)
                {
                    if (!mDeviceAuth.ContainsKey(e.Key))
                    {
                        HashSet<string> list = new HashSet<string>();
                        list.Add(friendlyNameID);
                        mDeviceAuth[e.Key] = list;
                    }
                    else
                    {
                        mDeviceAuth[e.Key].Add(friendlyNameID);
                    }
                }

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
                mThingsRev.Remove(mThings[id]);
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

            return TapTapConstants.STR_NULL;
        }


        public string GetThingNFCTag(string friendlyname)
        {
            if (mThings.ContainsKey(friendlyname))
            {
                return mThings[friendlyname];
            }

            return TapTapConstants.STR_NULL;
        }


        public bool VerifyDeviceAuthentication(string dName, string nfctag)
        {
            if (mDevices.ContainsKey(dName) && VerifyNFCTag(nfctag)) {
                HashSet<string> thingList = mDeviceAuth[dName];
                string thingName = mThingsRev[nfctag];
                return thingList.Contains(thingName);
            }

            return false;
        }

        public bool VerifyNFCTag(string tag)
        {
            return mThingsRev.ContainsKey(tag);
        }

        public bool AddDeviceAuth(string deviceName, string thingName)
        {
            try
            {
                if (mDeviceAuth.ContainsKey(deviceName))
                {
                    mDeviceAuth[deviceName].Add(thingName);
                }
                else
                {
                    HashSet<string> list = new HashSet<string>();
                    list.Add(thingName);
                    mDeviceAuth[deviceName] = list;

                }
                WriteToDisk();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in creating Device entry: {0}", e);

            }

            return false;
        }

        public bool RemoveDeviceAuth(string deviceName, string thingName)
        {
            if (!mDeviceAuth.ContainsKey(deviceName)) {
                return false;
            }

            try
            { 
                mDeviceAuth[deviceName].Remove(thingName);
                WriteToDisk();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in creating Device entry: {0}", e);

            }

            return false;
        }


        public void WriteToDisk()
        {
            // Write in separate thread
            TapTapConfig temp =  (TapTapConfig) this.MemberwiseClone();
            SafeThread w = new SafeThread(delegate {
                TapTapParser parser = new TapTapParser(mPath, mFile, CONFIG_NAME);
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
        private string mDeviceId = TapTapConstants.STR_NULL;
        private string mDevicePassPharse = TapTapConstants.STR_NULL;
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
                mEngine.SendDebug("Device Verify Success");
                mEngine.SendFormatedClientResponse(mDeviceId, "1", "1");
                return true;
            }
            mEngine.SendFormatedClientResponse(mDeviceId, "1", "0");
            mEngine.SendDebug("Device Verify Failed");
            return false;
        }

        public void AddEngine(TapTapEngine engine)
        {
            mEngine = engine;
        }


        public void Dispose()
        {
            mEngine.shutDown();
            mDeviceId = TapTapConstants.STR_NULL;
            mDevicePassPharse = TapTapConstants.STR_NULL;
        }


    }


    // TODO: Change the UNo into own class
    class ArduinoUno
    {


    }

    // TOOD: Change the swtich into own class.
    class BinarySwitch
    {

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

        // Swtich varibales
        Dictionary<VPort, SwitchInfo> switchRegistered = new Dictionary<VPort, SwitchInfo>();
        Dictionary<string, VPort> switchFriendlyName = new Dictionary<string, VPort>();
        Dictionary<VPort, string> switchFriendlyNameRev = new Dictionary<VPort, string>();

        // Android UNO Variables
        Dictionary<VPort, string> androidUnoRegistered = new Dictionary<VPort, string>();
        Dictionary<string, VPort> androidUnoFriendlyName = new Dictionary<string, VPort>();

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
            string taptapConfigDirector = moduleInfo.WorkingDir() + TapTapConstants.CONFIG_DIR;
            string taptapCertificate = moduleInfo.WorkingDir() + TapTapConstants.CERT_DIR_FILE;


            // Parser
            TapTapParser parser = new TapTapParser(taptapConfigDirector, TapTapConstants.CONFIG_FILE, TapTapConfig.CONFIG_NAME);

            config = parser.GenObject<TapTapConfig>();
            config.mPath = taptapConfigDirector;
            config.mFile = TapTapConstants.CONFIG_FILE;




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
            engine.SendDebug("Inside Taptap main!! \n");

            if (engine.Message.actionType == "")
            {
                engine.SendDebug("Error: no action value:");
                engine.shutDown();
                return;
            }

            // Check new Add Device Request
            if (engine.Message.actionType == ENUM_MESSAGE_TYPE.ACTION_ADD_DEVICE_REQUEST)
            {
                // Get device request. 
                deviceRequest = new TapTapDeviceRequest(engine.Message.clientID, engine.Message.actionValue);
                deviceRequest.AddEngine(engine);
                return;
            }

            // Check if device is valid. 
            if (!config.VerifyDeviceAuthentication(engine.Message.clientID, engine.Message.tagID))
            {
                engine.SendDebug("Device not Validated\n");
                engine.shutDown();
                return;
            }


            // Route Messages
            switch (engine.Message.actionType)
            {
                case ENUM_MESSAGE_TYPE.ACTION_BINARY_SWITCH:
                    // Process the switch and turn device on or off. 
                    VerifyBinarySwitch(engine);
                    break;

                case ENUM_MESSAGE_TYPE.ACTION_GET_INFO:
                    // get smart device status
                    GetInformationAction(engine);
                    break;

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
                accessibleTapTapPorts.Add(port);
                if (Role.ContainsRole(port, RoleSwitchBinary.RoleName))
                {
                    if (!switchRegistered.ContainsKey(port) && GetCapabilityFromPlatform(port) != null)
                    {
                        var switchType = Role.ContainsRole(port, RoleSwitchMultiLevel.RoleName) ? SwitchType.Multi : SwitchType.Binary;
                        bool colored = Role.ContainsRole(port, RoleLightColor.RoleName);

                        InitSwitch(port, switchType, colored);

                    }
                    else
                    {
                        // Rename port with new friendlyname
                        string newfriendlyname = port.GetInfo().GetFriendlyName();
                        string oldFriendlyName = switchFriendlyNameRev[port];
                        switchFriendlyNameRev.Remove(port);
                        switchFriendlyName.Remove(oldFriendlyName);

                        // TODO: Create a bi-directional hashtable for port <==> name lookup
                        switchFriendlyName[newfriendlyname] = port;
                        switchFriendlyNameRev[port] = newfriendlyname;

                        // Modify the config folder.
                        string nfctag = config.GetThingNFCTag(oldFriendlyName);
                        config.AddThing(newfriendlyname, nfctag);

                    }
                }
                else if (Role.ContainsRole(port, RoleArduinoUno.RoleName))
                {
                    if (!androidUnoRegistered.ContainsKey(port) && GetCapabilityFromPlatform(port) != null)
                    {
                        // Init the port
                        InitArduinoUno(port);
                    }
                    else
                    {
                        // Rename port with new friendlyname
                        string newfriendlyname = port.GetInfo().GetFriendlyName();
                        string oldFriendlyName = androidUnoRegistered[port];
                        androidUnoRegistered.Remove(port);
                        androidUnoFriendlyName.Remove(oldFriendlyName);

                        // TODO: Create a bi-directional hashtable for port <==> name lookup
                        androidUnoFriendlyName[newfriendlyname] = port;
                        androidUnoRegistered[port] = newfriendlyname;

                        // Modify the config folder.
                        string nfctag = config.GetThingNFCTag(oldFriendlyName);
                        config.AddThing(newfriendlyname, nfctag);

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
        // Get Info

        void GetInformationAction(TapTapEngine engine) {
            // Determine INformation to send
            // TODO: Fix this.
            engine.SendFormatedClientResponse(engine.Message.tagID, "1", "1");

        }



        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Arduino UNO

        void InitArduinoUno(VPort port)
        {
            string friendlyname = port.GetInfo().GetFriendlyName();
            androidUnoRegistered.Add(port, friendlyname);
            androidUnoFriendlyName.Add(friendlyname, port);
            
            logger.Log("{0} added port {1}", this.ToString(), port.ToString());

            if (Subscribe(port, RoleArduinoUno.Instance, RoleArduinoUno.OpEchoSubName))
                logger.Log("{0} subscribed to port {1}", this.ToString(), port.ToString());
            else
                logger.Log("failed to subscribe to port {1}", this.ToString(), port.ToString());
        }


        bool InvokeUno(string friendlyName, int level)
        {
            VPort port = androidUnoFriendlyName[friendlyName];
            try
            {
                DateTime requestTime = DateTime.Now;

                var retVals = Invoke(port, RoleArduinoUno.Instance, RoleArduinoUno.OpEchoName, new ParamType(level));

                double diffMs = (DateTime.Now - requestTime).TotalMilliseconds;

                if (retVals[0].Maintype() != (int)ParamType.SimpleType.error)
                {

                    int rcvdNum = (int)retVals[0].Value();

                    logger.Log("echo success to {0} after {1} ms. sent = {2} rcvd = {3}", port.ToString(), diffMs.ToString(), level.ToString(), rcvdNum.ToString());
                    return true;
                }
                else
                {
                    logger.Log("echo failure to {0} after {1} ms. sent = {2} error = {3}", port.ToString(), diffMs.ToString(), level.ToString(), retVals[0].Value().ToString());
                }

            }
            catch (Exception e)
            {
                logger.Log("Error while calling echo request: {0}", e.ToString());
            }


            return false;
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

            // Register Switches
            switchRegistered.Add(port, sinfo);

            string friendlyName = port.GetInfo().GetFriendlyName();
            switchFriendlyName.Add(friendlyName, port);
            switchFriendlyNameRev.Add(port, friendlyName);


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
        internal bool SetLevel(string friendlyName)
        {

            // Determine the swtich'
            if (switchFriendlyName.ContainsKey(friendlyName))
            {
                VPort sport = switchFriendlyName[friendlyName];

                if (switchRegistered.First().Key != null)
                {

                    SwitchInfo sinfo = switchRegistered[sport];
                    double level = sinfo.Level;

                    IList<VParamType> args = new List<VParamType>();

                    //make sure that the level is between zero and 1
                    level = level <= 0 ? 1 : 0;
                    

                    if (sinfo.Type == SwitchType.Binary)
                    {
                        // Do opposite of current switch value
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


        private bool VerifyBinarySwitch(TapTapEngine engine)
        {
            string friendlyname = "NULL";
            if ( (friendlyname = config.GetThingFriendlyName(engine.Message.tagID)) == TapTapConstants.STR_NULL)
            {
                engine.SendDebug("Tag Not Valid \n");
                engine.shutDown();
                return false;
            }

            bool activationSuccess = false;
            if (switchFriendlyName.ContainsKey(friendlyname))
            {

                // Don't need value for binary switch.
                activationSuccess = SetLevel(friendlyname);
            } 
            else if (androidUnoFriendlyName.ContainsKey(friendlyname))
            {
                // Invoke Android device.
                int value = Int32.Parse(engine.Message.actionValue);
                activationSuccess = InvokeUno(friendlyname, value);
            }

            if (activationSuccess)
            {
                engine.SendFormatedClientResponse(friendlyname, "0", "1");
                engine.SendDebug("Success: in activating " + friendlyname + "\n");
            }
            else
            {
                engine.SendFormatedClientResponse(friendlyname, "0", "0");
                engine.SendDebug("Failure: in activating " + friendlyname + "\n");
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

            foreach (string name in androidUnoFriendlyName.Keys)
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