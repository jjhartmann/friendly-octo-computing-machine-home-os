//Kamil.Hawdziejuk@uj.edu.pl
//02.01.2013

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;
using HomeOS.Hub.Common;
using System.ServiceModel.Description;

namespace HomeOS.Hub.Apps.Volume
{
    /// <summary>
    /// Service that changes volume on the system. WCF service uses NAudio library to control that.
    /// </summary>
    [System.AddIn.AddIn("HomeOS.Hub.Apps.Volume", Version = "1.0.0.0")]
    public class Volume : ModuleBase
    {
        ServiceHost serviceHost;

        public override void Start()
        {           
            //this.moduleInfo
            //initialize your module here
            //.................instantiate the port
            HomeOS.Hub.Platform.Views.VPortInfo portInfo = GetPortInfoFromPlatform("port");
            var port = InitPort(portInfo);


            //.................register the port after the binding is complete
            RegisterPortWithPlatform(port);


            // make sure that the control never falls out of this function. 
            // o/w, your module will be unloaded then
            // so, if your module doesn’t do anything active but reacts to events from other places
            // use “System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite)”  as the last 
            // line of this function


            logger.Log("Started: {0}", ToString());

            VolumeSvc volumeService = new VolumeSvc(logger);
                
            string homeId = this.GetConfSetting("HomeId");
            string homeIdPart = string.Empty;
            if (!string.IsNullOrEmpty(homeId))
            {
                homeIdPart = "/" + homeId;
            }

            var uri =  new Uri(Constants.InfoServiceAddress + homeIdPart + "/" + moduleInfo.FriendlyName());
            serviceHost = VolumeSvc.CreateServiceHost(volumeService,uri);
            var behavior = serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
            behavior.IncludeExceptionDetailInFaults = true;
            serviceHost.Open();

            logger.Log("volume service is open for business");

            //..... get the list of current ports from the platform
            IList<HomeOS.Hub.Platform.Views.VPort> allPortsList = GetAllPortsFromPlatform();

            if (allPortsList != null)
            {
                foreach (HomeOS.Hub.Platform.Views.VPort portItem in allPortsList)
                {
                    PortRegistered(portItem);
                }
            }

        }

        public override void Stop()
        {
            //throw new NotImplementedException();
        }

        public override void PortRegistered(Platform.Views.VPort port)
        {
           // throw new NotImplementedException();
        }

        public override void PortDeregistered(Platform.Views.VPort port)
        {
            //throw new NotImplementedException();
        }
    }
}
