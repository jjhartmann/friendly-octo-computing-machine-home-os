
namespace HomeOS.Hub.Platform
{
    using HomeOS.Hub.Common;
    using HomeOS.Hub.Platform.Views;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using Microsoft.WindowsAzure.StorageClient.Protocol;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using System.Xml;
   
    /// <summary>
    /// ConfigUpdater periodically uploads current config to an Azure Blob as a Zip 
    /// something happens here
    /// Platform is notified to re-initiate from the downloaded configuration.
    /// </summary>
    public sealed class ConfigUpdater
    {

        // Azure constants
        private const string DataStoreAccountName = "DataStoreAccountName";
        private const string DataStoreAccountKey="DataStoreAccountKey";
        private const string AzureConfigContainerName = "configs";

        private const int AzureBlobLeaseTimeout = 60; //seconds. Max amount of time needed for uploading data.



        private int frequency; // in milliseconds
        private Delegate methodToInvoke; // method in platform that is to be invoked if hash matches
        private TimerCallback tcb;
        private Timer timer;
        private static string temporaryZipLocation = Environment.CurrentDirectory + "\\temp" ; // temporary location where zip is stored
        private VLogger logger;
        
        
        private const string OldConfigSuffix = "_old";
        private const string NewConfigSuffix = "_new"; 

        private const string CurrentVersionFileName = ".currentversion";
        private const string ParentVersionFileName = ".parentversion";
        private const string CurrentConfigZipName = "actualconfig.zip";
        private const string DownloadeConfigZipName = "desiredconfig.zip";

        private ServiceHost serviceHost;
        private UpdateStatus status; // for reporting to the web service

        private Configuration config; 

        public ConfigUpdater(Configuration config, VLogger log , int frequency, Delegate method)
        {
            this.config = config;
            this.logger = log; 
            this.frequency = frequency;
            this.methodToInvoke = method;
            tcb = ConfigSync ;
            timer = new Timer(tcb, null, 500, frequency);

                if (System.IO.Directory.Exists(temporaryZipLocation)) // creating temporary directory location for downloading and holding zips
                    Utils.CleanDirectory(logger, temporaryZipLocation);
                Utils.CreateDirectory(logger, temporaryZipLocation);

            this.status = new UpdateStatus(this.frequency); 

            ConfigUpdaterWebService webService = new ConfigUpdaterWebService(logger,this);

            string homeIdPart = "";
            if (!string.IsNullOrWhiteSpace(Settings.HomeId))
                homeIdPart = "/" + Settings.HomeId;

            string url =  Constants.InfoServiceAddress + homeIdPart +  "/config";
            serviceHost = ConfigUpdaterWebService.CreateServiceHost(webService, new Uri(url));
            serviceHost.Open();
            Utils.structuredLog(logger,"I", "ConfigUpdaterWebService initiated at "+url);

        }

        public void setConfig(Configuration config)
        {
            this.config = config;
        }

