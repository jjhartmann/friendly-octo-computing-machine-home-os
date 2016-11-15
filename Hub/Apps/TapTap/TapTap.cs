using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;
using HomeOS.Hub.Common.Bolt.DataStore;

namespace HomeOS.Hub.Apps.TapTap
{

    /// <summary>
    /// A taptap a module that 
    /// 1. sends ping messages to all active taptap ports
    /// </summary>

    [System.AddIn.AddIn("HomeOS.Hub.Apps.TapTap")]
    public class TapTap :  ModuleBase
    {
        //list of accessible taptap ports in the system
        List<VPort> accessibleTapTapPorts;

        private SafeServiceHost serviceHost;

        private WebFileServer appServer;

        List<string> receivedMessageList;
        SafeThread worker = null;

        IStream datastream;

        public override void Start()
        {
            logger.Log("Started: {0} ", ToString());

            TapTapService taptapService = new TapTapService(logger, this);
            serviceHost = new SafeServiceHost(logger,typeof(ITapTapContract), taptapService , this, Constants.AjaxSuffix, moduleInfo.BaseURL());
            serviceHost.Open();

            appServer = new WebFileServer(moduleInfo.BinaryDir(), moduleInfo.BaseURL(), logger);
            
 
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
                Work();
            }, "AppTapTap-worker", logger);
            worker.Start();
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
        public void Work()
        {
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
                //if (!accessibleTapTapPorts.Contains(port) && 
                //    Role.ContainsRole(port, RoleSwitchMultiLevel.RoleName) && 
                //    GetCapabilityFromPlatform(port) != null)
                //{
                //    accessibleTapTapPorts.Add(port);

                //    logger.Log("{0} added port {1}", this.ToString(), port.ToString());

                //    if (Subscribe(port, RoleSwitchMultiLevel.Instance, RoleSwitchMultiLevel.OpEchoSubName))
                //        logger.Log("{0} subscribed to port {1}", this.ToString(), port.ToString());
                //    else
                //        logger.Log("failed to subscribe to port {1}", this.ToString(), port.ToString());
                //}
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

        public List<string> GetReceivedMessages()
        {
            List<string> retList = new List<string>(this.receivedMessageList);
            retList.Reverse();
            return retList;
        }
    }
}