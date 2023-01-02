using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs.Extensions.Storage;

namespace SiteCounter
{
    public class SiteCounter
    {
        public int Counter { get; set; }
        public string id { get; set; } = "1";
    }
    public static class SiteCounterCosmosDbFunction
    {
        [FunctionName("BlobTrigger")]
        [StorageAccount("BlobContainerConnectionString")]
        public static async Task Run([BlobTrigger("site-counter/{name}")] CloudBlockBlob siteCounterBlob, string name, ILogger log)
        {
            log.LogInformation("SiteCounterCosmosDbFunction function processed a request.");

            // Get the storage blob connection string from app settings
            var storageBlobConnectionString = System.Environment.GetEnvironmentVariable("BlobContainerConnectionString");

            // Get the CosmosDB connection string from app settings
            var cosmosDBConnectionString = System.Environment.GetEnvironmentVariable("DatabaseConnectionString");

            // Connect to CosmosDB
            CosmosClient cosmosClient = new CosmosClient(cosmosDBConnectionString);

            // Get reference to database and container
            var database = cosmosClient.GetDatabase("AzureServerlessCV");
            var container = database.GetContainer("SiteCounter");

            // Read site counter value from blob
            string siteCounterValue = await siteCounterBlob.DownloadTextAsync();

            // Create site counter object
            SiteCounter siteCounter = new SiteCounter();
            siteCounter.Counter = int.Parse(siteCounterValue);

            // Add site counter object to CosmosDB
            await container.UpsertItemAsync<SiteCounter>(siteCounter, new PartitionKey(siteCounter.id));
        }
    }
}
