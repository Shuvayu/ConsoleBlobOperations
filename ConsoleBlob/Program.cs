using ConsoleBlob.Models.ConfigModels;
using ConsoleBlob.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace ConsoleBlob
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        static void Main()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            var azureSettings = new AzureConfig();
            Configuration.GetSection("Azure").Bind(azureSettings);

            var testResource = new TestResourcesConfig();
            Configuration.GetSection("TestResources").Bind(testResource);

            try
            {
                using (var fileStream = File.OpenRead(testResource.FilePath))
                {
                    IBlobService blobService = new BlobService(azureSettings.StorageAccountConnectionString, testResource.ContainerName);
                    var newBlobName = Guid.NewGuid().ToString() + ".jpg";
                    blobService.UploadBlob(newBlobName, fileStream);
                    Console.WriteLine("Blob has been uploaded !!!");
                    Console.WriteLine(Environment.NewLine);
                    blobService.ListAttributes();
                    blobService.ClearMetadata();
                    blobService.SetMetaData("Author", "Shuv");
                    blobService.SetMetaData("Date", DateTimeOffset.Now.Date.ToString());
                    blobService.ListMetadata();
                    blobService.CopyBlob(newBlobName, newBlobName.Replace(".jpg", "-copy.jpg"));
                    Console.WriteLine("Blob has been copied !!!");
                    Console.WriteLine(Environment.NewLine);
                    blobService.UploadBlobSubDirectory("parent-directory", "child-directory", newBlobName, fileStream);
                    Console.WriteLine("Blob has been uploaded again !!!");
                    Console.WriteLine(Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }

            Console.ReadLine();
        }
    }
}
