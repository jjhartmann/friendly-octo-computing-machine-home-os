using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeOS.Hub.Common;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Drawing;
using HomeOS.Hub.Platform.Views;

namespace HomeOS.Hub.Drivers.Gadgeteer.MicrosoftResearch.WindowCamera
{
    [DataContract]
    public class Response
    {
        [DataMember(Name = "DeviceId")]
        public string DeviceId { get; set; }
        //[DataMember(Name = "moisture")]  //AJB - this looks like copy paste bug
        //public int moisture { get; set; }
    }


    [System.AddIn.AddIn("HomeOS.Hub.Drivers.Gadgeteer.MicrosoftResearch.WindowCamera")]
    public class DriverGadgeteerMicrosoftResearchWindowCamera : ModuleBase
    {

        const byte WetThreshold = 1; //values equal or more will be considered wet

        string deviceId;

        IPAddress deviceIp;

        Port devicePort;

        byte[] latestImageBytes = new byte[0];
        SafeThread worker = null;

        private WebFileServer imageServer;

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

            List<VRole> roles = new List<VRole>() {RoleCamera.Instance};

            devicePort = InitPort(pInfo);
            BindRoles(devicePort, roles, OnOperationInvoke);

            RegisterPortWithPlatform(devicePort);
            worker = new SafeThread(delegate()
            {
                PollDevice();
            }, "DriverGadgeteerMSRWindowCamera-PollDevice", logger);
            worker.Start();

            imageServer = new WebFileServer(moduleInfo.BinaryDir(), moduleInfo.BaseURL(), logger);
        }

        private void PollDevice()
        {
            while (true)
            {
                try
                {
                    string url = string.Format("http://{0}/webcam", deviceIp);

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                    
                    HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));

                    //logger.Log("GOT IMAGE");

                    if (response.ContentType.Equals("image/bmp"))
                    {
                        System.IO.Stream responseStream = response.GetResponseStream();

                        lock (this)
                        {

                            if (latestImageBytes.Length < response.ContentLength)
                            {
                                latestImageBytes = new byte[response.ContentLength];
                            }

                            int readCumulative = 0, readThisRound = 0;
                            do
                            {
                                readThisRound = responseStream.Read(latestImageBytes, readCumulative, (int)response.ContentLength - readCumulative);

                                readCumulative += readThisRound;
                            }
                            while (readThisRound != 0);

                            if (readCumulative != response.ContentLength)
                                logger.Log("Could not read all the bytes from the camera. Read {0}/{1}", readCumulative.ToString(),
                                    response.ContentLength.ToString());

                           //Uncomment this if the camera is inverted
                           //latestImageBytes = RotateImage(latestImageBytes);

                        }
                    }

                    response.Close();


                    //notify the subscribers
                    List<VParamType> ret = new List<VParamType>();
                    ret.Add(new ParamType(ParamType.SimpleType.jpegimage, latestImageBytes));

                    devicePort.Notify(RoleCamera.Instance.Name(), RoleCamera.OpGetVideo, ret);
                }
                catch (Exception e)
                {
                    logger.Log("couldn't talk to the device {0} ip={1}.\nare the arguments correct?\n exception details: {2}", this.ToString(), deviceIp.ToString(), e.ToString());

                    //lets try getting the IP again
                    deviceIp = GetDeviceIp(deviceId);
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        private byte[] RotateImage(byte[] imageBytes)
        {

            Bitmap bitmap = new Bitmap(new System.IO.MemoryStream(imageBytes));
            bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);

            System.IO.MemoryStream memStream = new System.IO.MemoryStream();
            bitmap.Save(memStream, System.Drawing.Imaging.ImageFormat.Jpeg);

            return memStream.ToArray();
        }


        /// <summary>
        /// The demultiplexing routing for incoming
        /// </summary>
        /// <param name="message"></param>
        private List<VParamType> OnOperationInvoke(string roleName, String opName, IList<VParamType> parameters)
        {
            switch (opName.ToLower())
            {
                case RoleCamera.OpGetImageName:
                    {
                        List<VParamType> retVals = new List<VParamType>();

                        retVals.Add(new ParamType(ParamType.SimpleType.jpegimage, latestImageBytes));

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

        public override void Stop()
        {
            if (worker != null)
                worker.Abort();
         //   throw new NotImplementedException();

            imageServer.Dispose();
        }

        public override string GetDescription(string hint)
        {
            logger.Log("DriverGadgeteer.GetDescription for {0}", hint);
            return "Gadgeteer Camera";
        }


        //we have nothing to do with other ports
        public override void PortRegistered(VPort port) { }
        public override void PortDeregistered(VPort port) { }
    }
}
