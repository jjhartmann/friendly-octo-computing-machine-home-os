using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;
using System.Net;
using System.Runtime.Serialization;
//using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Description;


namespace HomeOS.Hub.Apps.Light
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AppLightSvc : ISimplexLightNotifierContract
    {
        private VLogger logger;
        private AppLight lightApp;

        public AppLightSvc(AppLight _ligthApp, VLogger _logger)
        {
            this.logger = _logger;
            this.lightApp = _ligthApp;
        }

        public static SafeServiceHost CreateServiceHost(VLogger _logger, ModuleBase _moduleBase,
            ISimplexLightNotifierContract _instance, string _address)
        {
            SafeServiceHost service = new SafeServiceHost(_logger, _moduleBase, _instance, _address);
            var contract = ContractDescription.GetContract(typeof(ISimplexLightNotifierContract));
            var webBinding = new WebHttpBinding();
            var webEndPoint = new ServiceEndpoint(contract, webBinding, new EndpointAddress(service.BaseAddresses()[0]));
            webEndPoint.EndpointBehaviors.Add(new WebHttpBehavior());
            service.AddServiceEndpoint(webEndPoint);
            service.AddServiceMetadataBehavior(new ServiceMetadataBehavior());
            return service;
        }

        public double GetLight()
        {
            return this.lightApp.Light;
        }
    }

    [ServiceContract]
    public interface ISimplexLightNotifierContract
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat=WebMessageFormat.Json)]
        double GetLight();

        //[OperationContract]
        //[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        //string SetLEDs(double low, double high);
    }
}
