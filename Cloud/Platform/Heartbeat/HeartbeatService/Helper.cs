using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace HomeOS.Cloud.Platform.Heartbeat
{
    public class Helper
    {
        private static HeartbeatTable HeartbeatTable;
        private static HomeIdentityTable HomeIdentityTable;
        private static HeartbeatLocalStorageTraceListener trace;


        public static void AddWebServiceEndpoint<IContractInterface>(
                                            ServiceHost serviceHost,
                                            Uri baseAddress)
        {
            var contract = ContractDescription.GetContract(typeof(IContractInterface));
            WebHttpBinding webBinding;

            webBinding = new WebHttpBinding();

            var webEndPoint = new ServiceEndpoint(contract, webBinding, new EndpointAddress(baseAddress));
            webEndPoint.EndpointBehaviors.Add(new WebHttpBehavior() { HelpEnabled = true });

            serviceHost.AddServiceEndpoint(webEndPoint);
        }

        public static HeartbeatLocalStorageTraceListener Trace()
        {
            if (null == trace)
            {
                trace = new HeartbeatLocalStorageTraceListener();
            }

            return trace;
        }

        public static HeartbeatTable GetHeartbeatTable()
        {
            if (null == HeartbeatTable)
            {
                HeartbeatTable = new HeartbeatTable();
            }

            return HeartbeatTable;
        }

        public static HomeIdentityTable GetHomeIdentityTable()
        {
            if (null == HomeIdentityTable)
            {
                HomeIdentityTable = new HomeIdentityTable();
            }

            return HomeIdentityTable;
        }

    }
}