using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace HomeOS.Shared
{
    [DataContract]
    public class HeartbeatInfo : JsonSerializerBase<HeartbeatInfo>
    {
        [DataMember(Name = "HomeId")]
        public String HomeId { get; set; }
        [DataMember(Name = "OrgId")]
        public String OrgId { get; set; }
        [DataMember(Name = "StudyId")]
        public String StudyId { get; set; }
        [DataMember(Name = "HubTimestamp")]
        public String HubTimestamp { get; set; }
        [DataMember(Name = "TotalCpuPercentage")]
        public double TotalCpuPercentage { get; set; }
        [DataMember(Name = "PhysicalMemoryBytes")]
        public double PhysicalMemoryBytes { get; set; }
        [DataMember(Name = "ModuleMonitorInfoList")]
        public List<ModuleMonitorInfo> ModuleMonitorInfoList { get; set; }
        [DataMember(Name = "ScoutInfoList")]
        public List<ScoutInfo> ScoutInfoList { get; set; }
        [DataMember(Name = "HeartbeatIntervalMins")]
        public UInt32 HeartbeatIntervalMins { get; set; }
        [DataMember(Name = "SequenceNumber")]
        public UInt32 SequenceNumber { get; set; }
        [DataMember(Name = "HardwareId")]
        public string HardwareId { get; set; }
        [DataMember(Name = "PlatformVersion")]
        public string PlatformVersion { get; set; }

        public override string ToString()
        {
            string s = string.Format("HomeId:{0},\n OrgId={1},\n StudyId:{2},\n HubTimeStamp:{3},\n Total Cpu Usage:{4:0.00} %,\n Physical Memory Usage:{5:0.###} MBytes,\n ModuleMonitorInfoList: {6},\n  ScoutInfoList: {7},\n HeartbeatIntervalMins:{8},\n SequenceNumber:{9},\n HardwareId:{10},\n PlatformVersion:{11}",
                    this.HomeId.ToString(), this.OrgId, this.StudyId, this.HubTimestamp, this.TotalCpuPercentage, this.PhysicalMemoryBytes / 1.0E6,
                    this.ModuleMonitorInfoList.ToString(), this.ScoutInfoList.ToString(), this.HeartbeatIntervalMins, this.SequenceNumber, this.HardwareId,
                    this.PlatformVersion);
            return s;
        }
    }   
}
