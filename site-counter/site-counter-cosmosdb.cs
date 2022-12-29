using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Queue;

namespace SiteCounter
{
    public class SiteCounter
    {
        public int Counter { get; set; }
    }
    public static class SiteCounterCosmosDbFunction
    {
        [FunctionName("SiteCounterCosmosDbFunction")]
        public static async Task Run([QueueTrigger("site-counter", Connection = "%QueueStorageConnectionString%")] CloudQueueMessage siteCounterMessage, ILogger log)
        {
            log.LogInformation("SiteCounterCosmosDbFunction function processed a request.");

            // Get the CosmosDB connection string from the function app configuration settings
            var cosmosDBConnectionString = System.Environment.GetEnvironmentVariable("DatabaseConnectionString");

            // Connect to CosmosDB
            CosmosClient cosmosClient = new CosmosClient(cosmosDBConnectionString);

            // Get reference to database and container
            var database = cosmosClient.GetDatabase("AzureServerlessCV");
            var container = database.GetContainer("SiteCounter");

            // Convert site counter message to site counter object
            SiteCounter siteCounter = new SiteCounter
            {
                Counter = int.Parse(siteCounterMessage.AsString)
            };

            // Add site counter object to CosmosDB
            await container.CreateItemAsync<SiteCounter>(siteCounter, new PartitionKey(siteCounter.Counter.ToString()));
        }
    }
}


