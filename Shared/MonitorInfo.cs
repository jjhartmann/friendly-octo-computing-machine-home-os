﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace HomeOS.Shared
{
    [DataContract]
    public class ModuleMonitorInfo : JsonSerializerBase<ModuleMonitorInfo>
    {
        [DataMember(Name = "ModuleFriendlyName")]
        public string ModuleFriendlyName { get; set; }
        [DataMember(Name = "MonitoringTotalProcessorTime")]
        public long MonitoringTotalProcessorTime { get; set; }
        [DataMember(Name = "MonitoringTotalAllocatedMemorySize")]
        public long MonitoringTotalAllocatedMemorySize { get; set; }
        [DataMember(Name = "MonitoringSurvivedMemorySize")]
        public long MonitoringSurvivedMemorySize { get; set; }
        [DataMember(Name = "MonitoringSurvivedProcessMemorySize")]
        public long MonitoringSurvivedProcessMemorySize { get; set; }

        public override string ToString()
        {
            string s = string.Format("modName:{0,15}, cpu:{1,15} msecs, allocMem:{2,15} Bytes, surMem:{3,15} Bytes, procMem:{4,15} Bytes",
                    this.ModuleFriendlyName,
                    this.MonitoringTotalProcessorTime,
                    this.MonitoringTotalAllocatedMemorySize,
                    this.MonitoringSurvivedMemorySize,
                    this.MonitoringSurvivedProcessMemorySize);
            return s;
        }
    }
}
