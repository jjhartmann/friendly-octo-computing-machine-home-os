using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;
using HomeOS.Hub.Common.Bolt.DataStore;
using System.Drawing;
using HomeOS.Hub.Custom;

//DH: Example of using a MindWave sensor to 'control' a Z-wave switch 
//The sample code in this file works with a z-wave light switch and a mindwave device. It sends a "set" command to the switch when the mindwave sensor detects a blink 
//note: 
// * Since this is demo code, the switch is hard coded to "switch" so when you install the device you have to name it "switch" for this example to work
// * A conflict between ports was found when testing this code, the workaround is to always install the Mindwave device first before the switch. 
 
  
namespace HomeOS.Hub.Apps.MentalHouse
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
    //Nicholas Kostriken

    [System.AddIn.AddIn("HomeOS.Hub.Apps.MentalHouse")]

    public class MentalHouse :  ModuleBase
    {

        private static TimeSpan OneSecond = new TimeSpan(0, 0, 1);
        DateTime lastSet = DateTime.Now - OneSecond;

        Dictionary<VPort, SwitchInfo> registeredSwitches = new Dictionary<VPort, SwitchInfo>();
        Dictionary<string, VPort> switchFriendlyNames = new Dictionary<string, VPort>();

        internal void SetLevel(string switchFriendlyName, double level)
        {
            if (switchFriendlyNames.ContainsKey(switchFriendlyName))
            {
                VPort switchPort = switchFriendlyNames[switchFriendlyName];

                if (registeredSwitches.ContainsKey(switchPort))
                {
                    SwitchInfo switchInfo = registeredSwitches[switchPort];

                    IList<VParamType> args = new List<VParamType>();

                    //make sure that the level is between zero and 1
                    if (level < 0) level = 0;
                    if (level > 1) level = 1;

                    if (switchInfo.Type == SwitchType.Multi)
                    {
                        var retVal = Invoke(switchPort, RoleSwitchMultiLevel.Instance, RoleSwitchMultiLevel.OpSetName, new ParamType(level));

                        if (retVal != null && retVal.Count == 1 && retVal[0].Maintype() == (int)ParamType.SimpleType.error)
                        {
                            logger.Log("Error in setting level: {0}", retVal[0].Value().ToString());

                            throw new Exception(retVal[0].Value().ToString());
                        }
                    }
                    else
                    {
                        //interpret all non-zero values as ON
                        bool boolLevel = (level > 0) ? true : false;

                        var retVal = Invoke(switchPort, RoleSwitchBinary.Instance, RoleSwitchBinary.OpSetName, new ParamType(boolLevel));

                        if (retVal != null && retVal.Count == 1 && retVal[0].Maintype() == (int)ParamType.SimpleType.error)
                        {
                            logger.Log("Error in setting level: {0}", retVal[0].Value().ToString());

                            throw new Exception(retVal[0].Value().ToString());
                        }

                    }

                    lock (this)
                    {
                        this.lastSet = DateTime.Now;
                    }

                    switchInfo.Level = level;
                }
            }
            else
            {
                throw new Exception("Switch with friendly name " + switchFriendlyName + " not found");
            }
        }


        void InitSwitch(VPort switchPort, SwitchType switchType, bool isColored)
        {

            logger.Log("{0} adding switch {1} {2}", this.ToString(), switchType.ToString(), switchPort.ToString());

            SwitchInfo switchInfo = new SwitchInfo();
            switchInfo.Capability = GetCapability(switchPort, Constants.UserSystem);
            switchInfo.Level = 0;
            switchInfo.Type = switchType;

            switchInfo.IsColored = isColored;
            switchInfo.Color = Color.Black;

            registeredSwitches.Add(switchPort, switchInfo);

            string switchFriendlyName = switchPort.GetInfo().GetFriendlyName();
            switchFriendlyNames.Add(switchFriendlyName, switchPort);

            if (switchInfo.Capability != null)
            {
                IList<VParamType> retVals;

                if (switchType == SwitchType.Multi)
                {
                    retVals = switchPort.Invoke(RoleSwitchMultiLevel.RoleName, RoleSwitchMultiLevel.OpGetName, null,
                    ControlPort, switchInfo.Capability, ControlPortCapability);

                    switchPort.Subscribe(RoleSwitchMultiLevel.RoleName, RoleSwitchMultiLevel.OpGetName, ControlPort, switchInfo.Capability, ControlPortCapability);

                    if (retVals[0].Maintype() < 0)
                    {
                        logger.Log("SwitchController could not get current level for {0}", switchFriendlyName);
                    }
                    else
                    {
                        switchInfo.Level = (double)retVals[0].Value();
                    }
                }
                else
                {
                    retVals = switchPort.Invoke(RoleSwitchBinary.RoleName, RoleSwitchBinary.OpGetName, null,
                    ControlPort, switchInfo.Capability, ControlPortCapability);

                    switchPort.Subscribe(RoleSwitchBinary.RoleName, RoleSwitchBinary.OpGetName, ControlPort, switchInfo.Capability, ControlPortCapability);

                    if (retVals[0].Maintype() < 0)
                    {
                        logger.Log("SwitchController could not get current level for {0}", switchFriendlyName);
                    }
                    else
                    {
                        bool boolLevel = (bool)retVals[0].Value();
                        switchInfo.Level = (boolLevel) ? 1 : 0;
                    }
                }

                 
            }
        }

        internal double GetLevel(string switchFriendlyName)
        {
            if (switchFriendlyNames.ContainsKey(switchFriendlyName))
                return registeredSwitches[switchFriendlyNames[switchFriendlyName]].Level;

            return 0;
        }


        internal List<string> GetAllSwitches()
        {
            List<string> retList = new List<string>();

            foreach (string friendlyName in switchFriendlyNames.Keys)
            {
                VPort switchPort = switchFriendlyNames[friendlyName];
                SwitchInfo switchInfo = registeredSwitches[switchPort];

                retList.Add(friendlyName);
                retList.Add(switchPort.GetInfo().GetLocation().ToString());
                retList.Add(switchInfo.Type.ToString());
                retList.Add(switchInfo.Level.ToString());

                retList.Add(switchInfo.IsColored.ToString());
                retList.Add(switchInfo.Color.R.ToString());
                retList.Add(switchInfo.Color.G.ToString());
                retList.Add(switchInfo.Color.B.ToString());
            }

            return retList;
        }

        // MindWave driver port
        VPort mindWavePort;

        private SafeServiceHost serviceHost;
        private WebFileServer appServer;
        List<string> receivedMessageList;
        SafeThread worker = null;
       


        void ForgetSwitch(VPort switchPort)
        {
            switchFriendlyNames.Remove(switchPort.GetInfo().GetFriendlyName());

            registeredSwitches.Remove(switchPort);

            logger.Log("{0} removed switch/light port {1}", this.ToString(), switchPort.ToString());
        }

        /// <summary>
        /// Starts the web service and initializes/registers the ports
        /// </summary>
        public override void Start()
        {
            logger.Log("Started: {0} ", ToString());

            MentalHouseService mentalService = new MentalHouseService(logger, this);
            serviceHost = new SafeServiceHost(logger,typeof(IMentalHouseContract), mentalService , this, Constants.AjaxSuffix, moduleInfo.BaseURL());
            serviceHost.Open();

            appServer = new WebFileServer(moduleInfo.BinaryDir(), moduleInfo.BaseURL(), logger);


            //Get the list of current ports from the platform
            IList<VPort> allPortsList = GetAllPortsFromPlatform();
            if (allPortsList != null)
            {
                foreach (VPort port in allPortsList)
                {
                    PortRegistered(port);
                }
            }

            this.receivedMessageList = new List<string>();

       
        }

        /// <summary>
        /// Stops the main thread and shuts down the web service
        /// </summary>
        public override void Stop()
        {
            logger.Log("MentalHouse clean up");
            if (worker != null)
                worker.Abort();
 
        }


        /////////////////////////////////////////////////////
        // MINDWAVE FUNCTIONS - INTERFACING WITH THE HEADSET
        /////////////////////////////////////////////////////

        /// <summary>
        /// Gets the quality of the headset connection strength
        /// </summary>
        /// <returns>The quality of the headset connection</returns>
        public int GetConnection()
        {
            int rcvdNum = 0;
            var retVals = Invoke(mindWavePort, RoleMindWave.Instance, RoleMindWave.OpGetConnection);

            if (retVals[0].Maintype() != (int)ParamType.SimpleType.error)
            {
                rcvdNum = (int)retVals[0].Value();
            }

            return rcvdNum;
        }

        /// <summary>
        /// Gets the value of the current attention level
        /// </summary>
        /// <returns>The current attention level</returns>
        public int GetAttention()
        {
            int rcvdNum = 0;
            var retVals = Invoke(mindWavePort, RoleMindWave.Instance, RoleMindWave.OpGetAttention);

            if (retVals[0].Maintype() != (int)ParamType.SimpleType.error)
            {
                rcvdNum = (int)retVals[0].Value();
            }

            return rcvdNum;
        }

        /// <summary>
        /// Gets the value of the current meditation level
        /// </summary>
        /// <returns>The current meditation level</returns>
        public int GetMeditation()
        {
            int rcvdNum = 0;
            var retVals = Invoke(mindWavePort, RoleMindWave.Instance, RoleMindWave.OpGetMeditation);

            if (retVals[0].Maintype() != (int)ParamType.SimpleType.error)
            {
                rcvdNum = (int)retVals[0].Value();
            }

            return rcvdNum;
        }
        
        /// <summary>
        /// Gets a list of the current brainwave level values
        /// [delta, theta, lowAlpha, highAlpha, lowBeta, highBeta, lowGamma, highGamma]
        /// </summary>
        /// <returns>A list of the current brainwave values</returns>
        public List<int> GetWaves()
        {
            List<int> rcvdLst = new List<int>();
            var retVals = Invoke(mindWavePort, RoleMindWave.Instance, RoleMindWave.OpGetWaves);

            if (retVals[0].Maintype() != (int)ParamType.SimpleType.error)
            {
                for (int i = 0; i < retVals.Count; i++)
                    rcvdLst.Add( (int)retVals[i].Value() );
            }

            return rcvdLst;
        }

        /// <summary>
        /// Gets the strength of any blink since the last check. 0=none.
        /// </summary>
        /// <returns>The strength of the most recent blink</returns>
        public int GetBlink()
        {
            int rcvdNum = 0;
            var retVals = Invoke(mindWavePort, RoleMindWave.Instance, RoleMindWave.OpGetBlinks);

            if (retVals[0].Maintype() != (int)ParamType.SimpleType.error)
            {
                rcvdNum = (int)retVals[0].Value();


                if (rcvdNum > 55)
                {
                    double currentLevel = GetLevel("switch");
                    if (currentLevel > 0)
                    { SetLevel("switch", 0); }

                    else
                    {
                        SetLevel("switch", 1);
                    }

                }
 


            }
            return rcvdNum;
        }


        public override void OnNotification(string roleName, string opName, IList<VParamType> retVals, VPort senderPort)
        {
            //...

            if (opName == RoleSwitchBinary.OpGetName)
                        {
                            if (retVals.Count >= 1 && retVals[0].Value() != null)
                            {
                                bool level = (bool)retVals[0].Value();

                                registeredSwitches[senderPort].Level = (level)? 1 : 0;
                            }
                            else
                            {
                                logger.Log("{0} got bad result for getlevel subscription from {1}", this.ToString(), senderPort.ToString());
                            }
                        }
        }

        /// <summary>
        /// Called when a new port is registered with the platform
        /// </summary>
        public override void PortRegistered(VPort port)
        {

            lock (this)
            {
                //Add mindwave device...
                if (Role.ContainsRole(port, RoleMindWave.RoleName) && GetCapabilityFromPlatform(port) != null)
                    mindWavePort = port;



                if (Role.ContainsRole(port, RoleSwitchMultiLevel.RoleName) ||
                    Role.ContainsRole(port, RoleSwitchBinary.RoleName) ||
                    Role.ContainsRole(port, RoleLightColor.RoleName))
                {
                    if (!registeredSwitches.ContainsKey(port) &&
                        GetCapabilityFromPlatform(port) != null)
                    {
                        var switchType = (Role.ContainsRole(port, RoleSwitchMultiLevel.RoleName)) ? SwitchType.Multi : SwitchType.Binary;

                        bool colored = Role.ContainsRole(port, RoleLightColor.RoleName);

                        InitSwitch(port, switchType, colored);
                    }


                }


            }
        }

        /// <summary>
        /// Deregisters a port with the platform
        /// </summary>
        /// <param name="port">The port to deregister</param>
        public override void PortDeregistered(VPort port)
        {
            lock (this)
            {
                if (port == mindWavePort)
                    mindWavePort = null;
            }
        }


    }


}