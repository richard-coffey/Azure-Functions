using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Cosmos;
using System;

namespace SiteCounter
{
    public static class GetCounterValueFunction
    {
        [FunctionName("GetCounterValueFunction")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            // Read the Cosmos DB connection string and database name from app settings
            string connectionString = Environment.GetEnvironmentVariable("DatabaseConnectionString");

            // Connect to Cosmos DB
            CosmosClient cosmosClient = new CosmosClient(connectionString);

            // Get reference to database and container
            var database = cosmosClient.GetDatabase("AzureServerlessCV");
            var container = database.GetContainer("SiteCounter");

            // Read the site counter document
            var siteCounterDoc = await container.ReadItemAsync<JObject>("1", new PartitionKey("1"));
            var siteCounter = siteCounterDoc.Resource;

            // Get the counter value
            int counterValue = siteCounter["Counter"].Value<int>();

            // Return the counter value in the response body
            return req.CreateResponse(System.Net.HttpStatusCode.OK, counterValue);
        }
    }
}
