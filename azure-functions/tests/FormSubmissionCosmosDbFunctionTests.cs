using NUnit.Framework;
using NSubstitute;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace FormSubmission.Tests
{
    [TestFixture]
    public class FormSubmissionCosmosDbFunctionTests
    {
        private class FormData
        {
            public string name { get; set; }
            public string email { get; set; }
        }

        [Test]
        public void Run_ValidInput_InsertsDocumentIntoContainer()
        {
            // Arrange
            string myQueueItem = "name: John, email: john@example.com";
            ILogger log = Substitute.For<ILogger>();
            string cosmosDbConnectionString = "FakeConnectionString";
            CosmosClient client = Substitute.For<CosmosClient>(cosmosDbConnectionString);
            Container container = Substitute.For<Container>("AzureServerlessCV", "FormSubmission");
            client.GetContainer("AzureServerlessCV", "FormSubmission").Returns(container);

            // Act
            FormSubmissionCosmosDbFunction.Run(myQueueItem, log);

            // Assert
            FormData formData = new FormData { name = "John", email = "john@example.com" };
            container.Received().CreateItemAsync(formData);
        }
    }
}