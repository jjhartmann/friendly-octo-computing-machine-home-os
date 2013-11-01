using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using HomeOS.Hub.Platform.Views;
using HomeOS.Hub.Common;

namespace HomeOS.Hub.Apps.Switch
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SwitchSvc : ISwitchSvcContract
    {
        public static SafeServiceHost CreateServiceHost(VLogger logger, ModuleBase moduleBase, ISwitchSvcContract instance,
                                                     string address)
        {
            SafeServiceHost service = new SafeServiceHost(logger, moduleBase, instance, address);

            var contract = ContractDescription.GetContract(typeof(ISwitchSvcContract));

            var webBinding = new WebHttpBinding();
            var webEndPoint = new ServiceEndpoint(contract, webBinding, new EndpointAddress(service.BaseAddresses()[0]));
            webEndPoint.EndpointBehaviors.Add(new WebHttpBehavior());

            service.AddServiceEndpoint(webEndPoint);

            service.AddServiceMetadataBehavior(new ServiceMetadataBehavior());

            return service;
        }

        protected VLogger logger;
        SwitchMultiLevelController controller;

        public SwitchSvc(VLogger logger, SwitchMultiLevelController controller)
        {
            this.logger = logger;
            this.controller = controller;
        }



        public List<string> GetAllSwitches()
        {
            try
            {
                List<string> retVal = controller.GetAllSwitches();

                retVal.Insert(0, "");

                return retVal;
            }
            catch (Exception e)
            {
                logger.Log("Got exception in GetSwitchList: " + e);
                return new List<string>() { e.Message };
            }
        }

        public List<string> SetLevel(string switchFriendlyName, string level)
        {
            try
            {
                byte byteLevel = byte.Parse(level);

                controller.SetLevel(switchFriendlyName, byteLevel);

                return new List<string>() { "" };
            }
            catch (Exception e)
            {
                logger.Log("Got exception in GetSwitchList: " + e);
                return new List<string>() { e.Message };
            }
        }
    }


    [ServiceContract]
    public interface ISwitchSvcContract
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetAllSwitches();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> SetLevel(string switchFriendlyName, string level);
    }
}