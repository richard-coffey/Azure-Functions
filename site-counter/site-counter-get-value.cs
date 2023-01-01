using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace SiteCounter
{
    public static class GetCounterValueFunction
    {
        private static int lastGoodCounterValue = 0;

        [FunctionName("GetCounterValueFunction")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequest req, ILogger log)
        {
            // Read the value of the counter from the global variable
            int counterValue = CosmosDBCounterTriggerFunction.CounterValue;

            // If the counter value is 0, return the last good value instead
            if (counterValue == 0)
            {
                return new OkObjectResult(lastGoodCounterValue);
            }

            // Update the last good value
            lastGoodCounterValue = counterValue;

            // Log the counter value
            log.LogInformation($"Counter value: {counterValue}");

            // Return the counter value in the response body
            return new OkObjectResult(counterValue);
        }
    }
}
