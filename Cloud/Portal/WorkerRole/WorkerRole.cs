using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using HomeOS.Cloud.Portal.MvcWebRole.Models;
using HomeOS.Shared;

namespace HomeOS.Cloud.Portal.WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        [DataContract]
        private class GetHeartbeatInfoRangeByOrgResultWrapped
        {
            [DataMember(Name = "GetHeartbeatInfoRangeByOrgResult")]
            public List<Tuple<string /* orgId */, string /* homeId */, DateTime /* UtcTime */, HeartbeatInfo>> GetHeartbeatInfoRangeByOrgResult { get; set; }
        }

        private const uint HeartbeatCheckFrequencyInMins = 1;
        private CloudTable hubStatusTable;
        private CloudTable orgInfoTable;
        private volatile bool onStopCalled = false;
        private volatile bool returnedFromRunMethod = false;

        private string GetHeartbeatServiceHostString(bool Emulation)
        {
            string retString;
            if (Emulation)
            {
                retString = Constants.HeartbeatServiceEmulationHost;
            }
            else
            {
                retString = ConfigurationManager.AppSettings.Get("HeartbeatMonitorServiceHost");

            }
            return retString;
        }

        public override void Run()
        {
            Trace.TraceInformation("HomeOS.Cloud.Portal.WorkerRole entering Run()");
            while (true)
            {
                // check for the orgs being monitored, and update their status with the appropriate values received from the latest heartbeat info
                TableRequestOptions reqOptions = new TableRequestOptions()
                {
                    MaximumExecutionTime = TimeSpan.FromSeconds(1.5),
                    RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3)
                };

                List<OrgInfo> listsOrgInfo;
                try
                {
                    var query = new TableQuery<OrgInfo>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Organization"));
                    listsOrgInfo = orgInfoTable.ExecuteQuery(query, reqOptions).ToList();
                }
                catch (StorageException se)
                {
                    Trace.TraceError(se.Message);
                    return;
                }
                foreach (OrgInfo orgInfo in listsOrgInfo)
                {
                    try
                    {
                        // If OnStop has been called, return to do a graceful shutdown.
                        if (onStopCalled == true)
                        {
                            Trace.TraceInformation("onStopCalled HomeOS.Cloud.Portal.WorkerRole");
                            returnedFromRunMethod = true;
                            return;
                        }

                        Trace.TraceInformation("HomeOS.Cloud.Portal.WorkerRole requesting heartbeat");
                        WebClient webClient = new WebClient();
                        webClient.UploadStringCompleted += webClient_UploadStringCompleted;
                        webClient.Headers["Content-type"] = "application/json";
                        webClient.Encoding = Encoding.UTF8;
                        // scan twice the time window at which we are checking to be safe, so looking back two minutes...
                        string jsonDataPost = String.Format("{{\"orgId\": \"{0}\"}}", orgInfo.OrgID);
                        Uri uri = new Uri("http://" + GetHeartbeatServiceHostString(RoleEnvironment.IsEmulated) + ":" + Constants.HeartbeatServicePort + "/" +
                                      Constants.HeartbeatServiceWcftMonitorEndPointUrlSuffix + "/GetHeartbeatInfoRangeByOrg");

                        webClient.UploadStringAsync(uri, "POST", jsonDataPost, jsonDataPost);


                        System.Threading.Thread.Sleep(new TimeSpan(0, 0, 0, 0, (int)HeartbeatCheckFrequencyInMins * 60 * 1000 /* msecs */));
                    }
                    catch (Exception ex)
                    {
                        string err = ex.Message;
                        if (ex.InnerException != null)
                        {
                            err += " Inner Exception: " + ex.InnerException.Message;
                        }
                        Trace.TraceError(err);
                    }
                }
            }
        }

        void webClient_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            GetHeartbeatInfoRangeByOrgResultWrapped hbiListWrapped;

            if (e.Error != null)
            {
                Trace.TraceError("Heartbeat info request for {0} failed with Server error : {1}", (String)e.UserState, e.Error.Message);
                return;
            }
            Trace.TraceInformation("Heartbeat info request for {0} received  successfully", (String)e.UserState);
            try
            {
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(e.Result));
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(GetHeartbeatInfoRangeByOrgResultWrapped));
                hbiListWrapped = ser.ReadObject(ms) as GetHeartbeatInfoRangeByOrgResultWrapped;
                Trace.TraceInformation("Found {0} Heartbeats", hbiListWrapped.GetHeartbeatInfoRangeByOrgResult.Count);
                ms.Close();
            }
            catch (Exception e1)
            {
                Trace.TraceError("Failed to deserialize the Heartbeat Info key value list obtained by GetHeartbeatInfoRangeByOrg, Error={0}", !String.IsNullOrEmpty(e1.Message) ? e1.Message : e1.InnerException.ToString());
                return;
            }

            MergeHeartbeatEntitiesIntoHubStatusEntitiesByOrg(hbiListWrapped.GetHeartbeatInfoRangeByOrgResult);
        }

        private string ConvertModuleMonitorInfoListToModuleStatusListAsJson(List<ModuleMonitorInfo> moduleMonitorInfoList)
        {
            string moduleStatusListJson = "{\"ModuleStatusList\":[";
            foreach (ModuleMonitorInfo mi in moduleMonitorInfoList)
            {
                ModuleStatus moduleStatus = new ModuleStatus()
                            {
                                ModuleName = mi.ModuleFriendlyName,
                                SurvivedMemorySize = mi.MonitoringSurvivedMemorySize.ToString(),
                                TotalAllocatedMemorySize = mi.MonitoringTotalAllocatedMemorySize.ToString(),
                                TotalProcessorTime = mi.MonitoringTotalProcessorTime.ToString()
                            };
                moduleStatusListJson += moduleStatus.SerializeToJsonStream();
                moduleStatusListJson += ",";
            }
            moduleStatusListJson += "]}";
            return moduleStatusListJson;
        }

        private void MergeHeartbeatEntitiesIntoHubStatusEntitiesByOrg(List<Tuple<string /* orgId */, string /* homeId */, DateTime /* UtcTime */, HeartbeatInfo>> tuplesHeartbeat)
        {
            foreach (Tuple<string /* orgId */, string /* homeId */, DateTime /* UtcTime */, HeartbeatInfo> tupleHeartbeat in tuplesHeartbeat)
            {
                bool hubentityPresent = false;
                bool heartbeatUpdated = false;
                TableRequestOptions reqOptions = new TableRequestOptions()
                {
                    MaximumExecutionTime = TimeSpan.FromSeconds(1.5),
                    RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3)
                };

                List<HubStatus> listsHubStatus;
                HubStatus hubStatus;
                try
                {
                    var query = new TableQuery<HubStatus>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tupleHeartbeat.Item1));
                    listsHubStatus = hubStatusTable.ExecuteQuery(query, reqOptions).ToList();
                }
                catch (StorageException se)
                {
                    Trace.TraceError(se.Message);
                    return;
                }

                try
                {
                    hubStatus = listsHubStatus.Where(hb => hb.HomeID == tupleHeartbeat.Item2).First();
                }
                catch(Exception)
                {
                    Trace.TraceWarning("MergeHeartbeatEntitiesIntoHubStatusEntitiesByOrg - Home ID not found in HubStatus Table !");
                    hubStatus = null;
                }
                if (null != hubStatus)
                {
                    hubentityPresent = true;
                }

                if (!hubentityPresent)
                {
                    string moduleStatusListJson = ConvertModuleMonitorInfoListToModuleStatusListAsJson(tupleHeartbeat.Item4.ModuleMonitorInfoList);
                    TableOperation updateOperation = TableOperation.Insert(new HubStatus()
                    {
                        ETag="*",
                        OrgID = tupleHeartbeat.Item1,
                        HomeID = tupleHeartbeat.Item2,
                        StudyID = tupleHeartbeat.Item4.StudyId,
                        Status = "online",
                        LastHeartbeatReported = tupleHeartbeat.Item3.ToString(),
                        HubTimeStamp = tupleHeartbeat.Item4.HubTimestamp,
                        LastHeartbeatSequenceNumber = tupleHeartbeat.Item4.SequenceNumber.ToString(),
                        ExpectedHeartbeatIntervalInMins = String.Format("{0}", tupleHeartbeat.Item4.HeartbeatIntervalMins),
                        Memory = String.Format("{0}", tupleHeartbeat.Item4.PhysicalMemoryBytes),
                        CPU = String.Format("{0:0.00}", tupleHeartbeat.Item4.TotalCpuPercentage),
                        ModuleStatusListAsJson = moduleStatusListJson
                    });
                    try
                    {
                        hubStatusTable.Execute(updateOperation);
                    }
                    catch (StorageException se)
                    {
                        Trace.TraceError(se.Message);
                        continue;
                    }
                }
                else
                {

                    // calculate the average heart beat interval in minutes and the online status of this hub
                    if (!hubStatus.LastHeartbeatReported.Equals(tupleHeartbeat.Item3.ToString()))
                    {
                        heartbeatUpdated = true;
                    }

                    double currentAvgHeartbeatIntervalInMins = 0.0;
                    double.TryParse(hubStatus.CurrentHeartbeatIntervalInMins, out currentAvgHeartbeatIntervalInMins);

                    double expectedAvgHeartbeatIntervalInMins = 0.0;
                    double.TryParse(hubStatus.ExpectedHeartbeatIntervalInMins, out expectedAvgHeartbeatIntervalInMins);

                    // if no new heartbeat has been receivied for this hub since the last time we checked, just update
                    // heartbeat average interval 

                    if (!heartbeatUpdated)
                    {
                        currentAvgHeartbeatIntervalInMins += HeartbeatCheckFrequencyInMins;

                        bool statusOnline = hubStatus.Status == "online";
                        if (expectedAvgHeartbeatIntervalInMins > 0.0 && currentAvgHeartbeatIntervalInMins > Constants.MaxHeartbeatIntervalInMins)
                        {
                            statusOnline = false;
                        }

                        TableOperation updateOperation = TableOperation.Merge(new HubStatus()
                        {
                            ETag = "*",
                            OrgID = tupleHeartbeat.Item1,
                            HomeID = tupleHeartbeat.Item2,
                            CurrentHeartbeatIntervalInMins = String.Format("{0:0.##}", currentAvgHeartbeatIntervalInMins),
                            Status = statusOnline ? "online" : "offline"
                        });
                        try
                        {
                            hubStatusTable.Execute(updateOperation);
                        }
                        catch (StorageException se)
                        {
                            Trace.TraceError(se.Message);
                            continue;
                        }
                    }
                    else
                    {
                        double heartbeatIntervalInMinutes = 0.0;
                        DateTime lastHeartbeatReported = DateTime.MinValue;
                        if (DateTime.TryParse(hubStatus.LastHeartbeatReported, out lastHeartbeatReported))
                        {
                            TimeSpan heartbeatInterval = tupleHeartbeat.Item3 - lastHeartbeatReported;
                            heartbeatIntervalInMinutes = heartbeatInterval.Days * 24 * 60 + heartbeatInterval.Hours * 60 + heartbeatInterval.Minutes + heartbeatInterval.Seconds / 60.0;
                            if (currentAvgHeartbeatIntervalInMins > 0.0)
                            {
                                currentAvgHeartbeatIntervalInMins = (currentAvgHeartbeatIntervalInMins + heartbeatIntervalInMinutes) / 2.0;
                            }
                            else
                            {
                                currentAvgHeartbeatIntervalInMins = heartbeatIntervalInMinutes;
                            }
                        }
                        else
                        {
                            Trace.TraceWarning("HubStatus LastHeartbeatReported is invalid!");
                        }

                        string moduleStatusListJson = ConvertModuleMonitorInfoListToModuleStatusListAsJson(tupleHeartbeat.Item4.ModuleMonitorInfoList);
                        TableOperation updateOperation = TableOperation.Merge(new HubStatus()
                        {
                            ETag = "*",
                            OrgID = tupleHeartbeat.Item1,
                            HomeID = tupleHeartbeat.Item2,
                            StudyID = tupleHeartbeat.Item4.StudyId, // in case  updated by hub
                            Status = "online",
                            LastHeartbeatReported = tupleHeartbeat.Item3.ToString(),
                            HubTimeStamp = tupleHeartbeat.Item4.HubTimestamp,
                            LastHeartbeatSequenceNumber = tupleHeartbeat.Item4.SequenceNumber.ToString(),
                            ExpectedHeartbeatIntervalInMins = String.Format("{0}", tupleHeartbeat.Item4.HeartbeatIntervalMins), // in case updated by hub
                            CurrentHeartbeatIntervalInMins = String.Format("{0:0.##}", currentAvgHeartbeatIntervalInMins),
                            Memory = String.Format("{0}", tupleHeartbeat.Item4.PhysicalMemoryBytes),
                            CPU = String.Format("{0:0.##}", tupleHeartbeat.Item4.TotalCpuPercentage),
                            ModuleStatusListAsJson = moduleStatusListJson
                        });
                        try
                        {
                            hubStatusTable.Execute(updateOperation);
                        }
                        catch (StorageException se)
                        {
                            Trace.TraceError(se.Message);
                            continue;
                        }
                    }

                }

            }
        }

        private void ConfigureDiagnostics()
        {
            DiagnosticMonitorConfiguration config = DiagnosticMonitor.GetDefaultInitialConfiguration();
            config.ConfigurationChangePollInterval = TimeSpan.FromMinutes(1d);
            config.Logs.BufferQuotaInMB = 500;
            config.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;
            config.Logs.ScheduledTransferPeriod = TimeSpan.FromMinutes(1d);

            //DiagnosticMonitor.Start(
            //    "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString",
            //    config);
        }

        public override void OnStop()
        {
            onStopCalled = true;
            while (returnedFromRunMethod == false)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount;

            ConfigureDiagnostics();
            Trace.TraceInformation("Initializing storage account in HomeOS.Cloud.Portal.WorkerRole");
            var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));

            // Create tables if they don't exist
            var tableClient = storageAccount.CreateCloudTableClient();
            hubStatusTable = tableClient.GetTableReference("HubStatus");
            hubStatusTable.CreateIfNotExists();
            orgInfoTable = tableClient.GetTableReference("OrgInfo");
            orgInfoTable.CreateIfNotExists();

            return base.OnStart();
        }
    }
}
