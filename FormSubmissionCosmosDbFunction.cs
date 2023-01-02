using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace MyFunctionApp
{
    public static class QueueTriggerFunction
    {
        [FunctionName("QueueTriggerFunction")]
        public static void Run([QueueTrigger("queue-name", Connection = "BlobContainerConnectionString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            // Parse the message from the queue
            string[] messageParts = myQueueItem.Split(' ');
            string name = messageParts[0];
            string email = messageParts[1];

            // Get the CosmosDB connection string from the app settings
            string cosmosDbConnectionString = Environment.GetEnvironmentVariable("DatabaseConnectionString");

            // Connect to the CosmosDB database using the connection string
            CosmosClient client = new CosmosClient(cosmosDbConnectionString);

            // Get the "FormSubmission" container
            Container container = client.GetContainer("AzureServerlessCV", "FormSubmission");

            // Insert a new document into the container
            container.CreateItemAsync(new { name = name, email = email }).Wait();
        }
    }
}
