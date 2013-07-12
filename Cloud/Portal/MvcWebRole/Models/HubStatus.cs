using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace HomeOS.Cloud.Portal.MvcWebRole.Models
{
    public class HubStatus : TableEntity
    {
        public HubStatus()
        {
        }

        [Required]
        [Display(Name = "Org ID")]
        public string OrgID
        {
            get
            {
                return this.PartitionKey;
            }
            set
            {
                this.PartitionKey = value;
            }
        }

        [Required]
        [Display(Name = "Home ID")]
        public string HomeID
        {
            get
            {
                return this.RowKey;
            }
            set
            {
                this.RowKey = value;
            }
        }

        [Display(Name = "Study ID")]
        [DisplayFormat(NullDisplayText = "'Not Available'")]
        public string StudyID { get; set; }

        [Display(Name = "Status")]
        [DisplayFormat(NullDisplayText = "'Not Available'")]
        public string Status { get; set; }

        [Display(Name = "Heartbeat Received(UTC)")]
        [DisplayFormat(NullDisplayText = "'Not Available'")]
        public string LastHeartbeatReported { get; set; }

        [Display(Name = "Heartbeat Sent(UTC)")]
        [DisplayFormat(NullDisplayText = "'Not Available'")]
        public string HubTimeStamp { get; set; }

        [Display(Name = "Last Heartbeat Seq Num")]
        [DisplayFormat(NullDisplayText = "'Not Available'")]
        public string LastHeartbeatSequenceNumber { get; set; }

        [Display(Name = "Expected Heartbeat Interval (mins)")]
        [DisplayFormat(NullDisplayText = "'Not Available'")]
        public string ExpectedHeartbeatIntervalInMins { get; set; }

        [Display(Name = "Current Heartbeat Interval (mins)")]
        [DisplayFormat(NullDisplayText = "'Not Available'")]
        public string CurrentHeartbeatIntervalInMins { get; set; }

        [Display(Name = "Memory (Bytes)")]
        [DisplayFormat(NullDisplayText = "'Not Available'")]
        public string Memory { get; set; }

        [Display(Name = "CPU %")]
        [DisplayFormat(NullDisplayText = "'Not Available'")]
        public string CPU { get; set; }

        [Display(Name = "Module Status Info")]
        [DisplayFormat(NullDisplayText = "'Not Available'")]
        public string ModuleStatusListAsJson{ get; set; }
    }
}