using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MyFunctionApp
{
    public static class QueueTriggerFunction
    {
        [FunctionName("QueueTriggerFunction")]
        public static void Run([QueueTrigger("queue-name", Connection = "BlobContainerConnectionString")]string myQueueItem, ILogger log,
            [CosmosDB(databaseName: "AzureServerlessCV", collectionName: "FormSubmission", ConnectionStringSetting = "DatabaseConnectionString")] out dynamic document)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            // Parse the message from the queue
            string[] messageParts = myQueueItem.Split(' ');
            string name = messageParts[0];
            string email = messageParts[1];

            // Insert a new document into the "FormSubmission" container
            document = new { name = name, email = email };
        }
    }
}
