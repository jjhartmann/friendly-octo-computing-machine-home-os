using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeOS.Hub.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.StorageClient.Protocol;
using System.Net;
using System.IO;
using System.Security.Cryptography;
namespace ConfigPackager
{
    class ConfigPackager
    {
        //commands supported
        private const string getactual = "getactual";
        private const string setdesired = "setdesired";

        // version file names
        private const string CurrentVersionFileName = ".currentversion";
        private const string ParentVersionFileName = ".parentversion";
        private const string VersionDefinitionFileName = ".versiondef"; 

        // paths for storage on the blob store
        private const string actualConfigFilePathInHubFolder = "/config/actual/";
        private const string desiredConfigFilePathInHubFolder = "/config/desired/";

        // file names for actual and desired config zip files
        private const string actualConfigFileName = "actualconfig.zip";
        private const string desiredConfigFileName = "desiredconfig.zip";

        private const int AzureBlobLeaseTimeout = 60; 
        static void Main(string[] args)
        {
            var argsDict = ProcessArguments(args);

            // if the function is missing
            if(string.IsNullOrEmpty((string)argsDict["Function"]))
                die(missingArgumentMessage("Function"));

            // if the key file is missing
            if (string.IsNullOrEmpty((string)argsDict["AccountKeyFile"]) && string.IsNullOrEmpty((string)argsDict["AccountKey"]))
                 die(missingArgumentMessage("Key")); 

            // if the function is neither of the two the tool supports
            if (!getactual.Equals((string)argsDict["Function"], StringComparison.CurrentCultureIgnoreCase) &&
                !setdesired.Equals((string)argsDict["Function"], StringComparison.CurrentCultureIgnoreCase) )
                die("unknown function provided. chose " + getactual + " or " +setdesired); 

            // if the ActualConfigDir is missing 
            if (string.IsNullOrEmpty((string)argsDict["ActualConfigDir"]))
                die(missingArgumentMessage("ActualConfigDir"));

            // if the function is desired. check further arguments
            if (setdesired.Equals((string)argsDict["Function"], StringComparison.CurrentCultureIgnoreCase) &&
                string.IsNullOrEmpty((string)argsDict["DesiredConfigDir"]) )
                die(missingArgumentMessage("DesiredConfigDir"));

            if(getactual.Equals((string)argsDict["Function"]))
            {
                Console.WriteLine("\nPerforming GetActualConfig with following arguments:");
                printArgumentsDictionary(argsDict);
                getActualConfig(argsDict);
            }

            if(setdesired.Equals((string)argsDict["Function"]))
            {
                Console.WriteLine("\nPerforming SetDesiredConfig with following arguments:");
                printArgumentsDictionary(argsDict);
                setDesiredConfig(argsDict);
            }
            

        }

        #region methods for function: setdesiredconfig
        