        private void ConfigSync(Object stateInfo)
        {
            Tuple<string, string> configToUpload;

            if (config == null)
            {
                Utils.structuredLog(logger,"W", "config is null; aborting ConfigSync");
                return;
            }

            status.lastConfigSync = DateTime.Now;
            Utils.structuredLog(logger,"I", "initiating ConfigSync");
            
            string AzureAccountName = config.GetConfSetting(DataStoreAccountName);
            string AzureAccountKey = config.GetConfSetting(DataStoreAccountKey);

            if (string.IsNullOrEmpty(AzureAccountKey) || string.IsNullOrEmpty(AzureAccountName))
            {
                Utils.structuredLog(logger,"E", "AzureAccountKey or AzureAccountName is null; aborting ConfigSync");
                return;
            }

     
                configToUpload = PrepareCurrentConfig();

                if (!string.IsNullOrEmpty(configToUpload.Item1))
                {
                    if (UploadConfig_Azure(configToUpload.Item1, AzureAccountName, AzureAccountKey))
                    {
                        status.lastConfigUpload = DateTime.Now;
                        status.versionUploaded = configToUpload.Item2;
                        Utils.structuredLog(logger,"ConfigUpload", ActualConfigBlobName(), configToUpload.Item2);
                    }
                    else
                        Utils.structuredLog(logger,"E", "config upload failed", ActualConfigBlobName(), configToUpload.Item2);
                }
            Utils.DeleteFile(logger,configToUpload.Item1);
        
            string downloadedConfigZipPath = temporaryZipLocation + "\\" + DownloadeConfigZipName;

            if (DownloadConfig_Azure(downloadedConfigZipPath, AzureAccountName, AzureAccountKey))
            {
        
                    status.lastConfigDownload = DateTime.Now;
                    Tuple<bool, string> configDeploy = ConfigReloadNeeded(downloadedConfigZipPath);
                    if (configDeploy.Item1)
                {

                    string tempConfigDir = Settings.ConfigDir +"\\..\\Config"+ NewConfigSuffix;
                    Utils.CleanDirectory(logger,tempConfigDir);

                    if (!Utils.UnpackZip(logger, downloadedConfigZipPath, tempConfigDir))
                            Utils.structuredLog(logger,"E", "unpacking failed", downloadedConfigZipPath);

                    Utils.CopyDirectory(logger,Settings.ConfigDir, Settings.ConfigDir + "\\..\\Config" + OldConfigSuffix);// stash away existing copy 
                    Utils.CopyDirectory(logger, tempConfigDir, Settings.ConfigDir);
                    Utils.DeleteFile(logger, downloadedConfigZipPath);
                    Utils.DeleteDirectory(logger, tempConfigDir);
                    Utils.DeleteDirectory(logger, Settings.ConfigDir + "\\..\\Config" + OldConfigSuffix);
                    methodToInvoke.DynamicInvoke(Settings.ConfigDir);
                    Utils.structuredLog(logger,"I", "config reloading");

                    // update status

                }
                else
                {
                    Utils.structuredLog(logger,"ER", "Config Reload Failed", configDeploy.Item2);
                    Utils.DeleteFile(logger, downloadedConfigZipPath);
                }
      
            }
            else
                Utils.structuredLog(logger,"ConfigDownload", "failed");

        }


        
        private Tuple<string, string> PrepareCurrentConfig()
        {
            bool zipPacked = false ;
            Dictionary<string, string> currentVersion; 
            lock (config) // lock config so that no one else can change it. then, compute the version, zip the files, copy zip a location and relinquish lock.
            {
                currentVersion = GetConfigVersion(Settings.ConfigDir);
                UpdateCurrentVersionFile(currentVersion);
                zipPacked = Utils.PackZip(logger,Settings.ConfigDir, temporaryZipLocation + "\\"+ CurrentConfigZipName);
            }

            if (zipPacked)
                return new Tuple<string, string>(temporaryZipLocation + "\\" + CurrentConfigZipName, ConvertVersionToString(currentVersion)) ;
            
            return new Tuple<string,string>("","");
        }

