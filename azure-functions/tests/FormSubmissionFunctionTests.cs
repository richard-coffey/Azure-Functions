using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace FormSubmission.Tests
{
    public class FormSubmissionFunctionTests
    {
        [Test]
        public async Task Test_FormSubmissionFunction_ReturnsOkResult()
        {
            // Arrange
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("http://localhost/api/FormSubmissionFunction"),
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("name", "John Smith"),
                    new KeyValuePair<string, string>("email", "john@example.com")
                })
            };

            // Act
            IActionResult result = await FormSubmissionFunction.Run(request, null);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.AreEqual("Thank you for your submission!", ((OkObjectResult)result).Value);
        }

        [Test]
        public async Task Test_FormSubmissionFunction_ReturnsBadRequestResult()
        {
            // Arrange
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("http://localhost/api/FormSubmissionFunction"),
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("name", "")
                })
            };

            // Act
            IActionResult result = await FormSubmissionFunction.Run(request, null);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            Assert.AreEqual("Please provide both a name and an email.", ((BadRequestObjectResult)result).Value);
        }
    }
}