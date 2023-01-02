using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace FormSubmission
{
    public static class FormSubmissionCosmosDbFunction
    {
        [FunctionName("FormSubmissionCosmosDbFunction")]
        public static void Run([QueueTrigger("form-submission", Connection = "BlobContainerConnectionString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            // Parse the message from the queue
            string[] messageParts = myQueueItem.Split(' ');
            string name = messageParts[1];
            string email = messageParts[2];

            // Get the CosmosDB connection string from the app settings
            string cosmosDbConnectionString = Environment.GetEnvironmentVariable("DatabaseConnectionString");

            // Connect to the CosmosDB database using the connection string
            CosmosClient client = new CosmosClient(cosmosDbConnectionString);

            // Get the "FormSubmission" container
            Container container = client.GetContainer("AzureServerlessCV", "FormSubmission");

            // Generate a new GUID for the "id" field of the document
            string documentId = Guid.NewGuid().ToString();

            // Insert a new document into the container
            container.CreateItemAsync(new { id = documentId, name = name, email = email }).Wait();
        }
    }
}