        private static void setDesiredConfig(ArgumentsDictionary argsDict)
        {
            string orgId = (string)argsDict["OrgID"];
            string studyId = (string)argsDict["StudyID"];
            string homeIDs = (string)argsDict["HomeIDs"];
            string container = (string)argsDict["Container"];
            string actualConfigDir = (string)argsDict["ActualConfigDir"];
            string desiredConfigDir = (string)argsDict["DesiredConfigDir"];
            string accountName = (string)argsDict["AccountName"];


            string accountKey = null;
            if (!String.IsNullOrEmpty((string)argsDict["AccountKey"]))
                accountKey = (string)argsDict["AccountKey"];
            else
                accountKey = readFile((string)argsDict["AccountKeyFile"]);

            if (string.IsNullOrEmpty(accountKey))
                die("Could not obtain AccountKey");


            if(!Directory.Exists(desiredConfigDir+"/"+orgId))
                die("ERROR! Directory "+desiredConfigDir+"/"+orgId+" corresponding to given orgID: "+orgId+" does not exist.");

            if (!Directory.Exists(desiredConfigDir + "/" + orgId + "/" + studyId))
                die("ERROR! Directory " + desiredConfigDir + "/" + orgId + "/" + studyId + " corresponding to given studyID: " + studyId + " does not exist.");

            string[] homeID = null;
            if (!homeIDs.Equals("*"))
            {
                homeID = homeIDs.Split(',');
            }
            else
            {
                Console.WriteLine("\nFetching list of homeIDs from "+desiredConfigDir);
                homeID = ListDirectories(desiredConfigDir + "/" + orgId + "/" + studyId+"/").ToArray();
            }

            Console.WriteLine("HomeID list: ");
            for (int i = 0; i < homeID.Count(); i++)
            {
                homeID[i] = homeID[i].TrimEnd('/');
                Console.WriteLine("{0}", homeID[i]);
            }

            foreach (string h in homeID)
            {
                Console.WriteLine("\nSetting desired config for homeID:" + h);
                writeDesiredConfigToAzure(accountName, accountKey, container, orgId, studyId, h, actualConfigDir, desiredConfigDir);
            }


        }

        private static void writeDesiredConfigToAzure(string accountName, string accountKey, string container, string orgId, string studyId, string homeID, string actualConfigDir, string desiredConfigDir)
        {
            string zipPath_desired = desiredConfigDir + "/" + orgId + "/" + studyId + "/" + homeID + "/";
            string zipPath_actual = actualConfigDir + "/" + orgId + "/" + studyId + "/" + homeID + "/";

            if (!Directory.Exists(zipPath_desired))
            {
                Console.WriteLine("ERROR! desired config  for homeID: "+homeID +" "+zipPath_desired+ " does not exist" );
                return;
            }

            if (!Directory.Exists(zipPath_actual))
            {
                Console.WriteLine("ERROR! current/actual config for homeID: " + homeID + " " + zipPath_actual + " does not exist");
                return;
            }

            // copy current version from actual onto that of desired

            if (File.Exists(zipPath_actual + CurrentVersionFileName))
            {
                Console.WriteLine("Copying {0} to {1} ", zipPath_actual + CurrentVersionFileName, zipPath_desired + ParentVersionFileName);
                CopyFile(zipPath_actual + CurrentVersionFileName, zipPath_desired + ParentVersionFileName);
            }
            else
            {
               Console.WriteLine("Writing version of config in {0} to {1} ",zipPath_actual,  zipPath_desired + ParentVersionFileName);
               UpdateVersionFile( GetConfigVersion(zipPath_actual), zipPath_desired+ParentVersionFileName);
            }

            Console.WriteLine("Writing version of config in {0} to {1} ", zipPath_desired, zipPath_desired + CurrentVersionFileName);
            Dictionary<string, string> currentVersion_desired = GetConfigVersion(zipPath_desired);
            UpdateVersionFile(currentVersion_desired, zipPath_desired + CurrentVersionFileName);

            File.Delete(desiredConfigDir + "/" + desiredConfigFileName);
            PackZip(zipPath_desired, desiredConfigDir + "/" + desiredConfigFileName);
            MoveFile(desiredConfigDir + "/" + desiredConfigFileName, zipPath_desired + desiredConfigFileName);

            Console.WriteLine("Uploading desired config for homeID {0} ", homeID);
            if (!UploadConfig_Azure(zipPath_desired + desiredConfigFileName, accountName, accountKey, orgId, studyId, homeID, container))
            {
                Console.WriteLine("WARNING! unable to upload config for homeID: " + homeID);
                
            }
            DeleteFile(zipPath_desired + desiredConfigFileName);
        }

