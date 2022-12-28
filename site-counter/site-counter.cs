using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Storage.Queue;
using System.Threading.Tasks;

namespace SiteCounter
{
    public static class SiteCounterFunction
    {
        [FunctionName("SiteCounterFunction")]
        public static async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] Microsoft.AspNetCore.Http.HttpRequest req, ILogger log)
        {
            log.LogInformation("SiteCounterFunction function processed a request.");

            // Get the Azure AD token provider
            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            // Get a client for the Key Vault
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            // Get the storage connection string from the Key Vault
            var storageConnectionStringSecret = await keyVaultClient.GetSecretAsync("https://azure-serverless-cv.vault.azure.net/secrets/QueueStorageConnectionString/253316b1740d4b01a02a307101d77ef0/1");
            var storageConnectionString = storageConnectionStringSecret.Value;

            // Connect to Azure Storage Account
            var storageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(storageConnectionString);

            // Get reference to queue
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("site-counter");

            // Create queue if it doesn't already exist
            await queue.CreateIfNotExistsAsync();

            // Get current site counter from queue
            var siteCounter = 0;
            var queueMessage = await queue.GetMessageAsync();
            if (queueMessage != null)
            {
                // Increment site counter
                siteCounter = int.Parse(queueMessage.AsString) + 1;

                // Delete existing queue message
                await queue.DeleteMessageAsync(queueMessage);
            }

            // Add new site counter value to queue
            queueMessage = new CloudQueueMessage(siteCounter.ToString());
            await queue.AddMessageAsync(queueMessage);
        }
    }
}
