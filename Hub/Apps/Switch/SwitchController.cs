using System;
using System.Collections.Generic;
using System.Threading;
using System.AddIn;
using HomeOS.Hub.Common;
using System.ServiceModel;
using HomeOS.Hub.Platform.Views;

namespace HomeOS.Hub.Apps.Switch
{
    public enum SwitchType { Multi, Binary };

    class SwitchInfo
    {
        public VCapability Capability { get; set; }
        public byte Level { get; set; }
        public SwitchType Type { get; set; }
    }

    [AddIn("HomeOS.Hub.Apps.Switch")]
    public class SwitchMultiLevelController : Common.ModuleBase
    {
        private static TimeSpan OneSecond = new TimeSpan(0, 0, 1);
        DateTime lastSet = DateTime.Now - OneSecond;

        Dictionary<VPort, SwitchInfo> registeredSwitches = new Dictionary<VPort, SwitchInfo>();
        Dictionary<string, VPort> switchFriendlyNames = new Dictionary<string, VPort>();

        private SafeServiceHost serviceHost;
        private WebFileServer appServer;

        public override void Start()
        {
            logger.Log("Started: {0}", ToString());

            SwitchSvc service = new SwitchSvc(logger, this);
            serviceHost = new SafeServiceHost(logger, typeof(ISwitchSvcContract), service, this, Constants.AjaxSuffix, moduleInfo.BaseURL());
            serviceHost.Open();

            appServer = new WebFileServer(moduleInfo.BinaryDir(), moduleInfo.BaseURL(), logger);

            logger.Log("simplex camera service is open for business at " + moduleInfo.BaseURL());

            //..... get the list of current ports from the platform
            IList<VPort> allPortsList = GetAllPortsFromPlatform();
            foreach (VPort port in allPortsList)
            {
                PortRegistered(port);
            }
        }

        internal byte GetLevel(string switchFriendlyName)
        {
            if (switchFriendlyNames.ContainsKey(switchFriendlyName))
                return registeredSwitches[switchFriendlyNames[switchFriendlyName]].Level;

            return 0;
        }

        public override void OnNotification(string roleName, string opName, IList<VParamType> retVals, VPort senderPort)
        {
            lock (this)
            {
                if (registeredSwitches.ContainsKey(senderPort))
                {
                    if (RoleSwitchMultiLevel.OpGetName.Equals(opName, StringComparison.CurrentCultureIgnoreCase) &&
                        retVals.Count >= 1 && retVals[0].Value() != null)
                    {
                        byte level = (byte) (int) retVals[0].Value();

                        registeredSwitches[senderPort].Level = level;
                    }
                    else
                    {
                        logger.Log("{0} got bad result for getlevel subscription from {1}", this.ToString(), senderPort.ToString());
                    }
                }
            }
        }

        public override void Stop()
        {
            lock (this)
            {
                serviceHost.Close();

                appServer.Dispose();
            }
        }

        /// <summary>
        ///  Called when a new port is registered with the platform
        /// </summary>
        /// <param name="port"></param>
        public override void PortRegistered(VPort port)
        {
             lock (this)
            {
                if (Role.ContainsRole(port, RoleSwitchMultiLevel.RoleName) || Role.ContainsRole(port, RoleSwitchBinary.RoleName))
                {
                    if (!registeredSwitches.ContainsKey(port))
                    {
                        var switchType = (Role.ContainsRole(port, RoleSwitchMultiLevel.RoleName))? SwitchType.Multi : SwitchType.Binary;

                        InitSwitch(port, switchType);
                    }
                    else
                    {
                        //the friendly name of the port might have changed. update that.
                        string oldFriendlyName = null;

                        foreach (var pair in switchFriendlyNames)
                        {
                            if (pair.Value.Equals(port) &&
                                !pair.Key.Equals(port.GetInfo().GetFriendlyName()))
                            {
                                oldFriendlyName = pair.Key;
                                break;
                            }
                        }

                        if (oldFriendlyName != null)
                        {
                            switchFriendlyNames.Remove(oldFriendlyName);
                            switchFriendlyNames.Add(port.GetInfo().GetFriendlyName(), port);
                        }
                    }

                }
            }
        }

