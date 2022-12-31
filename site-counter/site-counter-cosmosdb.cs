using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;

namespace SiteCounter
{
    public class SiteCounter
    {
        public int Counter { get; set; }
        public string Id { get; set; } = "1";
    }

    public static class SiteCounterCosmosDbFunction
    {
        [FunctionName("SiteCounterCosmosDbFunction")]
        public static async Task Run([BlobTrigger("site-counter/{name}", Connection = "BlobContainerConnectionString")] CloudBlockBlob siteCounterBlob, string name, ILogger log,
            [CosmosDB(
                databaseName: "AzureServerlessCV",
                collectionName: "SiteCounter",
                ConnectionStringSetting = "DatabaseConnectionString"
            )] IAsyncCollector<SiteCounter> siteCounterCollector)
        {
            log.LogInformation("SiteCounterCosmosDbFunction function processed a request.");

            // Read site counter value from blob
            string siteCounterValue = await siteCounterBlob.DownloadTextAsync();

            // Create site counter object
            SiteCounter siteCounter = new SiteCounter();
            siteCounter.Counter = int.Parse(siteCounterValue);

            // Add site counter object to CosmosDB
            await siteCounterCollector.AddAsync(siteCounter);
        }
    }
}


