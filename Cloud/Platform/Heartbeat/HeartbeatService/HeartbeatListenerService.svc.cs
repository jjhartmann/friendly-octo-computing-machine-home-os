using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using HomeOS.Shared;

namespace HomeOS.Cloud.Platform.Heartbeat
{
    public class HeartbeatListenerServiceFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return HeartbeatListenerService.CreateServiceHost(serviceType, baseAddresses[0]);
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    [ServiceKnownType(typeof(HeartbeatInfo))]
    public class HeartbeatListenerService : IHeartbeatListenerService
    {

        public static ServiceHost CreateServiceHost(Type contractInterface,
                                                     Uri baseAddress)
        {
            ServiceHost serviceHost = new ServiceHost(contractInterface, baseAddress);

            Helper.AddWebServiceEndpoint<IHeartbeatListenerService>(serviceHost, baseAddress);

            //service.Description.Behaviors.Add(new ServiceMetadataBehavior());
            serviceHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

            return serviceHost;
        }


        public HeartbeatListenerService()
        {
        }

        public void SetHeartbeatInfo(HeartbeatInfo hbi)
        {
            if (null == hbi.OrgId || hbi.OrgId.Length == 0)
            {
                hbi.OrgId = "Unknown";
            }

            HeartbeatTable HeartbeatTable = Helper.GetHeartbeatTable();

            if (null == hbi)
            {
                throw new ArgumentNullException();
            }

            try
            {
                HeartbeatTable.Write(hbi.OrgId, hbi.HomeId, hbi.SerializeToJsonStream());
            }
            catch(Exception e)
            {
                Helper.Trace().WriteLine("SimpleStorage.Write Failed with Exception:{0}", e.Message);
            }
        }

        public bool CanClaimHomeId(ClaimHomeIdInfo chi)
        {
            bool canClaim = false;
            HomeIdentityTable homeIdTable = Helper.GetHomeIdentityTable();
            if (null != homeIdTable)
            {
                canClaim = !homeIdTable.IsHomeIdentityPresent(chi.HardwareId, chi.HomeId);
            }

            if (canClaim)
            {
                homeIdTable.AddHomeIdentity(chi.HardwareId, chi.HomeId);
            }

            return canClaim;            
        }

    }
}
