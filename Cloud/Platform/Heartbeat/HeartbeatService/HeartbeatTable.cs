using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.ServiceRuntime;
using HomeOS.Shared;
using System.Linq;
using System.Web;

namespace HomeOS.Cloud.Platform.Heartbeat
{
    public class HeartbeatTable
    {
        protected const string HeartbeatEntityTableName = "HubHeartbeats";

        protected CloudStorageAccount storageAccount;
        protected CloudTableClient tableClient;
        protected CloudTable heartbeatsTable;

        public class HeartbeatEntity : TableEntity
        {
            public const string partitionKeyPropertyName = "PartitionKey";
            public const string rowKeyPropertyName = "RowKey";
            public const string timeStampPropertyName = "TimeStamp";

            private const string heartbeatPropertyName = "HeartbeatInfo";
            public HeartbeatEntity(string partitionKey, string key)
            {
                base.PartitionKey = partitionKey;
                base.RowKey = key;
            }

            public HeartbeatEntity()
            {
            }

            public string HeartbeatInfo { get; set; }
        }

        public HeartbeatTable()
        {
            // Retrieve the storage account from the connection string.

            if (RoleEnvironment.IsEmulated)
            {
                this.storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            }
            else
            {
                // enable for local machine testing
                this.storageAccount = CloudStorageAccount.Parse(
                    Microsoft.WindowsAzure.CloudConfigurationManager.GetSetting("StorageConnectionString"));
            }

            // Create the table client.
            this.tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "heartbeats" table.
            this.heartbeatsTable = tableClient.GetTableReference(HeartbeatEntityTableName);

            // Create the table if this is the first time we are using the table
            this.heartbeatsTable.CreateIfNotExists();
        }

        public void Write(string partitionKey, string key, string heartbeatInfo)
        {
            try
            {
                HeartbeatEntity heartbeatEntity = new HeartbeatEntity(partitionKey, key);

                heartbeatEntity.HeartbeatInfo = heartbeatInfo;

                // Create the TableOperation that inserts the heartbeat entity.
                TableOperation insertOperation = TableOperation.InsertOrReplace(heartbeatEntity);

                // Execute the insert operation.
                this.heartbeatsTable.Execute(insertOperation);
            }
            catch (Exception e)
            {
                Helper.Trace().WriteLine(String.Format("Writing to Azure Table failed with exception:{0}\n InnerException:{1}",
                                                        e.Message, null != e.InnerException ? e.InnerException.ToString() : null));
            }
        }

        public HeartbeatEntity Read(string partitionKey, string key)
        {
            HeartbeatEntity hbe = null;
            try
            {
                // Create a retrieve operation that takes a customer entity.
                TableOperation retrieveOperation = TableOperation.Retrieve<HeartbeatEntity>(partitionKey, key);

                // Execute the retrieve operation.
                TableResult retrievedResult = this.heartbeatsTable.Execute(retrieveOperation);

                if (null != retrievedResult)
                {
                    hbe = (HeartbeatEntity)retrievedResult.Result;
                }
            }
            catch (Exception e)
            {
                Helper.Trace().WriteLine(String.Format("Reading from Azure Table failed with exception:{0}\n InnerException:{1}",
                                                        e.Message, null != e.InnerException ? e.InnerException.ToString() : null));
            }

            return hbe; 
        }

        public List<HeartbeatEntity> GetHeartbeatEntitiesForOrg(string partitionKey)
        {
            List<HeartbeatEntity> heartbeatEntityList = new List<HeartbeatEntity>();
            try
            {
                string equalParititionKeyFilterCondition = TableQuery.GenerateFilterCondition(HeartbeatEntity.partitionKeyPropertyName, QueryComparisons.Equal, partitionKey);

                // Create the table query.
                TableQuery<HeartbeatEntity> rangeQuery = new TableQuery<HeartbeatEntity>().Where(equalParititionKeyFilterCondition);

                // Loop through the results, displaying information about the entity.
                foreach (HeartbeatEntity entity in this.heartbeatsTable.ExecuteQuery(rangeQuery))
                {
                    heartbeatEntityList.Add(entity);
                }
            }
            catch (Exception e)
            {
                Helper.Trace().WriteLine(String.Format("Querying from Azure Table failed with exception:{0}\n InnerException:{1}",
                                                        e.Message, null != e.InnerException ? e.InnerException.ToString() : null));
            }

            return heartbeatEntityList;
        }

        public List<HeartbeatEntity> GetHeartbeatEntitiesInTimeRange(string partitionKey, DateTimeOffset dto1, DateTimeOffset dto2)
        {
            List<HeartbeatEntity> heartbeatEntityList = new List<HeartbeatEntity>();
            try
            {
                string greaterThanEqualKey1FilterCondition = TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(HeartbeatEntity.partitionKeyPropertyName, QueryComparisons.Equal, partitionKey),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition(HeartbeatEntity.timeStampPropertyName, QueryComparisons.GreaterThanOrEqual, dto1.ToUniversalTime().ToString()));
                string lessThanEqualKey2FilterCondition = TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(HeartbeatEntity.partitionKeyPropertyName, QueryComparisons.Equal, partitionKey),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition(HeartbeatEntity.timeStampPropertyName, QueryComparisons.LessThanOrEqual, dto2.ToUniversalTime().ToString()));

                // Create the table query.
                TableQuery<HeartbeatEntity> rangeQuery = new TableQuery<HeartbeatEntity>().Where(
                        TableQuery.CombineFilters(greaterThanEqualKey1FilterCondition,
                        TableOperators.And,
                        lessThanEqualKey2FilterCondition));

                // Loop through the results, displaying information about the entity.
                foreach (HeartbeatEntity entity in this.heartbeatsTable.ExecuteQuery(rangeQuery))
                {
                    heartbeatEntityList.Add(entity);
                }
            }
            catch (Exception e)
            {
                Helper.Trace().WriteLine(String.Format("Querying from Azure Table failed with exception:{0}\n InnerException:{1}",
                                                        e.Message, null != e.InnerException ? e.InnerException.ToString() : null));
            }

            return heartbeatEntityList;
        }
    }
}