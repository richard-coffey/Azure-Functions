using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FormSubmission
{
    public static class FormSubmissionFunction
    {
        [FunctionName("FormSubmissionFunction")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string name = req.Query["name"];
            string email = req.Query["email"];

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                return new BadRequestObjectResult("Please provide both a name and an email.");
            }

            // Process the form submission...

            return new OkObjectResult("Thank you for your submission!");
        }
    }
}
