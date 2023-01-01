using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace SiteCounter
{
    public static class GetCounterValueFunction
    {
        [FunctionName("GetCounterValueFunction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req, ILogger log)
        {
            log.LogInformation("GetCounterValueFunction function started");

            // Create an HTTP client
            HttpClient client = new HttpClient();

            // Set the request URI and headers
            string requestUri = "https://cosmosdb-azure-serverless-cv.documents.azure.com:443/dbs/AzureServerlessCV/colls/SiteCounter/docs/1";
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            // Get an access token for the Cosmos DB resource using managed identity
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://cosmosdb.azure.com/");

            // Add the access token to the "authorization" header
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {accessToken}");

            // Send the request and get the response
            HttpResponseMessage response = await client.GetAsync(requestUri);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Parse the response content and get the counter value
            JObject responseObject = JObject.Parse(responseContent);
            int counterValue = (int)responseObject["Counter"];

            // Return the counter value in the response body
            return req.CreateResponse(System.Net.HttpStatusCode.OK, counterValue);
        }
    }
}
