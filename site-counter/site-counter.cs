using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;

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

            // URI of the Key Vault
            var BaseUri = "https://azure-serverless-cv.vault.azure.net";

            // Name of the secret
            var secretname = "BlobStorageConnectionString";

            // Retrieve the secret from the Key Vault
            var secret = await keyVaultClient.GetSecretAsync(BaseUri, secretname);

            // Get the storage connection string from the secret
            var storageConnectionString = secret.Value;

            // Connect to Azure Storage Account
            var storageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(storageConnectionString);

            // Get reference to container
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("site-counter");

            // Create container if it doesn't already exist
            await container.CreateIfNotExistsAsync();

            // Get current site counter from blob
            var siteCounter = 0;
            var siteCounterBlob = container.GetBlockBlobReference("site-counter/site-counter-value.txt");
            if (await siteCounterBlob.ExistsAsync())
            {
                // Increment site counter
                siteCounter = int.Parse(await siteCounterBlob.DownloadTextAsync()) + 1;
            }

            // Add new site counter value to blob
            await siteCounterBlob.UploadTextAsync(siteCounter.ToString());
        }
    }
}