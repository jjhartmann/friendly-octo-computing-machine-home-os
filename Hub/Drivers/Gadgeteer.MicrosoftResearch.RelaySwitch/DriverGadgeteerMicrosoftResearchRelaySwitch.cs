using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeOS.Hub.Common;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using HomeOS.Hub.Platform.Views;

namespace HomeOS.Hub.Drivers.Gadgeteer.MicrosoftResearch.RelaySwitch
{
    [DataContract]
    public class Response
    {
        [DataMember(Name = "DeviceId")]
        public string DeviceId { get; set; }
        [DataMember(Name = "IsOn")]
        public int IsOn { get; set; }
    }


    [System.AddIn.AddIn("HomeOS.Hub.Drivers.Gadgeteer.MicrosoftResearch.RelaySwitch")]
    public class DriverGadgeteerMicrosoftResearchRelaySwitch : DriverGadgeteerBase
    {
        double lastValue = 0;
        /// <summary>
        /// For loging information about turning on/off relay switch
        /// </summary>
        VLogger driverLogger;

        protected override List<VRole> GetRoleList()
        {
            return new List<VRole>() { RoleSensor.Instance, RoleActuator.Instance };
        }

        public override void Start()
        {
            driverLogger = new Logger(moduleInfo.WorkingDir() +"\\" + "module.csv");   

            base.Start();
        }

        protected override void WorkerThread()
        {
            //while (true)
            {
                //try
                //{
                //    string url = string.Format("http://{0}/IsOn", deviceIp);

                //    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                //    HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                //    if (response.StatusCode != HttpStatusCode.OK)
                //        throw new Exception(String.Format(
                //        "Server error (HTTP {0}: {1}).",
                //        response.StatusCode,
                //        response.StatusDescription));
                //    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Response));
                //    object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                //    Response jsonResponse = objResponse as Response;

                //    response.Close();

                //    this.Log(jsonResponse.IsOn);
                //    //double newValue = NormalizeTempValue(jsonResponse.temperature);
                //    int newValue = jsonResponse.IsOn;
                //    //notify the subscribers
                //    //if (newValue != lastValue)
                //    {
                //        IList<VParamType> retVals = new List<VParamType>();
                //        retVals.Add(new ParamType(newValue));

                //        devicePort.Notify(RoleSensor.RoleName, RoleSensor.OpGetName, retVals);
                //    }

                //    lastValue = newValue;

                //}
                //catch (Exception e)
                //{
                //    logger.Log("{0}: couldn't talk to the device. are the arguments correct?\n exception details: {1}", this.ToString(), e.ToString());

                //    //lets try getting the IP again
                //    deviceIp = GetDeviceIp(deviceId);
                //}

                System.Threading.Thread.Sleep(10000);
            }
        }

        private void Log(int isOn)
        {
            if (isOn > -1 && false)
            {
                logger.Log("Gadgeteer Relay: {0}", isOn.ToString());
                DateTime date = DateTime.Now;
                driverLogger.Log(";{0}", isOn.ToString());
            }
        }

        /// <summary>
        /// The demultiplexing routing for incoming
        /// </summary>
        /// <param name="message"></param>
        protected override List<VParamType> OnOperationInvoke(string roleName, String opName, IList<VParamType> parameters)
        {
            switch (roleName.ToLower())
            {
                case RoleSensor.RoleName:
                    {
                        switch (opName.ToLower())
                        {
                            case RoleSensor.OpGetName:
                                {
                                    List<VParamType> retVals = new List<VParamType>();
                                    retVals.Add(new ParamType(lastValue));

                                    return retVals;
                                }
                            default:
                                logger.Log("Unknown operation {0} for role {1}", opName, roleName);
                                return null;
                        }
                    }
                case RoleActuator.RoleName:
                    {
                        switch (opName.ToLower()) 
                        { 
                            case RoleActuator.OpPutName:
                                { 
                                    try 
                                    {
                                        string url = string.Format("http://{0}/IsOn", deviceIp);

                                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                                        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                                        DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Response));
                                        object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                                        Response jsonResponse = objResponse as Response;
                                        response.Close();

                                       
                                        int newValue = jsonResponse.IsOn;
                                        //notify the subscribers
                                        if (newValue != lastValue)
                                        {
                                            driverLogger.Log("Relay switch is now {0}", newValue.ToString());
                                            IList<VParamType> retVals = new List<VParamType>();
                                            retVals.Add(new ParamType(newValue));

                                            devicePort.Notify(RoleSensor.RoleName, RoleSensor.OpGetName, retVals);
                                        }
                                        lastValue = newValue;

                                    } 
                                    catch (Exception e) 
                                    { 
                                        logger.Log("{0}: couldn't talk to the device. are the arguments correct?\n exception details: {1}", this.ToString(), e.ToString());
                                        //lets try getting the IP again 
                                        deviceIp = GetDeviceIp(deviceId); 
                                    } 
                                    return new List<VParamType>(); 
                                } 
                            default: 
                                logger.Log("Unknown operation {0} for {1}", opName, roleName); 
                                return null; 
                        }
                    }
                    default: 
                        logger.Log("Unknown role {0}", roleName); 
                        return null;
            }
        }

        public override void Stop()
        {
            if (worker != null)
                worker.Abort();
        }

        //we have nothing to do with other ports
        public override void PortRegistered(VPort port) { }
        public override void PortDeregistered(VPort port) { }

        public override string GetDescription(string hint)
        {
            logger.Log("DriverGadgeteer.GetDescription for {0}", hint);
            return "Gadgeteer";
        }

    }
}
