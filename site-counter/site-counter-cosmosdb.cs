using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;

namespace SiteCounter
{
    public class SiteCounter
    {
        public int Counter { get; set; }
        public string id { get; set; } = "1";
    }

    public static class SiteCounterCosmosDbFunction
    {
        [FunctionName("SiteCounterCosmosDbFunction")]
        public static async Task Run([BlobTrigger("site-counter/{name}", Connection = "BlobContainerConnectionString")] CloudBlockBlob siteCounterBlob, string name, ILogger log, ExecutionContext context)
        {
            log.LogInformation("SiteCounterCosmosDbFunction function processed a request.");

            // Build a configuration object from the app settings
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Get the storage blob connection string from the app settings
            var storageBlobConnectionString = config.GetConnectionString("BlobContainerConnectionString");

            // Get the CosmosDB connection string from the app settings
            var cosmosDBConnectionString = config.GetConnectionString("DatabaseConnectionString");

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


