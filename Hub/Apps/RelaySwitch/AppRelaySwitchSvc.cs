using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;

namespace HomeOS.Hub.Apps.RelaySwitch
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AppRelaySwitchService : ISimplexRelaySwitchNotifierContract
    {
        private VLogger logger;
        private AppRelaySwitch relaySwitchApp;

        public AppRelaySwitchService(AppRelaySwitch relayApp, VLogger logger)
        {
            this.logger = logger;
            this.relaySwitchApp = relayApp;
        }

        public static SafeServiceHost CreateServiceHost(VLogger logger, ModuleBase moduleBase, ISimplexRelaySwitchNotifierContract instance,
                                                     string address)
        {
            SafeServiceHost service = new SafeServiceHost(logger, moduleBase, instance, address);

            var contract = ContractDescription.GetContract(typeof(ISimplexRelaySwitchNotifierContract));

            var webBinding = new WebHttpBinding();
            var webEndPoint = new ServiceEndpoint(contract, webBinding, new EndpointAddress(service.BaseAddresses()[0]));
            webEndPoint.EndpointBehaviors.Add(new WebHttpBehavior());

            service.AddServiceEndpoint(webEndPoint);

            service.AddServiceMetadataBehavior(new ServiceMetadataBehavior());

            return service;
        }

        public int GetRelaySwitch()
        {
            return (int)relaySwitchApp.IsOn;
        }

        public string SetRelaySwitch()
        {
            relaySwitchApp.SetRelaySwitch();
            if (this.relaySwitchApp.IsOn == 0)
            {
                this.relaySwitchApp.IsOn = 1;
            }
            else
            {
                this.relaySwitchApp.IsOn = 0;
            }
            return "";
        }
    }

    [ServiceContract]
    public interface ISimplexRelaySwitchNotifierContract
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ///is on?
        int GetRelaySwitch();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string SetRelaySwitch();
    }

}