        void InitSwitch(VPort switchPort, SwitchType switchType) {

            logger.Log("{0} adding switch {1} {2}", this.ToString(), switchType.ToString(), switchPort.ToString());

            SwitchInfo switchInfo = new SwitchInfo();
            switchInfo.Capability = GetCapability(switchPort, Constants.UserSystem);
            switchInfo.Level = 0;
            switchInfo.Type = switchType;

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

                }
                else
                {
                    retVals = switchPort.Invoke(RoleSwitchBinary.RoleName, RoleSwitchBinary.OpGetName, null,
                    ControlPort, switchInfo.Capability, ControlPortCapability);

                    switchPort.Subscribe(RoleSwitchBinary.RoleName, RoleSwitchBinary.OpGetName, ControlPort, switchInfo.Capability, ControlPortCapability);
                }

                if (retVals[0].Maintype() < 0)
                {
                    logger.Log("SwitchController could not get current level for {0}", switchFriendlyName);
                }
                else
                {
                    switchInfo.Level = (byte) (int) (retVals[0].Value());
                }

            }
        }

        /// <summary>
        ///  Called when a new port is registered with the platform
        /// </summary>
        /// <param name="port"></param>
        public override void PortDeregistered(VPort port)
        {
            lock (this)
            {
                if (registeredSwitches.ContainsKey(port))
                    ForgetSwitch(port);
            }
        }

        void ForgetSwitch(VPort switchPort)
        {
            switchFriendlyNames.Remove(switchPort.GetInfo().GetFriendlyName());

            registeredSwitches.Remove(switchPort);

            logger.Log("{0} removed camera port {1}", this.ToString(), switchPort.ToString());
        }

        public void Log(string format, params string[] args)
        {
            logger.Log(format, args);
        }

        internal void SetLevel(string switchFriendlyName, byte level)
        {
            if (switchFriendlyNames.ContainsKey(switchFriendlyName))
            {
                VPort switchPort = switchFriendlyNames[switchFriendlyName];

                if (registeredSwitches.ContainsKey(switchPort))
                {
                    SwitchInfo switchInfo = registeredSwitches[switchPort];

                    IList<VParamType> args = new List<VParamType>();

                    if (switchInfo.Type == SwitchType.Multi)
                    {
                        //args.Add(new ParamType(level));
                        //switchPort.Invoke(RoleSwitchMultiLevel.RoleName, RoleSwitchMultiLevel.OpSetName, args,
                        //                  ControlPort, switchInfo.Capability, ControlPortCapability);

                        var retVal = Invoke(switchPort, RoleSwitchMultiLevel.Instance, RoleSwitchMultiLevel.OpSetName, new ParamType(level));

                        if (retVal!= null && retVal.Count == 1 && retVal[0].Maintype() == (int) ParamType.SimpleType.error) 
                        {
                            logger.Log("Error in setting level: {0}", retVal[0].Value().ToString());

                            throw new Exception(retVal[0].Value().ToString());
                        }
                    }
                    else
                    {
                        if (level > 0) level = 255;
                        //args.Add(new ParamType(level));
                        //switchPort.Invoke(RoleSwitchBinary.RoleName, RoleSwitchBinary.OpSetName, args,
                        //                  ControlPort, switchInfo.Capability, ControlPortCapability);

                        var retVal = Invoke(switchPort, RoleSwitchBinary.Instance, RoleSwitchBinary.OpSetName, new ParamType(level));

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

                    if (level != switchInfo.Level)
                    {
                        switchInfo.Level = level;
                    }
                }
            }
            else
            {
                throw new Exception("Switch with friendly name " + switchFriendlyName + " not found");
            }
        }

        //returns a 4-tuples for each switch: (name, location, type, level)
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
            }

            return retList;
        }

    }
}