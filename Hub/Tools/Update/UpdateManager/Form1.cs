
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.StorageClient.Protocol;
using System;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace HomeOS.Hub.Tools.UpdateManager
{
    public partial class Form1 : Form
    {

        private static string tempDir = Environment.CurrentDirectory + "/" + "tmp";
        private const string actualConfigFileName = "actualconfig.zip";
        // paths for storage on the blob store
        private const string actualConfigFilePathInHubFolder = "/config/actual/";
        private const int AzureBlobLeaseTimeout = 60; 

        public Form1()
        {
            InitializeComponent();
            this.outputPanel.Hide();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScroll = true;
            this.MaximizedBounds = new Rectangle(0, 0, this.Size.Width, this.Size.Height);
            this.MaximumSize = new System.Drawing.Size(this.Size.Width, this.Size.Height);
        }

        private void hideAllControls(Panel panel)
        {
            foreach (Control control in panel.Controls)
                control.Hide();
        }

        private void showAllControls(Panel panel)
        {
            foreach (Control control in panel.Controls)
                control.Show();
        }


        private bool validateInput(string inputVarName, string inputVarValue)
        {
            if (string.IsNullOrEmpty(inputVarValue))
            {
                System.Windows.Forms.MessageBox.Show("Invalid "+inputVarName+ " provided!");
                return false;
            }
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            datemodified.Text = "";
            Cursor.Current = Cursors.WaitCursor;

            if (!validateInput(account.Name, account.Text))
                return;
            if (!validateInput(accountKey.Name, accountKey.Text))
                return;
            if (!validateInput(container.Name, container.Text))
                return;
           
    

            Tuple<bool, List<string>> ret =  listHubs(account, accountKey, container, orgID, studyID);

            if(!ret.Item1)
            {
                System.Windows.Forms.MessageBox.Show(ret.Item2[0].ToString());
            }
            else
            {
                hubList.Items.Clear();
                tabWindow.TabPages.Clear();
                foreach (string hub in ret.Item2)
                {
                    ListViewItem a = new ListViewItem(hub);
                    hubList.Items.Add(a);
                }

                this.outputPanel.Show();

            }

            Cursor.Current = Cursors.Default;
   

        }

        private Tuple<bool, List<string>> listHubs(MaskedTextBox account, MaskedTextBox accountKey, MaskedTextBox container, MaskedTextBox orgID, MaskedTextBox studyID)
        {
            
            try
            {
                CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(account.Text, accountKey.Text), true);
                Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer storageContainer = blobClient.GetContainerReference(container.Text);
             
                List<String> hubList = lsDirectory(storageContainer.ListBlobs(),"/"+orgID.Text+"/"+ studyID.Text+"/");

                return new Tuple<bool, List<string>>(true, hubList);

            }
            catch (Exception e)
            {
                return new Tuple<bool, List<string>>(false, new List<string>() { "Exception in listing homeIDs: "+ e.Message} );
            }
            
        }

        private string findDateModifiedBlob(string blobName)
        {
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(account.Text, accountKey.Text), true);
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer storageContainer = blobClient.GetContainerReference(container.Text);
            Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob blockBlob = storageContainer.GetBlockBlobReference(blobName);
            blockBlob.FetchAttributes();
            return blockBlob.Properties.LastModified.Value.DateTime.ToString();
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
                    ret.Add(directoryName.TrimEnd('/')); 
                }
                else
                    ret = ret.Concat(lsDirectory(directory.ListBlobs(), dirPath)).ToList();
            }
            return ret;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            initialInputPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        }

     

        private void outputPanel_Paint(object sender, PaintEventArgs e)
        {
            outputPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        }

        private void hubList_SelectedIndexChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            e.Item.ForeColor = Color.Black;
            if (e.IsSelected)
            {
                e.Item.ForeColor = Color.Blue;
                Cursor.Current = Cursors.WaitCursor;
                tabWindow.TabPages.Clear();
                writeActualConfigToDisk(account.Text, accountKey.Text, container.Text, orgID.Text, studyID.Text, e.Item.Text, tempDir);
                List<string> configFiles = ListFiles(tempDir + "/" + orgID.Text + "/" + studyID.Text + "/" + e.Item.Text + "/");
                configFiles.Sort();
                foreach (string file in configFiles)
                {
                    TabPage a = new TabPage(Path.GetFileName(file));
                    //a.AutoScroll = true;
                    
                    TextBox b = new TextBox();
                    b.Multiline = true;
                    b.BackColor = Color.White;
                    b.Width =  tabWindow.Size.Width * 99 / 100 ;
                    b.Height = tabWindow.Size.Height* 95 / 100;
                    b.ScrollBars = ScrollBars.Both;
                    b.Text = readFile(file);
                    b.ReadOnly = true; 

                    a.Controls.Add(b);
                    tabWindow.TabPages.Add(a);
                }
                datemodified.Text = "Actual Config Updated: " + findDateModifiedBlob("/" + orgID.Text + "/" + studyID.Text + "/" + e.Item.Text + actualConfigFilePathInHubFolder + actualConfigFileName);
                Cursor.Current = Cursors.Default;

            }
        }


        private static void writeActualConfigToDisk(string accountName, string accountKey, string container, string orgId, string studyId, string homeID, string actualConfigDir)
        {

            string zipPath = actualConfigDir + "/" + orgId + "/" + studyId + "/" + homeID + "/";

            Console.WriteLine("Creating directory: " + zipPath);
            CreateDirectory(zipPath);

            Console.WriteLine("Downloading config blob...");
            if (!DownloadConfig_Azure(zipPath + actualConfigFileName,  accountName, accountKey, orgId, studyId, homeID, container))
            {
                Console.WriteLine("WARNING! unable to download config for homeID: " + homeID);
                return;
            }

            Console.WriteLine("Unpacking config blob... ");
            if (!UnpackZip(zipPath + actualConfigFileName, zipPath))
                Console.WriteLine("WARNING! unable to unpack zip for homeID: " + homeID);
            else
            {
                Console.WriteLine("Current/actual config for homeID: " + homeID + "  is now in {0}", zipPath);
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

                blockBlob = container.GetBlockBlobReference(ActualConfigBlobName(orgID, studyID, homeID));

                bool blobExists = BlockBlobExists(blockBlob);

                if (blobExists)
                    leaseId = AcquireLease(blockBlob); // Acquire Lease on Blob
                else
                    return false;

                if (blobExists && leaseId == null)
                {
                    Console.WriteLine( "AcquireLease on Blob: " + ActualConfigBlobName(orgID, studyID, homeID) + " Failed");
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
                Console.WriteLine("E", e.Message + ". DownloadConfig_Azure, downloadZipPath: " + downloadedZipPath + ". " + e);
                ReleaseLease(blockBlob, leaseId);
                return false;
            }
        }

        private static string ActualConfigBlobName(string orgID, string studyID, string homeID)
        {
            return "/" + orgID + "/" + studyID + "/" + homeID + actualConfigFilePathInHubFolder + actualConfigFileName;
        }


        #region methods to acquire and relinquich leases on azure blobs; and check if a blob already exists
        private static string AcquireLease(Microsoft.WindowsAzure.StorageClient.CloudBlockBlob blob)
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
                Console.WriteLine("WebException", e.Message + ". AcquireLease, blob: " + blob);
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
                Console.WriteLine("WebException", e.Message + ". DoLeaseOperation, blob: " + blob.Name + ", leaseId: " + leaseId + ", action " + action);
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

        #region file and dir ops
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


        private static List<string> ListFiles(string directory)
        {
            List<string> retVal = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(directory))
                    retVal.Add(f);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + ". ListFiles, directory: " + directory);
            }
            return retVal;
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
                Console.WriteLine("E", e.Message + ". UnpackZip, zipPath: " + zipPath + ", extractPath:" + extractPath);
                return false;
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
                Console.WriteLine("E", e.Message + ". DeleteFile, filePath:" + filePath);
            }
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
                Console.WriteLine("E", e.Message + ". CleanDirectory, directory:" + directory);
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
                Console.WriteLine(e.Message + ". CreateDirectory, completePath:" + completePath);
            }

            return null;
        }

        #endregion

    }
}
