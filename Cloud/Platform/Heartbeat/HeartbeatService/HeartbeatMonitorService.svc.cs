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
    public class HeartbeatMonitorServiceFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return HeartbeatMonitorService.CreateServiceHost(serviceType, baseAddresses[0]);
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    [ServiceKnownType(typeof(HeartbeatInfo))]
    public class HeartbeatMonitorService : IHeartbeatMonitorService
    {

        public static ServiceHost CreateServiceHost(Type contractInterface,
                                                     Uri baseAddress)
        {
            ServiceHost serviceHost = new ServiceHost(contractInterface, baseAddress);

            Helper.AddWebServiceEndpoint<IHeartbeatMonitorService>(serviceHost, baseAddress);

            //service.Description.Behaviors.Add(new ServiceMetadataBehavior());
            serviceHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

            return serviceHost;
        }
        

        public HeartbeatMonitorService()
        {
        }

        public List<Tuple<string /* orgId */, string /* homeId */, DateTime /* UtcTime */, HeartbeatInfo>> GetHeartbeatInfoRangeByOrg(string orgId)
        {
            HeartbeatTable HeartbeatTable = new HeartbeatTable();

            List<Tuple<string, string, DateTime, HeartbeatInfo>> heartbeatInfoTuples = new List<Tuple<string, string, DateTime, HeartbeatInfo>>();

            foreach (HeartbeatTable.HeartbeatEntity hbe in 
                    HeartbeatTable.GetHeartbeatEntitiesForOrg(
                        orgId))
            {
                heartbeatInfoTuples.Add(
                    new Tuple<string, string, DateTime, HeartbeatInfo>(hbe.PartitionKey, hbe.RowKey, hbe.Timestamp.DateTime, (HeartbeatInfo) new HeartbeatInfo().DeserializeFromJsonStream(hbe.HeartbeatInfo)));
            }

            return heartbeatInfoTuples;
        }

        public List<Tuple<string /* orgId */, string /* homeId */, DateTime /* UtcTime */, HeartbeatInfo>> GetHeartbeatInfoRangeByOrgAndCloudTime(string orgId, string startTimeUtc, string timeOffset)
        {
            HeartbeatTable HeartbeatTable = new HeartbeatTable();

            List<Tuple<string, string, DateTime, HeartbeatInfo>> timeStampHeartbeatInfoTuples = new List<Tuple<string, string, DateTime, HeartbeatInfo>>();
            DateTimeOffset dtoStartTime;
            DateTimeOffset dtoStopTime;
            TimeSpan tsTimeOffset;

            TimeSpan.TryParse(timeOffset, out tsTimeOffset);

            if (tsTimeOffset.CompareTo(TimeSpan.Zero) >= 0)
            {
                DateTimeOffset.TryParse(startTimeUtc, out dtoStartTime);
                dtoStopTime = dtoStartTime + tsTimeOffset;
            }
            else
            {
                DateTimeOffset.TryParse(startTimeUtc, out dtoStopTime);
                dtoStartTime = dtoStopTime + tsTimeOffset;
            }

            foreach (HeartbeatTable.HeartbeatEntity hbe in 
                    HeartbeatTable.GetHeartbeatEntitiesInTimeRange(
                        orgId,
                        dtoStartTime,
                        dtoStopTime))
            {
                timeStampHeartbeatInfoTuples.Add(
                    new Tuple<string, string, DateTime, HeartbeatInfo>(hbe.PartitionKey, hbe.RowKey, hbe.Timestamp.DateTime, (HeartbeatInfo) new HeartbeatInfo().DeserializeFromJsonStream(hbe.HeartbeatInfo)));
            }

            return timeStampHeartbeatInfoTuples;
        }

    }
}
