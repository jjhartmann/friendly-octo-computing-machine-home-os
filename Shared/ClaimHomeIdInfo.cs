using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace HomeOS.Shared
{
    [DataContract]
    public class ClaimHomeIdInfo : JsonSerializerBase<ClaimHomeIdInfo>
    {
        [DataMember(Name = "HardwareId")]
        public string HardwareId { get; set; }
        [DataMember(Name = "HomeId")]
        public String HomeId { get; set; }

        public override string ToString()
        {
            string s = string.Format("HardwareId:{0}, HomeId:{1}",
                    this.HardwareId,
                    this.HomeId);
            return s;
        }
    }
}
