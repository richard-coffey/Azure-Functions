using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;

namespace FormSubmission
{
    public static class FormSubmissionFunction
    {
        [FunctionName("FormSubmissionFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string name = req.Form["name"];
            string email = req.Form["email"];

            log.LogInformation($"Received request with name: {name}, email: {email}");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                return new BadRequestObjectResult("Please provide both a name and an email.");
            }

            // Get the connection string for the storage account
            string connectionString = Environment.GetEnvironmentVariable("BlobContainerConnectionString");

            // Connect to the storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Get a reference to the queue
            CloudQueue queue = storageAccount.CreateCloudQueueClient().GetQueueReference("form-submission");

            // Create the queue if it doesn't already exist
            await queue.CreateIfNotExistsAsync();

            // Create a message with the name and email values
            string messageContent = $"name: {name}, email: {email}";
            CloudQueueMessage message = new CloudQueueMessage(messageContent);

            log.LogInformation($"Adding message to queue: {messageContent}");

            // Add the message to the queue
            await queue.AddMessageAsync(message);

            return new OkObjectResult("Thank you for your submission!");
        }
    }
}
