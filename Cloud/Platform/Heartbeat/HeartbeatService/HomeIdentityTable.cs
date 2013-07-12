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
    public class HomeIdentityTable
    {
        protected const string HomeIdentityEntityTableName = "HomeIdentity";

        protected CloudStorageAccount storageAccount;
        protected CloudTableClient tableClient;
        protected CloudTable homeIdentityTable;

        public class HomeIdentityEntity : TableEntity
        {
            public const string partitionKeyPropertyName = "PartitionKey";
            public const string rowKeyPropertyName = "RowKey";
            public const string timeStampPropertyName = "TimeStamp";

            public HomeIdentityEntity(string partitionKey, string key)
            {
                base.PartitionKey = partitionKey;
                base.RowKey = key;
            }

            public HomeIdentityEntity()
            {
            }
        }

        public HomeIdentityTable()
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

            // Create the CloudTable object that represents the "homeIdentity" table.
            this.homeIdentityTable = tableClient.GetTableReference(HomeIdentityEntityTableName);

            // Create the table if this is the first time we are using the table
            this.homeIdentityTable.CreateIfNotExists();
        }

        public bool IsHomeIdentityPresent(string hardwareId, string homeId)
        {
            bool IsPresent = false;
            try
            {
                string FilterCondition = TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(HomeIdentityEntity.partitionKeyPropertyName, QueryComparisons.Equal, hardwareId),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition(HomeIdentityEntity.rowKeyPropertyName, QueryComparisons.Equal, homeId));
                // Create the table query.
                TableQuery<HomeIdentityEntity> rangeQuery = new TableQuery<HomeIdentityEntity>().Where(FilterCondition);
                IsPresent = this.homeIdentityTable.ExecuteQuery(rangeQuery).Count<HomeIdentityEntity>() != 0;
            }
            catch(Exception e) 
            {
                Helper.Trace().WriteLine(String.Format("Querying from Azure Table failed with exception:{0}\n InnerException:{1}",
                                                        e.Message, null != e.InnerException ? e.InnerException.ToString() : null));
            }

            return IsPresent;
        }

        public void AddHomeIdentity(string hardwareId, string homeId)
        {
            try
            {
                HomeIdentityEntity homeIdentityEntity = new HomeIdentityEntity(hardwareId, homeId);

                // Create the TableOperation that inserts the homeIdentity entity.
                TableOperation insertOperation = TableOperation.Insert(homeIdentityEntity);

                // Execute the insert operation.
                this.homeIdentityTable.Execute(insertOperation);
            }
            catch (Exception e)
            {
                Helper.Trace().WriteLine(String.Format("Adding entry to HomeIdentity Table failed with exception:{0}\n InnerException:{1}",
                                                        e.Message, null != e.InnerException ? e.InnerException.ToString() : null));
            }
        }

    }
}