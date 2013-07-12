using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using HomeOS.Hub.Common.DataStore;

namespace HomeOS.Hub.Common.DataStore
{
    public class AzureSynchronizer : ISync, IDisposable
    {
        protected bool disposed;
        protected SyncOrchestrator orchestrator;

        public AzureSynchronizer(RemoteInfo ri, string container)
        {
            disposed = false;
            string _containerName = container;

            //
            // Setup Store and Provider
            //
            CloudStorageAccount storageAccount = new CloudStorageAccount(new StorageCredentialsAccountAndKey(ri.accountName, ri.accountKey), true);
            AzureBlobStore blobStore = new AzureBlobStore(_containerName, storageAccount);
            Console.WriteLine("Successfully created/attached to container {0}.", _containerName);
            AzureBlobSyncProvider azureProvider = new AzureBlobSyncProvider(_containerName, blobStore);
            azureProvider.ApplyingChange += new EventHandler<ApplyingBlobEventArgs>(UploadingFile);

            orchestrator = new SyncOrchestrator();
            orchestrator.RemoteProvider = azureProvider;
            // upload only
            orchestrator.Direction = SyncDirectionOrder.Upload;
        }

        public void SetLocalSource(string FqDirName)
        {
            if (!Directory.Exists(FqDirName))
            {
                Console.WriteLine("Please ensure that the local target directory exists.");
                throw new ArgumentException("Please ensure that the local target directory exists.");
            }
            
            string _localPathName = FqDirName;
            FileSyncProvider fileSyncProvider = null;
            try
            {
                fileSyncProvider = new FileSyncProvider(_localPathName);
            }
            catch (ArgumentException)
            {
                fileSyncProvider = new FileSyncProvider(Guid.NewGuid(), _localPathName);
            }
            //fileSyncProvider.ApplyingChange += new EventHandler<ApplyingChangeEventArgs>(DownloadingFile);

            orchestrator.LocalProvider = fileSyncProvider;
        }

        public bool Sync()
        {
            bool status = false;
            if (orchestrator.LocalProvider != null) {
                SyncOperationStatistics sos = orchestrator.Synchronize();
                Console.WriteLine("Synchronization Complete");
                status = true;
            }
            return status;
        }

        public void Dispose() // NOT virtual
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Prevent finalizer from running.
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Call Dispose() on other objects owned by this instance.
                    // You can reference other finalizable objects here.
                }

                // Release unmanaged resources owned by (just) this object.
                disposed = true;
            }
        }

        ~AzureSynchronizer()
        {
            Dispose(false);
        }

        /*
        public static void DownloadingFile(object sender, ApplyingChangeEventArgs args)
        {
            switch (args.ChangeType)
            {
                case ChangeType.Create:
                    Console.WriteLine("Creating File: {0}...", args.NewFileData.Name);
                    break;
                case ChangeType.Delete:
                    Console.WriteLine("Deleting File: {0}...", args.CurrentFileData.Name);
                    break;
                case ChangeType.Rename:
                    Console.WriteLine("Renaming File: {0} to {1}...", args.CurrentFileData.Name, args.NewFileData.Name);
                    break;
                case ChangeType.Update:
                    Console.WriteLine("Updating File: {0}...", args.NewFileData.Name);
                    break;
            }
        }
        */

        public static void UploadingFile(object sender, ApplyingBlobEventArgs args)
        {
            switch (args.ChangeType)
            {
                case ChangeType.Create:
                    Console.WriteLine("Creating Azure Blob: {0}...", args.CurrentBlobName);
                    break;
                case ChangeType.Delete:
                    Console.WriteLine("Deleting Azure Blob: {0}...", args.CurrentBlobName);
                    break;
                case ChangeType.Rename:
                    Console.WriteLine("Renaming Azure Blob: {0} to {1}...", args.CurrentBlobName, args.NewBlobName);
                    break;
                case ChangeType.Update:
                    Console.WriteLine("Updating Azure Blob: {0}...", args.CurrentBlobName);
                    break;
            }
        }
    }
}
