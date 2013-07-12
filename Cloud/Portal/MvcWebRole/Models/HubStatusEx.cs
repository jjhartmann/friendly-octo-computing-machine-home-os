using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using HomeOS.Shared;

namespace HomeOS.Cloud.Portal.MvcWebRole.Models
{
    public class HubStatusEx : HubStatus
    {
        public HubStatusEx(HubStatus hubStatus)
        {
            this.ETag = hubStatus.ETag;
            this.PartitionKey = hubStatus.PartitionKey;
            this.RowKey = hubStatus.RowKey;
            this.Timestamp = hubStatus.Timestamp;
            this.OrgID = hubStatus.OrgID;
            this.HomeID = hubStatus.HomeID;
            this.Status = hubStatus.Status;
            this.StudyID = hubStatus.StudyID;
            this.HubTimeStamp = hubStatus.HubTimeStamp;
            this.LastHeartbeatReported = hubStatus.LastHeartbeatReported;
            this.LastHeartbeatSequenceNumber = hubStatus.LastHeartbeatSequenceNumber;
            this.ExpectedHeartbeatIntervalInMins = hubStatus.ExpectedHeartbeatIntervalInMins;
            this.CurrentHeartbeatIntervalInMins = hubStatus.CurrentHeartbeatIntervalInMins;
            this.ModuleStatusListAsJson = hubStatus.ModuleStatusListAsJson;
            this.Memory = hubStatus.Memory;
            this.CPU = hubStatus.CPU;
        }

        public class ModuleStatusListWrapper
        {
            public List<ModuleStatus> ModuleStatusList { get; set; }
        }

        [Display(Name = "Module Status Info")]
        [DisplayFormat(NullDisplayText = "'Not Available'")]
        public List<ModuleStatus> ModuleStatusList
        {
            get
            {
                ModuleStatusListWrapper moduleStatusListWrapper = null;
                moduleStatusListWrapper = SerializerHelper<ModuleStatusListWrapper>.DeserializeFromJsonStream(this.ModuleStatusListAsJson);
                if (null == moduleStatusListWrapper)
                    moduleStatusListWrapper = new ModuleStatusListWrapper();

                return moduleStatusListWrapper.ModuleStatusList;
            }
        }

        public List<OrgInfo> OrgList { get; set; }                                         
    }
}