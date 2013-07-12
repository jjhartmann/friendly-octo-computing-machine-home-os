using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace HomeOS.Cloud.Portal.MvcWebRole.Models
{
    public class OrgInfo : TableEntity
    {
        public OrgInfo()
        {
            this.PartitionKey = "Organization";
        }

        [Required]
        [Display(Name = "Org ID")]
        public string OrgID
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

        [Required]
        [Display(Name = "Org Name")]
        public string OrgName { get; set; }

        [Required]
        [Display(Name = "Org Type")]
        public string OrgType { get; set; }

    }
}