        private static bool UploadConfig_Azure(string configZipPath, string AzureAccountName, string AzureAccountKey,string orgID, string studyID, string homeID,  string containerName)
        {
            Microsoft.WindowsAzure.CloudStorageAccount storageAccount = null;
            Microsoft.WindowsAzure.StorageClient.CloudBlobClient blobClient = null;
            Microsoft.WindowsAzure.StorageClient.CloudBlobContainer container = null;
            Microsoft.WindowsAzure.StorageClient.CloudBlockBlob blockBlob = null;
            string leaseId = null;

            try
            {
                storageAccount = new Microsoft.WindowsAzure.CloudStorageAccount(new Microsoft.WindowsAzure.StorageCredentialsAccountAndKey(AzureAccountName, AzureAccountKey), true);                    
                blobClient = storageAccount.CreateCloudBlobClient();
                container = blobClient.GetContainerReference(containerName);
                container.CreateIfNotExist();
                blockBlob = container.GetBlockBlobReference(DesiredConfigBlobName(orgID, studyID, homeID));

                bool blobExists = BlockBlobExists(blockBlob);

                if (blobExists)
                    leaseId = AcquireLease(blockBlob); // Acquire Lease on Blob
                else
                    blockBlob.Container.CreateIfNotExist();

                if (blobExists && leaseId == null)
                {
                    configLog("ER", "AcquireLease on Blob: " + DesiredConfigBlobName(orgID, studyID, homeID) + " Failed");
                    return false;
                }

                string url = blockBlob.Uri.ToString();
                if (blockBlob.ServiceClient.Credentials.NeedsTransformUri)
                {
                    url = blockBlob.ServiceClient.Credentials.TransformUri(url);
                }

                var req = BlobRequest.Put(new Uri(url), AzureBlobLeaseTimeout, new Microsoft.WindowsAzure.StorageClient.BlobProperties(), Microsoft.WindowsAzure.StorageClient.BlobType.BlockBlob, leaseId, 0);

                using (var writer = new BinaryWriter(req.GetRequestStream()))
                {
                    writer.Write(File.ReadAllBytes(configZipPath));
                    writer.Close();
                }

                blockBlob.ServiceClient.Credentials.SignRequest(req);
                req.GetResponse().Close();
                ReleaseLease(blockBlob, leaseId); // Release Lease on Blob
                return true;
            }
            catch (Exception e)
            {
                configLog("E", e.Message + ". UploadConfig_Azure, configZipPath: " + configZipPath + ". " + e);
                ReleaseLease(blockBlob, leaseId);
                return false;
            }
        }



        #endregion


        #region methods for function: getactualconfig

        private static void getActualConfig(ArgumentsDictionary argsDict)
        {
            string orgId = (string)argsDict["OrgID"];
            string studyId = (string)argsDict["StudyID"];
            string homeIDs = (string)argsDict["HomeIDs"];
            string container = (string)argsDict["Container"];
            string actualConfigDir = (string)argsDict["ActualConfigDir"];
            string accountName = (string)argsDict["AccountName"];


            string accountKey= null ;
            if (!String.IsNullOrEmpty((string)argsDict["AccountKey"]))
                accountKey = (string)argsDict["AccountKey"]; 
            else 
                accountKey = readFile((string)argsDict["AccountKeyFile"]);

            if (string.IsNullOrEmpty(accountKey))
                die("Could not obtain AccountKey");

            string[] homeID=null;
            if (!homeIDs.Equals("*"))
            {
                homeID = homeIDs.Split(',');
            }
            else
            {

                Console.WriteLine("\nFetching list of homeIDs from container: "+container);
                Tuple<bool, List<string>> fetchHubList = listHubs(accountName, accountKey, container, orgId, studyId );
                if (fetchHubList.Item1)
                    homeID = fetchHubList.Item2.ToArray();
                else
                    die("ERROR! Cannot obtain hubs list! Exception : "+ fetchHubList.Item2[0]);
            }

            Console.WriteLine("HomeID list: ");
            for (int i=0; i < homeID.Count() ; i++)
            {
                homeID[i] = homeID[i].TrimEnd('/'); 
                Console.WriteLine("{0}", homeID[i]);
            }

            foreach (string h in homeID)
            {
                Console.WriteLine("\nFetching config for homeID:"+h );
                writeActualConfigToDisk(accountName, accountKey, container, orgId, studyId, h, actualConfigDir);
            }
        }

