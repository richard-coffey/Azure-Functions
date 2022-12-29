using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Queue;
using System;

namespace SiteCounter
{
    public class SiteCounter
    {
        public int Counter { get; set; }
        public string Id { get; set; }
    }

    public static class SiteCounterCosmosDbFunction
    {
        [FunctionName("SiteCounterCosmosDbFunction")]
        public static async Task Run([QueueTrigger("site-counter", Connection = "QueueStorageConnectionString")] CloudQueueMessage siteCounterMessage, ILogger log)
        {
            log.LogInformation("SiteCounterCosmosDbFunction function processed a request.");

            // Get the Azure AD token provider
            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            // Get a client for the Key Vault
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            // URI of the Key Vault
            var baseUri = "https://azure-serverless-cv.vault.azure.net";

            // Name of the storage queue connection string secret
            var storageQueueSecretName = "QueueStorageConnectionString";

            // Name of the CosmosDB connection string secret
            var cosmosDBSecretName = "DatabaseConnectionString";

            // Retrieve the storage queue connection string secret from the Key Vault
            var storageQueueSecret = await keyVaultClient.GetSecretAsync(baseUri, storageQueueSecretName);

            // Retrieve the CosmosDB connection string secret from the Key Vault
            var cosmosDBSecret = await keyVaultClient.GetSecretAsync(baseUri, cosmosDBSecretName);

            // Get the storage queue connection string from the secret
            var storageQueueConnectionString = storageQueueSecret.Value;

            // Get the CosmosDB connection string from the secret
            var cosmosDBConnectionString = cosmosDBSecret.Value;

            // Connect to CosmosDB
            CosmosClient cosmosClient = new CosmosClient(cosmosDBConnectionString);

            // Get reference to database and container
            var database = cosmosClient.GetDatabase("AzureServerlessCV");
            var container = database.GetContainer("SiteCounter");

            // Create site counter object
            SiteCounter siteCounter = new SiteCounter();
            siteCounter.Counter = int.Parse(siteCounterMessage.AsString);
            siteCounter.Id = System.Guid.NewGuid().ToString();

            // Add site counter object to CosmosDB
            await container.CreateItemAsync<SiteCounter>(siteCounter, new PartitionKey(siteCounter.Id));

            Console.WriteLine("Id: " + siteCounter.Id);
        }
    }
}


