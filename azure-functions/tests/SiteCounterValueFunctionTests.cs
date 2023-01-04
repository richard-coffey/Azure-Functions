using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace SiteCounter.Tests
{
    public class SiteCounterValueFunctionTests
    {
        [Test]
        public async Task Test_SiteCounterValueFunction()
        {
            // Arrange
            HttpRequestMessage req = new HttpRequestMessage();
            ILogger log = new LoggerFactory().CreateLogger("SiteCounterValueFunctionTests");

            // Act
            IActionResult result = await SiteCounterValueFunction.Run(req, log);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result, "The result of the SiteCounterValueFunction function should be an OkObjectResult object.");
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.IsInstanceOf<int>(okResult.Value, "The payload of the OkObjectResult should be an integer value.");
        }
    }
}