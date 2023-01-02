using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;

namespace SiteCounter
{
    public static class SiteCounterFunction
    {
        [FunctionName("SiteCounterFunction")]
        public static async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] Microsoft.AspNetCore.Http.HttpRequest req, ILogger log, ExecutionContext context)
        {
            log.LogInformation("SiteCounterFunction function processed a request.");
            // Get the storage connection string from the function app settings
            var storageConnectionString = System.Environment.GetEnvironmentVariable("BlobContainerConnectionString", System.EnvironmentVariableTarget.Process);

            // Connect to Azure Storage Account
            var storageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(storageConnectionString);

            // Get reference to container
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("azure-functions");

            // Create container if it doesn't already exist
            await container.CreateIfNotExistsAsync();

            // Get current site counter from blob
            var siteCounter = 0;
            var siteCounterBlob = container.GetBlockBlobReference("site-counter-value.txt");
            if (await siteCounterBlob.ExistsAsync())
            {
                // Increment site counter
                siteCounter = int.Parse(await siteCounterBlob.DownloadTextAsync()) + 1;
            }

            // Add new site counter value to blob
            await siteCounterBlob.UploadTextAsync(siteCounter.ToString());
        }
    }
}