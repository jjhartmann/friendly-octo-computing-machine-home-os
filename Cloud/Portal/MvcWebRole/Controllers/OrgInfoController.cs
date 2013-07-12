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
    public class OrgInfoController : Controller
    {
        private CloudTable OrgInfoTable;

        public OrgInfoController()
        {
            var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
            // If this is running in a Windows Azure Web Site (not a Cloud Service) use the Web.config file:
            //    var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

            var tableClient = storageAccount.CreateCloudTableClient();
            OrgInfoTable = tableClient.GetTableReference("OrgInfo");
        }

        private OrgInfo FindRow(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<OrgInfo>(partitionKey, rowKey);
            var retrievedResult = OrgInfoTable.Execute(retrieveOperation);
            var OrgInfo = retrievedResult.Result as OrgInfo;
            if (OrgInfo == null)
            {
                throw new Exception("No Hub found for: " + partitionKey);
            }

            return OrgInfo;
        }

        //
        // GET: /OrgInfo/

        public ActionResult Index()
        {
            TableRequestOptions reqOptions = new TableRequestOptions()
            {
                MaximumExecutionTime = TimeSpan.FromSeconds(1.5),
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3)
            };
            List<OrgInfo> lists;
            try
            {
                var query = new TableQuery<OrgInfo>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Organization"));
                lists = OrgInfoTable.ExecuteQuery(query, reqOptions).ToList();
            }
            catch (StorageException se)
            {
                ViewBag.errorMessage = "Timeout error, try again. ";
                Trace.TraceError(se.Message);
                return View("Error");
            }

            return View(lists);
        }

        //
        // GET: /OrgInfo/AddNew

        public ActionResult AddNew()
        {
            return View();
        }

        //
        // POST: /OrgInfo/AddNew

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNew(OrgInfo OrgInfo)
        {
            if (ModelState.IsValid)
            {
                var insertOperation = TableOperation.Insert(OrgInfo);
                try
                {
                    OrgInfoTable.Execute(insertOperation);
                }
                catch (StorageException se)
                {
                    ViewBag.errorMessage = "HomeID already taken, try another name.";
                    Trace.TraceError(se.Message);
                    return View("Error");
                }
                return RedirectToAction("Index");
            }

            return View(OrgInfo);
        }

        //
        // GET: /OrgInfo/Edit/5

        public ActionResult Edit(string partitionKey, string rowKey)
        {
            var OrgInfo = FindRow(partitionKey, rowKey);
            return View(OrgInfo);
        }

        //
        // POST: /OrgInfo/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string partitionKey, string rowKey, OrgInfo editedOrgInfo)
        {
            if (ModelState.IsValid)
            {
                var OrgInfo = new OrgInfo();
                UpdateModel(OrgInfo);
                try
                {
                    var replaceOperation = TableOperation.Replace(OrgInfo);
                    OrgInfoTable.Execute(replaceOperation);
                    return RedirectToAction("Index");
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode == 412)
                    {
                        // Concurrency error
                        var currentOrgInfo = FindRow(partitionKey, rowKey);
                        if (currentOrgInfo.OrgID != editedOrgInfo.OrgID)
                        {
                            ModelState.AddModelError("OrgID", "Current value: " + currentOrgInfo.OrgID);
                        }
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                            + "was modified by another user after you got the original value. The "
                            + "edit operation was canceled and the current values in the database "
                            + "have been displayed. If you still want to edit this record, click "
                            + "the Save button again. Otherwise click the Back to List hyperlink.");
                         ModelState.SetModelValue("ETag", new ValueProviderResult(currentOrgInfo.ETag, currentOrgInfo.ETag, null));
                    }
                    else
                    {
                        throw; 
                    }
                }
            }
            return View(editedOrgInfo);
        }

        //
        // GET: /OrgInfo/Delete/5

        public ActionResult Delete(string partitionKey, string rowKey)
        {
            var OrgInfo = FindRow(partitionKey, rowKey);
            return View(OrgInfo);
        }

        //
        // POST: /OrgInfo/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string partitionKey)
        {
            // Delete all rows for this mailing list, that is, 
            // Subscriber rows as well as OrgInfo rows.
            // Therefore, no need to specify row key.
            var query = new TableQuery<OrgInfo>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            var listRows = OrgInfoTable.ExecuteQuery(query).ToList();
            var batchOperation = new TableBatchOperation();
            int itemsInBatch = 0;
            foreach (OrgInfo listRow in listRows)
            {
                batchOperation.Delete(listRow);
                itemsInBatch++;
                if (itemsInBatch == 100)
                {
                    OrgInfoTable.ExecuteBatch(batchOperation);
                    itemsInBatch = 0;
                    batchOperation = new TableBatchOperation();
                }
            }
            if (itemsInBatch > 0)
            {
                OrgInfoTable.ExecuteBatch(batchOperation);
            }
            return RedirectToAction("Index");
        }

        //
        // GET: /OrgInfo/Details/5

        public ActionResult Details(string partitionKey, string rowKey)
        {
            OrgInfo OrgInfo = FindRow(partitionKey, rowKey);
            return View(OrgInfo);
        }

    }
}