        private static void writeActualConfigToDisk(string accountName, string accountKey, string container, string orgId, string studyId, string homeID, string actualConfigDir)
        {

            string zipPath = actualConfigDir + "/" + orgId + "/" + studyId + "/" + homeID +"/";
            
            Console.WriteLine("Creating directory: "+zipPath);
            CreateDirectory(zipPath);

            Console.WriteLine("Downloading config blob...");
            if (!DownloadConfig_Azure(zipPath + actualConfigFileName, accountName, accountKey, orgId, studyId, homeID, container))
            {
                Console.WriteLine("WARNING! unable to download config for homeID: " + homeID);
                return;
            }

            Console.WriteLine("Unpacking config blob... ");
            if (!UnpackZip(zipPath + actualConfigFileName, zipPath))
                Console.WriteLine("WARNING! unable to unpack zip for homeID: " + homeID);
            else
            {
                Console.WriteLine("Current/actual config for homeID: "+homeID +"  is now in {0}", zipPath );
                DeleteFile(zipPath + actualConfigFileName);
            }
        }

        private static bool DownloadConfig_Azure(string downloadedZipPath, string AzureAccountName, string AzureAccountKey, string orgID, string studyID, string homeID, string containerName)
        {
            
            Microsoft.WindowsAzure.CloudStorageAccount storageAccount = null;
            Microsoft.WindowsAzure.StorageClient.CloudBlobClient blobClient = null;
            Microsoft.WindowsAzure.StorageClient.CloudBlobContainer container = null;
            Microsoft.WindowsAzure.StorageClient.CloudBlockBlob blockBlob = null;
            string leaseId = null;

            try
            {
                storageAccount = new Microsoft.WindowsAzure.CloudStorageAccount(new Microsoft.WindowsAzure.StorageCredentialsAccountAndKey(AzureAccountName, AzureAccountKey), true);                    
                blobClient = storageAccount.CreateCloudBlobClient();
                container = blobClient.GetContainerReference(containerName);
                
                blockBlob = container.GetBlockBlobReference(ActualConfigBlobName(orgID, studyID, homeID) );

                bool blobExists = BlockBlobExists(blockBlob);

                if (blobExists)
                    leaseId = AcquireLease(blockBlob); // Acquire Lease on Blob
                else
                    return false;

                if (blobExists && leaseId == null)
                {
                    configLog("ER", "AcquireLease on Blob: " + ActualConfigBlobName(orgID, studyID, homeID) + " Failed");
                    return false;
                }

                string url = blockBlob.Uri.ToString();
                if (blockBlob.ServiceClient.Credentials.NeedsTransformUri)
                {
                    url = blockBlob.ServiceClient.Credentials.TransformUri(url);
                }

                var req = BlobRequest.Get(new Uri(url), AzureBlobLeaseTimeout, null, leaseId);
                blockBlob.ServiceClient.Credentials.SignRequest(req);

                using (var reader = new BinaryReader(req.GetResponse().GetResponseStream()))
                {
                    FileStream zipFile = new FileStream(downloadedZipPath, FileMode.OpenOrCreate);
                    reader.BaseStream.CopyTo(zipFile);
                    zipFile.Close();
                }
                req.GetResponse().GetResponseStream().Close();

                ReleaseLease(blockBlob, leaseId); // Release Lease on Blob
                return true;
            }
            catch (Exception e)
            {
                configLog("E", e.Message + ". DownloadConfig_Azure, downloadZipPath: " + downloadedZipPath + ". " + e);
                ReleaseLease(blockBlob, leaseId);
                return false;
            }
        }

