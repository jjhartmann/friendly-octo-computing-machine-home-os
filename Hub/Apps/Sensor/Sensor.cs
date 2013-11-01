using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;
using HomeOS.Hub.Common.DataStore;

namespace HomeOS.Hub.Apps.Sensor
{

    /// <summary>
    /// A dummy a module that 
    /// 1. sends ping messages to all active dummy ports
    /// </summary>

    [System.AddIn.AddIn("HomeOS.Hub.Apps.Sensor", Version = "1.0.0.0")]
    public class Sensor : ModuleBase
    {
        //list of accessible dummy ports in the system
        List<VPort> accessibleSensorPorts;

        private SafeServiceHost serviceHost;

        private WebFileServer appServer;

        List<string> receivedMessageList;

        IStream datastream;

        public override void Start()
        {
            logger.Log("Started: {0} ", ToString());

            SensorService dummyService = new SensorService(logger, this);
            serviceHost = new SafeServiceHost(logger, typeof(ISensorContract), dummyService, this, Constants.AjaxSuffix, moduleInfo.BaseURL());
            serviceHost.Open();

            appServer = new WebFileServer(moduleInfo.BinaryDir(), moduleInfo.BaseURL(), logger);


            //........... instantiate the list of other ports that we are interested in
            accessibleSensorPorts = new List<VPort>();

            //..... get the list of current ports from the platform
            IList<VPort> allPortsList = GetAllPortsFromPlatform();

            if (allPortsList != null)
                ProcessAllPortsList(allPortsList);

            this.receivedMessageList = new List<string>();

            // remoteSync flag can be set to true, if the Platform Settings has the Cloud storage
            // information i.e., DataStoreAccountName, DataStoreAccountKey values
            datastream = base.CreateFileStream<StrKey, StrValue>("dumb", false /* remoteSync */);
        }

        public override void Stop()
        {
            logger.Log("AppSensor clean up");
            if (datastream != null)
                datastream.Close();
        }

        public void WriteToStream()
        {
            StrKey key = new StrKey("SensorKey");
            datastream.Append(key, new StrValue("SensorVal"));
            logger.Log("Writing {0} to stream ", datastream.Get(key).ToString());
        }

        public override void OnNotification(string roleName, string opName, IList<VParamType> retVals, VPort senderPort)
        {
            string message;
            lock (this)
            {
                if (roleName.Contains(":sensor:") && opName.Equals(RoleSensor.OpGetName))
                {
                        byte rcvdNum = (byte) (int) retVals[0].Value();

                        message = String.Format("from {0}, role = {1}, value = {2}", senderPort.ToString(), roleName, rcvdNum.ToString());
                        this.receivedMessageList.Add(message);
                }
                else if (roleName.Contains(":sensormultilevel:") && opName.Equals(RoleSensorMultiLevel.OpGetName))
                {
                    double rcvdNum = (double) retVals[0].Value();

                    message = String.Format("from {0}, role = {1}, value = {2}", senderPort.ToString(), roleName, rcvdNum.ToString());
                    this.receivedMessageList.Add(message);
                }
                else
                {
                    message = String.Format("Invalid role->op {0}->{1} from {2}", roleName, opName, senderPort.ToString());
                    this.receivedMessageList.Add(message);
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
                if (!accessibleSensorPorts.Contains(port) &&
                    GetCapabilityFromPlatform(port) != null &&
                    (Role.ContainsRole(port, RoleSensor.RoleName) || Role.ContainsRole(port, RoleSensorMultiLevel.RoleName)))
                {
                    accessibleSensorPorts.Add(port);

                    logger.Log("{0} added port {1}", this.ToString(), port.ToString());

                    if (Role.ContainsRole(port, RoleSensor.RoleName))
                    {
                        if (Subscribe(port, RoleSensor.Instance, RoleSensor.OpGetName))
                            logger.Log("{0} subscribed to port {1}", this.ToString(), port.ToString());
                        else
                            logger.Log("failed to subscribe to port {1}", this.ToString(), port.ToString());
                    }

                    if (Role.ContainsRole(port, RoleSensorMultiLevel.RoleName))
                    {
                        if (Subscribe(port, RoleSensorMultiLevel.Instance, RoleSensorMultiLevel.OpGetName))
                            logger.Log("{0} subscribed to port {1}", this.ToString(), port.ToString());
                        else
                            logger.Log("failed to subscribe to port {1}", this.ToString(), port.ToString());
                    }

                }
            }
        }

        public override void PortDeregistered(VPort port)
        {
            lock (this)
            {
                if (accessibleSensorPorts.Contains(port))
                {
                    accessibleSensorPorts.Remove(port);
                    logger.Log("{0} deregistered port {1}", this.ToString(), port.GetInfo().ModuleFacingName());
                }
            }
        }

        public List<string> GetReceivedMessages()
        {
            List<string> retList = new List<string>(this.receivedMessageList);
            retList.Reverse();
            return retList;
        }
    }
}