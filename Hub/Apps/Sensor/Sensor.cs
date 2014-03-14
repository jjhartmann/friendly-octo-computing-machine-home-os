using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;
using HomeOS.Hub.Common.Bolt.DataStore;

namespace HomeOS.Hub.Apps.Sensor
{

    /// <summary>
    /// Logging module that logs data reported by sensors to UI and to data store
    /// </summary>

    [System.AddIn.AddIn("HomeOS.Hub.Apps.Sensor", Version = "1.0.0.0")]
    public class Sensor : ModuleBase
    {
        //list of accessible sensor ports in the system
        List<VPort> accessibleSensorPorts;

        private SafeServiceHost serviceHost;

        private WebFileServer appServer;

        List<string> receivedMessageList;

        List<string> tagList;

        IStream datastream;
        DateTime streamClosed = DateTime.Now;

        public override void Start()
        {
            //Using "Sensor:" to indicate clearly in the log where this line came from.
            logger.Log("Sensor:Started: {0} ", ToString());

            // remoteSync flag can be set to true, if the Platform Settings has the Cloud storage
            // information i.e., DataStoreAccountName, DataStoreAccountKey values
            datastream = base.CreateValueDataStream<StrKey, StrValue>("data", true /* remoteSync */);

            SensorService sensorService = new SensorService(logger, this);
            serviceHost = new SafeServiceHost(logger, typeof(ISensorContract), sensorService, this, Constants.AjaxSuffix, moduleInfo.BaseURL());
            serviceHost.Open();

            appServer = new WebFileServer(moduleInfo.BinaryDir(), moduleInfo.BaseURL(), logger);


            //........... instantiate the list of other ports that we are interested in
            accessibleSensorPorts = new List<VPort>();

            //..... get the list of current ports from the platform
            IList<VPort> allPortsList = GetAllPortsFromPlatform();

            if (allPortsList != null)
                ProcessAllPortsList(allPortsList);

            this.receivedMessageList = new List<string>();

            this.tagList = new List<string>();
        }

        public override void Stop()
        {
            logger.Log("Sensor:clean up");
            if (datastream != null)
                datastream.Close();
        }

        public void WriteToStream(string tag, string data)
        {

            //add the tag to a list if it's not already in there
            if (!tagList.Contains(tag))
            {
                this.tagList.Add(tag);
            }

            StrKey key = new StrKey(tag);
            if (datastream != null)
            {
                datastream.Append(key, new StrValue(data));
             // Don't put in log twice   logger.Log("Sensor:Writing tag {0},{1} to stream ", key.ToString(), datastream.Get(key).ToString());

                //Check if we should close it to force data to be written to Azure (AJB - remove once there is option to open stream with sycning at least every N minutes)
                double minutes = DateTime.Now.Subtract(streamClosed).TotalMinutes;
                if (minutes > 60)
                {
                    streamClosed = DateTime.Now;
                    datastream.Close();
                    logger.Log("Sensor:{0}: closed and reopened data stream", streamClosed.ToString());
                    datastream = base.CreateValueDataStream<StrKey, StrValue>("data", true /* remoteSync */);

                }

            }
        }

        public override void OnNotification(string roleName, string opName, IList<VParamType> retVals, VPort senderPort)
        {
            string message;
            string sensorData;
            string sensorTag = senderPort.GetInfo().GetFriendlyName() + roleName;

            lock (this)
            {
                if (roleName.Contains(RoleSensor.RoleName) && opName.Equals(RoleSensor.OpGetName))
                {
                        byte rcvdNum = (byte) (int) retVals[0].Value();
                        sensorData = rcvdNum.ToString();
                                     
                }
                else if (roleName.Contains(RoleSensorMultiLevel.RoleName) && opName.Equals(RoleSensorMultiLevel.OpGetName))
                {
                    double rcvdNum = (double) retVals[0].Value();           
                    sensorData = rcvdNum.ToString();                
                }
                else
                {
                    sensorData = String.Format("Invalid role->op {0}->{1} from {2}", roleName, opName, sensorTag);                 
                }
            }

            //Write to the stream
            WriteToStream(sensorTag, sensorData);
            //Create local list of alerts for display
            message = String.Format("{0}\t{1}\t{2}", DateTime.Now, sensorTag, sensorData);
            this.receivedMessageList.Add(message);
            //Log
            logger.Log("Sensor\t{0}", message);          
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

            logger.Log("Sensor:{0} got registeration notification for {1}", ToString(), port.ToString());

            lock (this)
            {
                if (!accessibleSensorPorts.Contains(port) &&
                    GetCapabilityFromPlatform(port) != null &&
                    (Role.ContainsRole(port, RoleSensor.RoleName) || Role.ContainsRole(port, RoleSensorMultiLevel.RoleName)))
                {
                    accessibleSensorPorts.Add(port);

                    logger.Log("Sensor:{0} added port {1}", this.ToString(), port.ToString());

                    if (Role.ContainsRole(port, RoleSensor.RoleName))
                    {
                        if (Subscribe(port, RoleSensor.Instance, RoleSensor.OpGetName))
                            logger.Log("Sensor:{0} subscribed to port {1}", this.ToString(), port.ToString());
                        else
                            logger.Log("Sensor:{0} failed to subscribe to port {1}", this.ToString(), port.ToString());
                    }

                    if (Role.ContainsRole(port, RoleSensorMultiLevel.RoleName))
                    {
                        if (Subscribe(port, RoleSensorMultiLevel.Instance, RoleSensorMultiLevel.OpGetName))
                            logger.Log("Sensor:{0} subscribed to port {1}", this.ToString(), port.ToString());
                        else
                            logger.Log("Sensor: {0} failed to subscribe to port {1}", this.ToString(), port.ToString());
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
                    logger.Log("Sensor:{0} deregistered port {1}", this.ToString(), port.GetInfo().ModuleFacingName());
                }
            }
        }

        public List<string> GetReceivedMessages()
        {

            ////Ask for data from all tags
            //tagList.ForEach(delegate(String tag)
            //{
            //    IEnumerable<IDataItem> data = datastream.GetAll(new StrKey(tag), 0, StreamFactory.NowUtc());
            //    foreach (IDataItem dataItem in data)
            //        logger.Log(tag + " ---> " + DateTime.FromFileTimeUtc(dataItem.GetTimestamp()) + ":" + dataItem.GetVal().ToString());

            //});

          

            List<string> retList;
            int numtoShow = 100;
            //check how long it is and just return last numToShow elements
            int length = this.receivedMessageList.Count();
            if (length > numtoShow) {
                retList = new List<string>(this.receivedMessageList.GetRange(length - numtoShow, numtoShow));
            }
            else 
                retList = new List<string>(this.receivedMessageList);

            //newest displayed at the top
            retList.Reverse();
            return retList;
        }
    }
}