        private static string ActualConfigBlobName(string orgID,  string studyID, string homeID)
        {
            return "/" + orgID + "/" + studyID + "/" + homeID + actualConfigFilePathInHubFolder + actualConfigFileName;
        }

        private static string DesiredConfigBlobName(string orgID, string studyID, string homeID)
        {
            return "/" + orgID + "/" + studyID + "/" + homeID + desiredConfigFilePathInHubFolder + desiredConfigFileName;
        }


#endregion 
              
        #region file and directory ops

        private static string GetMD5HashOfFile(string filePath)
        {
            try
            {
                FileStream file = new FileStream(filePath, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                configLog("E", e.Message + ". GetMD5HashOfFile(), file" + filePath);
                return "";
            }
        }


        private static List<string> ListFiles(string directory)
        {
            List<string> retVal = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(directory))
                    retVal.Add(Path.GetFileName(f));
            }
            catch (Exception e)
            {
                configLog("E", e.Message + ". ListFiles, directory: " + directory);
            }
            return retVal;
        }

        private static List<string> ListDirectories(string directory)
        {
            List<string> retVal = new List<string>();
            try
            {
                foreach (string f in Directory.GetDirectories(directory))
                    retVal.Add(Path.GetFileName(f));
            }
            catch (Exception e)
            {
                configLog("E", e.Message + ". ListFiles, directory: " + directory);
            }
            return retVal;
        }

        private static string readFile(string filePath)
        {
            try
            {

                System.IO.StreamReader myFile = new System.IO.StreamReader(filePath);
                string myString = myFile.ReadToEnd();
                myFile.Close();
                return myString;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in reading file " + filePath + "! " + e.Message);
                return "";
            }
        }

