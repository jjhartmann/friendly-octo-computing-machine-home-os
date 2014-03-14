using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace HomeOS.Shared
{
    [DataContract]
    public class ScoutInfo : JsonSerializerBase<ScoutInfo>
    {
        [DataMember(Name = "ScoutFriendlyName")]
        public string ScoutFriendlyName { get; set; }
        [DataMember(Name = "ScoutVersion")]
        public string ScoutVersion { get; set; }

        public override string ToString()
        {
            string s = string.Format("scoutName:{0,15}, scoutVersion:{1,15}",
                    this.ScoutFriendlyName,
                    this.ScoutVersion);
            return s;
        }
    }
}
