using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace MyFunctionApp
{
    public static class AddToQueue
    {
        [FunctionName("AddToQueue")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Get the name and email from the API
            string apiUrl = Environment.GetEnvironmentVariable("ApiUrl");
            using (var client = new WebClient())
            {
                string json = await client.DownloadStringTaskAsync(apiUrl);
                dynamic data = JObject.Parse(json);
                string name = data.name;
                string email = data.email;

                // Get the connection string for the queue storage account from app settings
                string connectionString = Environment.GetEnvironmentVariable("BlobContainerConnectionString");

                // Create a queue service client
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

                // Create a queue if it doesn't already exist
                string queueName = "form-submission";
                CloudQueue queue = queueClient.GetQueueReference(queueName);
                await queue.CreateIfNotExistsAsync();

                // Add the name and email to the queue as separate messages
                await queue.AddMessageAsync(new CloudQueueMessage(name));
                await queue.AddMessageAsync(new CloudQueueMessage(email));

                return new OkObjectResult("Successfully added name and email to queue!");
            }
        }
    }
}
