using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace HomeOS.Shared
{
    [DataContract]
    public class WatchdogMsgInfo : JsonSerializerBase<WatchdogMsgInfo>
    {
        [DataMember(Name = "HardwareId")]
        public string HardwareId { get; set; }
        [DataMember(Name = "HubTimestamp")]
        public String HubTimestamp { get; set; }
        [DataMember(Name = "SequenceNumber")]
        public UInt32 SequenceNumber { get; set; }
        [DataMember(Name = "MessageText")]
        public string MessageText { get; set; }

        public WatchdogMsgInfo(string hwId, string hubTime, uint sequenceNumber, string messageText)
        {
            this.HardwareId = hwId;
            this.HubTimestamp = hubTime;
            this.SequenceNumber = sequenceNumber;
            this.MessageText = messageText;
        }

        public override string ToString()
        {
            return string.Format("HwId: {0} Timestamp: {1} SeqNo: {2} Msg:{3}", HardwareId, HubTimestamp, SequenceNumber, MessageText);
        }
    }
}
