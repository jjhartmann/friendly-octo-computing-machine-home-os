using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using HomeOS.Cloud.Portal.MvcWebRole.Models;
using System.Diagnostics;

namespace AzureHubManagementControllers
{
    public class HubStatusController : Controller
    {
        private CloudTable hubStatusTable;
        private CloudTable OrgInfoTable;

        public HubStatusController()
        {
            var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));

            var tableClient = storageAccount.CreateCloudTableClient();
            hubStatusTable = tableClient.GetTableReference("HubStatus");
            OrgInfoTable = tableClient.GetTableReference("OrgInfo");
        }

        private HubStatus FindRow(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<HubStatus>(partitionKey, rowKey);
            var retrievedResult = hubStatusTable.Execute(retrieveOperation);
            var HubStatus = retrievedResult.Result as HubStatus;
            if (HubStatus == null)
            {
                throw new Exception("No Hub found for: " + partitionKey);
            }

            return HubStatus;
        }

        public List<OrgInfo> GetOrgInfoList()
        {
            TableRequestOptions reqOptions = new TableRequestOptions()
            {
                MaximumExecutionTime = TimeSpan.FromSeconds(1.5),
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3)
            };
            List<OrgInfo> list = null;
            try
            {
                var query = new TableQuery<OrgInfo>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Organization"));
                list = OrgInfoTable.ExecuteQuery(query, reqOptions).ToList();
            }
            catch (StorageException se)
            {
                Trace.TraceError(se.Message);
            }

            return list;
        }

        private HubStatusEx MakeHubStatusEx(HubStatus hs, List<OrgInfo> orgListCached)
        {
            HubStatusEx hsEx = new HubStatusEx(hs);
            List<OrgInfo> orgList = orgListCached != null ? orgListCached : GetOrgInfoList();
            hsEx.OrgList = orgList;
            List<ModuleStatus> moduleStatusList = hsEx.ModuleStatusList;
            return hsEx;
        }

        private List<HubStatusEx> MakeHubStatusExList(List<HubStatus> listHubStatus, List<OrgInfo> orgListCached)
        {
            // Generate a HubStatusEx list from HubStatus so that right view can be created
            List<HubStatusEx> listHubStatusEx = new List<HubStatusEx>();
            foreach (HubStatus hs in listHubStatus)
            {
                listHubStatusEx.Add(MakeHubStatusEx(hs, orgListCached));
            }

            return listHubStatusEx;
        }


        //
        // GET: /HubStatus/Organization/{orgId}

        public ActionResult Organization(string orgId)
        {
            return RedirectToAction("IndexForOrg", new { orgid = orgId });            
        }

        //
        // GET: /Hubstatus
        public ActionResult Index()
        {
            return IndexForOrg(null);
        }

        //
        // GET: /HubStatus/{orgId}

        public ActionResult IndexForOrg(string orgid)
        {
            TableRequestOptions reqOptions = new TableRequestOptions()
            {
                MaximumExecutionTime = TimeSpan.FromSeconds(1.5),
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3)
            };
            List<HubStatus> listHubStatus;
            List<OrgInfo> orgInfoList = GetOrgInfoList();
            OrgInfo selectedOrg = null;
            foreach (OrgInfo oi in orgInfoList)
            {
                if (oi.OrgID == orgid)
                {
                    selectedOrg = oi;
                    break;
                }
            }

            try
            {
                if (null == selectedOrg)
                {
                    selectedOrg = orgInfoList.Count != 0 ? orgInfoList[0] : new OrgInfo() { OrgID = "unknown", OrgName = "unknown", OrgType = "unknown" };
                }
                var query = new TableQuery<HubStatus>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, selectedOrg.OrgID));
                listHubStatus = hubStatusTable.ExecuteQuery(query, reqOptions).ToList();
            }
            catch (StorageException se)
            {
                ViewBag.errorMessage = "Timeout error, try again. ";
                Trace.TraceError(se.Message);
                return View("Error");
            }

            return View(MakeHubStatusExList(listHubStatus, orgInfoList));
        }

        //
        // GET: /HubStatus/Details/5

        public ActionResult Details(string partitionKey, string rowKey)
        {
            HubStatus hubStatus = FindRow(partitionKey, rowKey);
            return View(MakeHubStatusEx(hubStatus, GetOrgInfoList()));
        }

    }
}