        private static void DeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception e)
            {
                configLog("E", e.Message + ". DeleteFile, filePath:" + filePath);
            }
        }

        private static void CopyFile(string file1 , string file2)
        {
            try
            {
                if (File.Exists(file2))
                    File.Delete(file2);

                File.Copy(file1, file2);
            }
            catch (Exception e)
            {
                configLog("E", e.Message + ". CopyFile, filePath1: {0} filePath2: {1}" , file1, file2);
            }
        }

        private static void MoveFile(string file1, string file2)
        {
            try
            {
                if (File.Exists(file2))
                    File.Delete(file2);

                File.Move(file1, file2);
            }
            catch (Exception e)
            {
                configLog("E", e.Message + ". MoveFile, filePath1: {0} filePath2: {1}", file1, file2);
            }
        }


        private static string CreateDirectory(String completePath)
        {
            try
            {
                if (Directory.Exists(completePath))
                {
                    CleanDirectory(completePath);
                    Directory.Delete(completePath, true);
                }

                Directory.CreateDirectory(completePath);
                DirectoryInfo info = new DirectoryInfo(completePath);
                System.Security.AccessControl.DirectorySecurity security = info.GetAccessControl();
                security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(Environment.UserName, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.InheritanceFlags.ContainerInherit, System.Security.AccessControl.PropagationFlags.None, System.Security.AccessControl.AccessControlType.Allow));
                security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(Environment.UserName, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.InheritanceFlags.ContainerInherit, System.Security.AccessControl.PropagationFlags.None, System.Security.AccessControl.AccessControlType.Allow));
                info.SetAccessControl(security);
                return completePath;
            }
            catch (Exception e)
            {
                configLog("E", e.Message + ". CreateDirectory, completePath:" + completePath);
            }

            return null;
        }

        private static void CleanDirectory(string directory)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(directory);
                foreach (FileInfo fi in dir.GetFiles())
                {
                    fi.Delete();
                }

                foreach (DirectoryInfo di in dir.GetDirectories())
                {
                    CleanDirectory(di.FullName);
                    di.Delete();
                }
            }
            catch (Exception e)
            {
                configLog("E", e.Message + ". CleanDirectory, directory:" + directory);
            }
        }

        private static bool UnpackZip(String zipPath, string extractPath)
        {
            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);
                return true;
            }
            catch (Exception e)
            {
                configLog("E", e.Message + ". UnpackZip, zipPath: " + zipPath + ", extractPath:" + extractPath);
                return false;
            }

        }

        private static bool PackZip(string startPath, String zipPath)
        {
            try
            {
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
                System.IO.Compression.ZipFile.CreateFromDirectory(startPath, zipPath);
                return true;
            }
            catch (Exception e)
            {
                configLog("E", e.Message + ". PackZip, startPath: " + startPath + ", zipPath:" + zipPath);
                return false;
            }

        }
        
        #endregion

        #region methods to acquire and relinquich leases on azure blobs; and check if a blob already exists
        private static  string AcquireLease(Microsoft.WindowsAzure.StorageClient.CloudBlockBlob blob)
        {
            try
            {
                var creds = blob.ServiceClient.Credentials;
                var transformedUri = new Uri(creds.TransformUri(blob.Uri.ToString()));
                var req = BlobRequest.Lease(transformedUri, AzureBlobLeaseTimeout, // timeout (in seconds)
                    Microsoft.WindowsAzure.StorageClient.Protocol.LeaseAction.Acquire, // as opposed to "break" "release" or "renew"
                    null); // name of the existing lease, if any
                blob.ServiceClient.Credentials.SignRequest(req);
                using (var response = req.GetResponse())
                {
                    return response.Headers["x-ms-lease-id"];
                }
            }

            catch (WebException e)
            {
                configLog("WebException", e.Message + ". AcquireLease, blob: " + blob);
                return null;
            }
        }

        public static void ReleaseLease(CloudBlob blob, string leaseId)
        {
            DoLeaseOperation(blob, leaseId, Microsoft.WindowsAzure.StorageClient.Protocol.LeaseAction.Release);
        }

        private static void DoLeaseOperation(CloudBlob blob, string leaseId, Microsoft.WindowsAzure.StorageClient.Protocol.LeaseAction action)
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
                configLog("WebException", e.Message + ". DoLeaseOperation, blob: " + blob.Name + ", leaseId: " + leaseId + ", action " + action);
            }
        }
        private static bool BlockBlobExists(Microsoft.WindowsAzure.StorageClient.CloudBlockBlob blob)
        {
            try
            {
                blob.FetchAttributes();
                return true;
            }
            catch (Microsoft.WindowsAzure.StorageClient.StorageClientException e)
            {
                if (e.ErrorCode == Microsoft.WindowsAzure.StorageClient.StorageErrorCode.ResourceNotFound)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

#endregion 
        
        private static Tuple<bool, List<string>> listHubs(string account, string accountKey, string container, string orgID, string studyID)
        {

            try
            {
                CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(account, accountKey), true);
                Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer storageContainer = blobClient.GetContainerReference(container);

                List<String> hubList = lsDirectory(storageContainer.ListBlobs(), "/" + orgID + "/" + studyID + "/");

                return new Tuple<bool, List<string>>(true, hubList);

            }
            catch (Exception e)
            {
                return new Tuple<bool, List<string>>(false, new List<string>() { e.Message });
            }

        }

        private static List<string> lsDirectory(IEnumerable<Microsoft.WindowsAzure.Storage.Blob.IListBlobItem> enumerable, string dirPath)
        {

            List<string> ret = new List<string>();

            foreach (var item in enumerable.Where((blobItem, type) => blobItem is Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob))
            {
                var blobFile = item as Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob;
                string outputFileName = blobFile.Name;
                if (blobFile.Parent.Prefix.Equals(dirPath))
                    ret.Add(outputFileName);
            }

            foreach (var item in enumerable.Where((blobItem, type) => blobItem is Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory))
            {
                var directory = item as Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory;
                string directoryName = directory.Prefix;

                if (directoryName.Equals(dirPath))
                    ret = ret.Concat(lsDirectory(directory.ListBlobs(), dirPath)).ToList();

                else if (directory.Parent != null && directory.Parent.Prefix.Equals(dirPath))
                {
                    directoryName = directoryName.Replace(directory.Parent.Prefix, "");
                    ret.Add(directoryName);
                }
                else
                    ret = ret.Concat(lsDirectory(directory.ListBlobs(), dirPath)).ToList();
            }
            return ret;
        }

        #region argument printing and handling 

        private static void configLog(string type, params string[] messages)
        {
            if (type == "ER") type = "ERROR";
            else if (type == "I") type = "INFO";
            else if (type == "E") type = "EXCEPTION";
            else if (type == "W") type = "WARNING";

            StringBuilder s = new StringBuilder();
            s.Append("[" + type + "]");
            foreach (string message in messages)
                s.Append("[" + message + "]");
            Console.WriteLine(s.ToString());
        }



        /// <summary>
        /// die after printing given message
        /// </summary>
        /// <param name="message"></param>
        private static void die(string message)
        {
            Console.WriteLine(message);
            System.Environment.Exit(0);
        }

        private static string missingArgumentMessage(string argumentName)
        {
            return "Argument: " + argumentName + " is missing. Use --Help for help.";
        }

        private static void printArgumentsDictionary(ArgumentsDictionary dict)
        {
            foreach (string key in dict.Keys)
            {
                Console.WriteLine(key + " : " + dict[key]);
            }

        }
        #endregion 

        #region method for computing version, reading and writing them 
        private static Dictionary<string, string> GetConfigVersion(string configDir)
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            List<string> configFilesToHash = GetFileNamesInVersionDef(configDir);

            foreach (string name in configFilesToHash)
            {
                if (!name.Equals(CurrentVersionFileName, StringComparison.CurrentCultureIgnoreCase)
                    && !name.Equals(ParentVersionFileName, StringComparison.CurrentCultureIgnoreCase)
                    && !name.Equals(VersionDefinitionFileName, StringComparison.CurrentCultureIgnoreCase))
                    retVal.Add(name, GetMD5HashOfFile(configDir + "\\" + name));
            }

            return retVal;
            /*
            Dictionary<string, string> retVal = new Dictionary<string, string>();
           
                List<string> configFileNames = ListFiles(configDir);
                configFileNames.Sort();
                foreach (string name in configFileNames)
                {
                    if (!name.Equals(currentVersionFileName, StringComparison.CurrentCultureIgnoreCase) && !name.Equals(parentVersionFileName, StringComparison.CurrentCultureIgnoreCase))
                        retVal.Add(name, GetMD5HashOfFile(configDir + "\\" + name));
                }
                return retVal;*/
            
        }

        private static List<string> GetFileNamesInVersionDef(string configDir)
        {
            List<string> filesInVersion = Constants.DefaultConfigVersionDefinition.ToList();
            List<string> filesInConfigDir = ListFiles(configDir);
            List<string> configFilesToHash = filesInVersion.ToList();

            try
            {
                filesInVersion = GetVersionDef(configDir);
                filesInConfigDir.Sort();
                configFilesToHash = filesInConfigDir.Intersect(filesInVersion.ToList()).ToList();
            }
            catch (Exception e)
            {
                configLog( "E", e.Message + " .GetConfigVersion");
            }

            configFilesToHash.Sort();
            return configFilesToHash;
        }

        private static List<string> GetVersionDef(string configDir)
        {
            List<string> retVal = new List<string>();
            try
            {
                string versionDefinition = ReadFile( configDir + "\\" + VersionDefinitionFileName);
                if (!string.IsNullOrEmpty(versionDefinition))
                {
                    retVal = versionDefinition.Split(';').ToList();
                    retVal.Sort();
                }

            }
            catch (Exception e)
            {
                configLog( "E", e.Message + " .GetVersionDef " + configDir);
            }

            return retVal;
        }

        public static string ReadFile( string filePath)
        {
            try
            {

                System.IO.StreamReader myFile = new System.IO.StreamReader(filePath);
                string myString = myFile.ReadToEnd();
                myFile.Close();
                return myString;
            }
            catch (Exception e)
            {
                configLog( "E", e.Message + ". GetMD5HashOfFile(), file" + filePath);
                return "";
            }
        }


        private static void UpdateVersionFile(Dictionary<string, string> version, string versionFilePath)
        {
            try
            {
                FileStream versionFile = new FileStream(versionFilePath, FileMode.OpenOrCreate);
                foreach (string fileName in version.Keys)
                {
                    string nameHashPair = fileName + "," + version[fileName] + ";";
                    versionFile.Write(System.Text.Encoding.ASCII.GetBytes(nameHashPair), 0, System.Text.Encoding.ASCII.GetByteCount(nameHashPair));
                }
                versionFile.Close();
            }
            catch (Exception e)
            {
                configLog("E", e.Message + ". UpdateVersionFile, version: " + version.ToString());
            }
        }


        #endregion 


        /// <summary>
        /// Processes the command line arguments
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private static ArgumentsDictionary ProcessArguments(string[] arguments)
        {
            ArgumentSpec[] argSpecs = new ArgumentSpec[]
            {
                    new ArgumentSpec(
                    "Help",
                    '?',
                    false,
                    null,
                    "Display this help message."),
               
                    new ArgumentSpec(
                   "AccountKey",
                   'k',
                   "",
                   "Azure account key",
                   "Azure account key"),

                    new ArgumentSpec(
                   "AccountKeyFile",
                   '\0',
                   "",
                   "file containing Azure account key",
                   "file containing Azure account key"),

                   new ArgumentSpec(
                   "AccountName",
                   'n',
                   "homelab",
                   "Azure account name",
                   "Azure account name"),

                   new ArgumentSpec(
                   "Container",
                   'c',
                   "configs",
                   "Config storage container name",
                   "Config storage container name"),

                   new ArgumentSpec(
                   "OrgID",
                   'o',
                   "Default",
                   "Organization ID or OrgID",
                   "Organization ID or OrgID"),

                   new ArgumentSpec(
                   "StudyID",
                   's',
                   "Default",
                   "StudyID",
                   "StudyID"),

                   new ArgumentSpec(
                   "HomeIDs",
                   'h',
                   "*",
                   "HomeIDs",
                   "single HomeID or comma-separated multiple HomeIDs"),

                   new ArgumentSpec(
                   "ActualConfigDir",
                   'a',
                   "",
                   "Actual Configs Directory",
                   "Directory for reading or writing actual/current Hub configs"),

                   new ArgumentSpec(
                   "DesiredConfigDir",
                   'd',
                   "",
                   "Desired Configs Directory",
                   "Directory for reading desired Hub configs"),

                    new ArgumentSpec(
                   "Function",
                   'f',
                   "",
                   "function to get performed",
                   "function to get performed"),

            };

            ArgumentsDictionary args = new ArgumentsDictionary(arguments, argSpecs);
            if (args.AppSettingsParseError)
            {
                Console.Error.WriteLine("Error in .config file options: ignoring");
            }

            if (args.CommandLineParseError)
            {
                Console.Error.WriteLine("Error in command line arguments at {0}\n", args.ParseErrorArgument);
                Console.Error.WriteLine(args.GetUsage("ConfigPackager"));
                System.Environment.Exit(1);
            }

            if ((bool)args["Help"])
            {
                Console.Error.WriteLine("Downloads current Hub configs, and uploads desired configs for deployment on hubs\n");
                Console.Error.WriteLine(args.GetUsage("ConfigPackager"));
                System.Environment.Exit(0);
            }

            return args;

        }
    }
}
