using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeOS.Hub.Common;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using HomeOS.Hub.Platform.Views;

namespace HomeOS.Hub.Drivers.Gadgeteer.MicrosoftResearch.MoistureSensor
{
    [DataContract]
    public class Response
    {
        [DataMember(Name = "DeviceId")]
        public string DeviceId { get; set; }
        [DataMember(Name = "moisture")]
        public int moisture { get; set; }
    }


    [System.AddIn.AddIn("HomeOS.Hub.Drivers.Gadgeteer.MicrosoftResearch.MoistureSensor")]
    public class DriverGadgeteerMicrosoftResearchMoistureSensor : ModuleBase
    {

        const byte WetThreshold = 1; //values equal or more will be considered wet

        string deviceId;

        IPAddress deviceIp;

        Port devicePort;

        byte lastValue = 0;
        SafeThread worker = null; 
        public override void Start()
        {

            try
            {
                string[] words = moduleInfo.Args();

                deviceId = words[0];
            }
            catch (Exception e)
            {
                logger.Log("{0}: Improper arguments: {1}. Exiting module", this.ToString(), e.ToString());
                return;
            }

            //get the IP address
            deviceIp = GetDeviceIp(deviceId);

            if (deviceIp == null)
            {
                logger.Log("{0} did not get a device ip for deviceId: {1}. Returning", base.moduleInfo.BinaryName(), deviceId.ToString()); 
                return;
            }

            //add the camera service port
            VPortInfo pInfo = GetPortInfoFromPlatform("gadgeteer-" + deviceId);

            List<VRole> roles = new List<VRole>() {RoleSensor.Instance};

            devicePort = InitPort(pInfo);
            BindRoles(devicePort, roles, OnOperationInvoke);

            RegisterPortWithPlatform(devicePort);

            worker = new SafeThread(delegate()
            {
                PollDevice();
            }, "DriverGadgeteerMSRMoistureSensor-PollDevice", logger);
            worker.Start();
        }

        private void PollDevice()
        {
            while (true)
            {
                try
                {
                    string url = string.Format("http://{0}/moisture", deviceIp);

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Response));
                    object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                    Response jsonResponse = objResponse as Response;

                    response.Close();

                    if (jsonResponse.moisture > 0)
                        logger.Log("Gadgeteer Moisture: {0}", jsonResponse.moisture.ToString());

                    byte newValue = NormalizeMoistureValue(jsonResponse.moisture);

                    //notify the subscribers
                    if (newValue != lastValue)
                    {
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


                System.Threading.Thread.Sleep(4 * 1000);
            }
        }

        private byte NormalizeMoistureValue(int rawValue)
        {
            if (rawValue >= WetThreshold)
                return 255;
            else
                return 0;
        }


        /// <summary>
        /// The demultiplexing routing for incoming
        /// </summary>
        /// <param name="message"></param>
        private List<VParamType> OnOperationInvoke(string roleName, String opName, IList<VParamType> parameters)
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

        public IPAddress GetDeviceIp(string deviceId)
        {

            //if the Id is an IP Address itself, return that.
            //else get the Ip from platform

            IPAddress ipAddress = null;

            try
            {
                ipAddress = IPAddress.Parse(deviceId);
                return ipAddress;
            }
            catch (Exception)
            {
            }

            string ipAddrStr = GetDeviceIpAddress(deviceId);

            try
            {
                ipAddress = IPAddress.Parse(ipAddrStr);
                return ipAddress;
            }
            catch (Exception)
            {
                logger.Log("{0} couldn't get IP address from {1} or {2}", this.ToString(), deviceId, ipAddrStr);
            }

            return null;
        }

        //public override string TestIpDevice(string ipAddress, string username, string password)
        //{
        //    //gadgeteer devices don't use credentials
        //    //so we ok anything we get
        //    return "";
        //}

        public override void Stop()
        {
            if (worker != null)
                worker.Abort();
        //    throw new NotImplementedException();
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
