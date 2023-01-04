using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Moq;
using System;
using System.Threading;

namespace SiteCounter.Tests
{
    [TestFixture]
    public class SiteCounterCosmosDbFunctionTests
    {
        public class FakeLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state)
            {
                // Implementation of the BeginScope<TState> method
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                // Implementation of the IsEnabled method
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                string logMessage = formatter(state, exception);
                Console.WriteLine($"[{logLevel}] {logMessage}");
            }
        }

        [Test]
        public async Task Test_SiteCounterCosmosDbFunction_Run()
        {
            // Define the mockContainer variable
            var mockContainer = new Mock<Container>();

            // Arrange
            CloudBlockBlob fakeSiteCounterBlob = new CloudBlockBlob(new Uri("https://localhost/fake/blob"));
            string fakeBlobName = "fake-blob-name";
            ILogger fakeLogger = new FakeLogger();

            // Act
            await SiteCounterCosmosDbFunction.Run(fakeSiteCounterBlob, fakeBlobName, fakeLogger);

            // Assert
            SiteCounter expectedSiteCounter = new SiteCounter { Counter = 0, id = "1" };
            SiteCounter actualSiteCounter = null;
            string actualPartitionKey = null;
            mockContainer.Setup(c => c.UpsertItemAsync(It.IsAny<SiteCounter>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>())).Returns((Task<ItemResponse<SiteCounter>>)Task.CompletedTask).Callback((SiteCounter sc, PartitionKey pk, ItemRequestOptions ro, CancellationToken ct) =>
            {
                actualSiteCounter = sc;
                actualPartitionKey = pk.ToString();
            });

            await SiteCounterCosmosDbFunction.Run(fakeSiteCounterBlob, fakeBlobName, fakeLogger);

            Assert.AreEqual(expectedSiteCounter, actualSiteCounter, "The UpsertItemAsync method of the container object was not called with the expected SiteCounter object.");
            Assert.AreEqual("1", actualPartitionKey, "The UpsertItemAsync method of the container object was not called with the expected partition key.");
        }
    }
}