        private Tuple<bool, string> ConfigReloadNeeded(string downloadedConfigZip)
        {
            String newconfigdir = Utils.CreateDirectory(logger, downloadedConfigZip.Replace(".zip", ""));

            if (string.IsNullOrEmpty(newconfigdir))
                return new Tuple<bool, string>(false, "directory creation for unzipping downloaded config failed");

            if (!Utils.UnpackZip(logger, downloadedConfigZip, newconfigdir))
                return new Tuple<bool, string>(false, "unpacking of downloaded config failed");
            
            string downloadedVersion = ConvertVersionToString(GetConfigVersion(newconfigdir));
            status.versionDownloaded = downloadedVersion;
            if(File.Exists(newconfigdir+"\\"+CurrentVersionFileName))
            {
                string versionFilecontent = File.ReadAllText(newconfigdir+"\\" + CurrentVersionFileName);
                if (!versionFilecontent.Equals(downloadedVersion, StringComparison.CurrentCultureIgnoreCase))
                    Utils.structuredLog(logger,"W", "version file of downloaded config has INCORRECT version");
            }

            if (File.Exists(newconfigdir + "\\" + ParentVersionFileName))
            {
                string parentVersionFilecontent = File.ReadAllText(newconfigdir + "\\" + ParentVersionFileName);
                string currentVersion;
                
                lock (config)
                {
                    currentVersion = ConvertVersionToString(GetConfigVersion(Settings.ConfigDir));
                }

                if (currentVersion.Equals(ConvertVersionToString(GetConfigVersion(newconfigdir))))
                {
                    return new Tuple<bool, string>(false, "version of current config and downloaded config match");
                }
                Utils.DeleteDirectory(logger, newconfigdir);

                if (currentVersion.Equals(parentVersionFilecontent, StringComparison.CurrentCultureIgnoreCase))
                {
                    return new Tuple<bool, string>(true, "");
                }
                else
                {
                    return new Tuple<bool, string>(false, "parent of downloaded config does not match current config");
                }

            }

            Utils.DeleteDirectory(logger, newconfigdir);
            return new Tuple<bool, string>(false, "downloaded config does not have parent config version file");
        }

        private bool UploadConfig_Azure(string configZipPath, string AzureAccountName, string AzureAccountKey)
        {
            CloudStorageAccount storageAccount = null;
            CloudBlobClient blobClient = null;
            CloudBlobContainer container = null;
            CloudBlockBlob blockBlob = null;
            string leaseId = null;

            try
            {
                storageAccount = new CloudStorageAccount(new StorageCredentialsAccountAndKey(AzureAccountName, AzureAccountKey), true);
                blobClient = storageAccount.CreateCloudBlobClient();
                container = blobClient.GetContainerReference(AzureConfigContainerName);
                container.CreateIfNotExist();
                blockBlob = container.GetBlockBlobReference(ActualConfigBlobName());

                bool blobExists = AzureHelper.BlockBlobExists(logger, blockBlob);
               
                if (blobExists)
                    leaseId = AzureHelper.AcquireLease(logger, blockBlob, AzureBlobLeaseTimeout); // Acquire Lease on Blob
                else
                    blockBlob.Container.CreateIfNotExist();

                if (blobExists && leaseId == null)
                {
                    Utils.structuredLog(logger,"ER", "AcquireLease on Blob: " + ActualConfigBlobName() + " Failed");
                    return false;
                }

                string url = blockBlob.Uri.ToString();
                if (blockBlob.ServiceClient.Credentials.NeedsTransformUri)
                {
                    url = blockBlob.ServiceClient.Credentials.TransformUri(url);
                }

                var req = BlobRequest.Put(new Uri(url), AzureBlobLeaseTimeout, new BlobProperties(), BlobType.BlockBlob, leaseId, 0);

                using (var writer = new BinaryWriter(req.GetRequestStream()))
                {
                    writer.Write(File.ReadAllBytes(configZipPath));
                    writer.Close();
                }

                blockBlob.ServiceClient.Credentials.SignRequest(req);
                req.GetResponse().Close();
                AzureHelper.ReleaseLease(logger, blockBlob, leaseId, AzureBlobLeaseTimeout); // Release Lease on Blob
                return true;
            }
            catch (Exception e)
            {
                Utils.structuredLog(logger,"E", e.Message + ". UploadConfig_Azure, configZipPath: " + configZipPath + ". " + e);
                AzureHelper.ReleaseLease(logger, blockBlob, leaseId, AzureBlobLeaseTimeout);
                return false;
            }
        }

