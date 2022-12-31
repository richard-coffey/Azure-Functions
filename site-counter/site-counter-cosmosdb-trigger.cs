using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;

namespace SiteCounter
{
    public static class CosmosDBCounterTriggerFunction
    {
        // Declare a global variable to store the counter value
        public static int CounterValue;

        [FunctionName("CosmosDBCounterTriggerFunction")]
        public static void Run([CosmosDBTrigger(
            databaseName: "AzureServerlessCV",
            collectionName: "SiteCounter",
            ConnectionStringSetting = "DatabaseConnectionString",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true
        )]IReadOnlyList<Document> documents, ILogger log)
        {
            try
            {
                if (documents != null && documents.Count > 0)
                {
                    var document = documents[0];
                    dynamic counterObject = JsonConvert.DeserializeObject<dynamic>(document.ToString());

                    // Check if the document is the counter with an id of "1"
                    if (counterObject.id == "1")
                    {
                        // Store the counter value in the global variable
                        CounterValue = counterObject.Counter;

                        // Log the counter value
                        log.LogInformation($"Counter value: {CounterValue}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                // If an error occurs, log the error and return the last good value
                log.LogError(ex, "Error occurred while processing Cosmos DB trigger");
                CounterValue = CounterValue > 0 ? CounterValue : 0;
            }
        }
    }
}
