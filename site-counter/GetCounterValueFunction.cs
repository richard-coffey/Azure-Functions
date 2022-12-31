using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace SiteCounter
{
    public static class GetCounterValueFunction
    {
        [FunctionName("GetCounterValueFunction")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequest req, ILogger log)
        {
            // Read the value of the counter from the global variable
            int counterValue = CosmosDBCounterTriggerFunction.CounterValue;

            // Return the counter value in the response body
            return new OkObjectResult(counterValue);
        }
    }
}
