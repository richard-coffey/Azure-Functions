using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Cors;

namespace SiteCounter
{
    public static class GetCounterValueFunction
    {
        [FunctionName("GetCounterValueFunction")]
        [EnableCors("AllowAllOrigins")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequest req, ILogger log)
        {
            // Read the value of the counter from the global variable
            int counterValue = CosmosDBCounterTriggerFunction.CounterValue;

            // Log the counter value
            log.LogInformation($"Counter value: {counterValue}");

            // Return the counter value in the response body
            return new OkObjectResult(counterValue);
        }
    }
}
