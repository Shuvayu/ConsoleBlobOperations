using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleBlob.Services
{
    public class BlobService : IBlobService
    {
        private string _connectionString;
        private CloudStorageAccount _storageAccount;
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;

        public BlobService(string connectionString, string containerName)
        {
            _connectionString = connectionString;
            _storageAccount = CloudStorageAccount.Parse(_connectionString);
            _blobClient = _storageAccount.CreateCloudBlobClient();
            _blobContainer = _blobClient.GetContainerReference(containerName);
            _blobContainer.CreateIfNotExistsAsync();
        }

        public void ClearMetadata()
        {
            _blobContainer.Metadata.Clear();
        }

        public void CopyBlob(string fromBlobName, string toBlobName)
        {
            try
            {
                var fromBlockBlob = _blobContainer.GetBlockBlobReference(fromBlobName);
                var toBlockBlob = _blobContainer.GetBlockBlobReference(toBlobName);
                toBlockBlob.StartCopyAsync(new Uri(fromBlockBlob.Uri.AbsoluteUri)).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message); ;                                
            }
        }

        public void CreateCORSPolicy()
        {
            ServiceProperties sp = new ServiceProperties();
            sp.Cors.CorsRules.Add(new CorsRule()
            {
                AllowedMethods = CorsHttpMethods.Get,
                AllowedOrigins = new List<string>() { "http://localhost:8080/" },
                MaxAgeInSeconds = 3600
            });
            _blobClient.SetServicePropertiesAsync(sp).Wait();
            Console.WriteLine("Cors Policy Set !!!");
        }

        public void CreateSharedAccessPolicy()
        {
            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.List
            };

            BlobContainerPermissions permissions = new BlobContainerPermissions();

            permissions.SharedAccessPolicies.Clear();
            permissions.SharedAccessPolicies.Add("PolicyName", policy);
            _blobContainer.SetPermissionsAsync(permissions).Wait();
            Console.WriteLine("Shared Access Policy Set !!!");
        }

        public void ListAttributes()
        {
            _blobContainer.FetchAttributesAsync().Wait();
            Console.WriteLine("Container Name : {0}", _blobContainer.StorageUri.PrimaryUri.ToString());
            Console.WriteLine("Last Modified : {0}", _blobContainer.Properties.LastModified.ToString());
            Console.WriteLine(Environment.NewLine);
        }

        public void ListMetadata()
        {
            _blobContainer.FetchAttributesAsync().Wait();
            Console.WriteLine("Metadata:");
            Console.WriteLine(Environment.NewLine);
            foreach (var item in _blobContainer.Metadata)
            {
                Console.WriteLine("Key: {0}", item.Key);
                Console.WriteLine("Value: {0}", item.Value);
                Console.WriteLine(Environment.NewLine);
            }
        }

        public void SetMetaData(string key, string value)
        {
            _blobContainer.Metadata.Add(key, value);
            _blobContainer.SetMetadataAsync().Wait();
        }

        public void UploadBlob(string blobName, Stream file)
        {
            try
            {
                var blockBlob = _blobContainer.GetBlockBlobReference(blobName);
                blockBlob.UploadFromStreamAsync(file).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message); ;
            }
        }

        public void UploadBlobSubDirectory(string directoryName, string subDirectoryName, string blobName, Stream file)
        {
            var directory = _blobContainer.GetDirectoryReference(directoryName);
            var subDirectory = directory.GetDirectoryReference(subDirectoryName);
            var blockBlob = subDirectory.GetBlockBlobReference(blobName);
            blockBlob.UploadFromStreamAsync(file).Wait();
        }
    }
}