        private bool DownloadConfig_Azure(string downloadedZipPath, string AzureAccountName, string AzureAccountKey)
        {
            CloudStorageAccount storageAccount = null;
            CloudBlobClient blobClient = null;
            CloudBlobContainer container = null;
            CloudBlockBlob blockBlob = null;
            string leaseId = null;

            try
            {
                storageAccount = new CloudStorageAccount(new StorageCredentialsAccountAndKey(AzureAccountName, AzureAccountKey), true);
                blobClient = storageAccount.CreateCloudBlobClient();
                container = blobClient.GetContainerReference(AzureConfigContainerName);
                container.CreateIfNotExist();
                blockBlob = container.GetBlockBlobReference(DesiredConfigBlobName() );

                bool blobExists = AzureHelper.BlockBlobExists(logger, blockBlob);

                if (blobExists)
                    leaseId = AzureHelper.AcquireLease(logger, blockBlob,AzureBlobLeaseTimeout); // Acquire Lease on Blob
                else
                    return false; 

                if (blobExists && leaseId == null)
                {
                    Utils.structuredLog(logger,"ER", "AcquireLease on Blob: " + DesiredConfigBlobName() + " Failed");
                    return false;
                }

                string url = blockBlob.Uri.ToString();
                if (blockBlob.ServiceClient.Credentials.NeedsTransformUri)
                {
                    url = blockBlob.ServiceClient.Credentials.TransformUri(url);
                }

                var req = BlobRequest.Get(new Uri(url), AzureBlobLeaseTimeout, null, leaseId);
                blockBlob.ServiceClient.Credentials.SignRequest(req);
                
                using (var  reader = new BinaryReader(req.GetResponse().GetResponseStream()))
                {
                    FileStream zipFile = new FileStream(downloadedZipPath, FileMode.OpenOrCreate);
                    reader.BaseStream.CopyTo(zipFile);
                    zipFile.Close();
                }
                req.GetResponse().GetResponseStream().Close();

                AzureHelper.ReleaseLease(logger, blockBlob, leaseId,AzureBlobLeaseTimeout); // Release Lease on Blob
                return true;
            }
            catch (Exception e)
            {
                Utils.structuredLog(logger,"E", e.Message + ". DownloadConfig_Azure, downloadZipPath: " + downloadedZipPath+". "+e );
                AzureHelper.ReleaseLease(logger, blockBlob, leaseId,AzureBlobLeaseTimeout);
                return false;
            }
        }


        private string ActualConfigBlobName()
        {
            return "/" + Settings.OrgId + "/" + Settings.StudyId + "/" + Settings.HomeId + "/config/actual/" + CurrentConfigZipName; 
        }
        
        private string DesiredConfigBlobName()
        {
            return "/" +Settings.OrgId + "/" + Settings.StudyId + "/" + Settings.HomeId + "/config/desired/" + DownloadeConfigZipName; 
        }


        #region Methods to compute version of config dirs
        
        private Dictionary<string, string> GetConfigVersion(string configDir)
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();

            List<string> configFileNames = Utils.ListFiles(logger, configDir);
                configFileNames.Sort();
                foreach (string name in configFileNames)
                {
                    if (!name.Equals(CurrentVersionFileName, StringComparison.CurrentCultureIgnoreCase) && !name.Equals(ParentVersionFileName, StringComparison.CurrentCultureIgnoreCase))
                        retVal.Add(name, Utils.GetMD5HashOfFile(logger,configDir + "\\" + name));
                }
                return retVal;
           
        }

        private string ConvertVersionToString(Dictionary<string,string> version)
        {
            StringBuilder ret = new StringBuilder();
            foreach (string fileName in version.Keys)
            {
                string nameHashPair = fileName + "," + version[fileName] + ";";
                ret.Append(nameHashPair);
            }
            return ret.ToString();
        }
        
