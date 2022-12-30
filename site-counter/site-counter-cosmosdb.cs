using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;

namespace SiteCounter
{
    public class SiteCounter
    {
        public int Counter { get; set; }
        public string id { get; set; } = "1";
    }

    public static class SiteCounterCosmosDbFunction
    {
        [FunctionName("SiteCounterCosmosDbFunction")]
        public static async Task Run([BlobTrigger("site-counter/{name}", Connection = "BlobContainerConnectionString")] CloudBlockBlob siteCounterBlob, string name, ILogger log)
        {
            log.LogInformation("SiteCounterCosmosDbFunction function processed a request.");

            // Get the Azure AD token provider
            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            // Get a client for the Key Vault
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            // URI of the Key Vault
            var baseUri = "https://azure-serverless-cv.vault.azure.net";

            // Name of the storage blob connection string secret
            var storageBlobSecretName = "BlobContainerConnectionString";

            // Name of the CosmosDB connection string secret
            var cosmosDBSecretName = "DatabaseConnectionString";

            // Retrieve the storage blob connection string secret from the Key Vault
            var storageBlobSecret = await keyVaultClient.GetSecretAsync(baseUri, storageBlobSecretName);

            // Retrieve the CosmosDB connection string secret from the Key Vault
            var cosmosDBSecret = await keyVaultClient.GetSecretAsync(baseUri, cosmosDBSecretName);

            // Get the storage blob connection string from the secret
            var storageBlobConnectionString = storageBlobSecret.Value;

            // Get the CosmosDB connection string from the secret
            var cosmosDBConnectionString = cosmosDBSecret.Value;

            // Connect to CosmosDB
            CosmosClient cosmosClient = new CosmosClient(cosmosDBConnectionString);

            // Get reference to database and container
            var database = cosmosClient.GetDatabase("AzureServerlessCV");
            var container = database.GetContainer("SiteCounter");

            // Read site counter value from blob
            string siteCounterValue = await siteCounterBlob.DownloadTextAsync();

            // Create site counter object
            SiteCounter siteCounter = new SiteCounter();
            siteCounter.Counter = int.Parse(siteCounterValue);

            // Add site counter object to CosmosDB
            await container.UpsertItemAsync<SiteCounter>(siteCounter, new PartitionKey(siteCounter.id));      
        }
    }
}


