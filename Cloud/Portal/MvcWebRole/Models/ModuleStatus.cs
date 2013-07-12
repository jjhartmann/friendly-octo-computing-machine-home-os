using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using HomeOS.Shared;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace HomeOS.Cloud.Portal.MvcWebRole.Models
{
    [DataContract]
    public class ModuleStatus : JsonSerializerBase<ModuleStatus>
    {
        public ModuleStatus()
        {
        }

        [Required]
        [Display(Name = "Module")]
        [DataMember]
        public string ModuleName { get; set; }

        [Required]
        [Display(Name = "Proc Time")]
        [DataMember]
        public string TotalProcessorTime { get; set; }

        [Required]
        [Display(Name = "Mem Alloc")]
        [DataMember]
        public string TotalAllocatedMemorySize { get; set; }

        [Required]
        [Display(Name = "Mem Survived")]
        [DataMember]
        public string SurvivedMemorySize { get; set; }
    }
}