        private void UpdateCurrentVersionFile(Dictionary<string, string> version)
        {
            try
            {
                FileStream versionFile = new FileStream(Settings.ConfigDir + "\\" + CurrentVersionFileName, FileMode.OpenOrCreate);
                foreach (string fileName in version.Keys)
                {
                    string nameHashPair = fileName + "," + version[fileName] + ";";
                    versionFile.Write(System.Text.Encoding.ASCII.GetBytes(nameHashPair), 0, System.Text.Encoding.ASCII.GetByteCount(nameHashPair));
                }
                versionFile.Close();
            }
            catch (Exception e)
            {
                Utils.structuredLog(logger,"E", e.Message + ". UpdateCurrentVersionFile, version: " + version.ToString());
            }
        }

        #endregion 


      



        public void Reset(Configuration config, VLogger log,  int freq, Delegate method)
        {
            lock (this)
            {
                this.config = config;
                this.logger = log;
                this.frequency = freq;
                this.timer.Change(this.frequency, this.frequency);
                this.methodToInvoke = method;

                this.serviceHost.Close();

                string homeIdPart = "";
                if (!string.IsNullOrWhiteSpace(Settings.HomeId))
                    homeIdPart = "/" + Settings.HomeId;
                ConfigUpdaterWebService webService = new ConfigUpdaterWebService(logger, this);
                string url = Constants.InfoServiceAddress + homeIdPart + "/config";
                serviceHost = ConfigUpdaterWebService.CreateServiceHost(webService, new Uri(url));
                serviceHost.Open();
                Utils.structuredLog(logger,"I", "ConfigUpdaterWebService initiated at " + url);


            }
        }


        public UpdateStatus LastStatus()
        {
            lock (this.status)
            {
                return this.status;
            }
        }


        public bool SetDueTime(int dueTime)
        {
            lock (this.timer)
            {
                return timer.Change(dueTime, this.frequency);
            }

        }



        public void Dispose()
        {
            this.timer.Dispose();
            this.serviceHost.Close();
            GC.SuppressFinalize(this);
        }


        

    }




}



/* #region methods to acquire and relinquich leases on azure blobs; and check if a blob already exists
      private string AcquireLease(CloudBlockBlob blob)
      {
          try
          {
              var creds = blob.ServiceClient.Credentials;
              var transformedUri = new Uri(creds.TransformUri(blob.Uri.ToString()));
              var req = BlobRequest.Lease(transformedUri, AzureBlobLeaseTimeout, // timeout (in seconds)
                  LeaseAction.Acquire, // as opposed to "break" "release" or "renew"
                  null); // name of the existing lease, if any
              blob.ServiceClient.Credentials.SignRequest(req);
              using (var response = req.GetResponse())
              {
                  return response.Headers["x-ms-lease-id"];
              }
          }

          catch (WebException e)
          {
              Utils.structuredLog(logger,"WebException", e.Message + ". AcquireLease, blob: " + blob);
              return null;
          }
      }

      public void ReleaseLease(CloudBlob blob, string leaseId)
      {
          DoLeaseOperation(blob, leaseId, LeaseAction.Release);
      }

      private void DoLeaseOperation(CloudBlob blob, string leaseId, LeaseAction action)
      {
          try
          {
              if (blob == null || leaseId == null)
                  return;
              var creds = blob.ServiceClient.Credentials;
              var transformedUri = new Uri(creds.TransformUri(blob.Uri.ToString()));
              var req = BlobRequest.Lease(transformedUri, AzureBlobLeaseTimeout, action, leaseId);
              creds.SignRequest(req);
              req.GetResponse().Close();
          }
          catch (WebException e)
          {
              Utils.structuredLog(logger,"WebException", e.Message + ". DoLeaseOperation, blob: "+blob.Name+", leaseId: "+leaseId+", action "+ action);
          }
      }

      private bool BlockBlobExists(CloudBlockBlob blob)
      {
          try
          {
              blob.FetchAttributes();
              return true;
          }
          catch (StorageClientException e)
          {
              if (e.ErrorCode == StorageErrorCode.ResourceNotFound)
              {
                  Utils.structuredLog(logger,"E", "BlockBlob: " + blob.Name + " does not exist.");
                  return false;
              }
              else
              {
                  throw;
              }
          }
      }
  #endregion*/
