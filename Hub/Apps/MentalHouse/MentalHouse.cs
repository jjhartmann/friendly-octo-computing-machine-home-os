using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;
using HomeOS.Hub.Common.Bolt.DataStore;
using HomeOS.Hub.Custom;


namespace HomeOS.Hub.Apps.MentalHouse
{
    //Nicholas Kostriken

    [System.AddIn.AddIn("HomeOS.Hub.Apps.MentalHouse")]

    public class MentalHouse :  ModuleBase
    {
        // MindWave driver port
        VPort mindWavePort;

        private SafeServiceHost serviceHost;
        private WebFileServer appServer;
        List<string> receivedMessageList;
        SafeThread worker = null;
        

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
            }

            return rcvdNum;
        }


        public override void OnNotification(string roleName, string opName, IList<VParamType> retVals, VPort senderPort)
        {
            //...
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