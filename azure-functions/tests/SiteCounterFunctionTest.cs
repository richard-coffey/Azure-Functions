using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;

namespace SiteCounter.Tests
{
    [TestClass]
    public class SiteCounterFunctionTests
    {
        [TestMethod]
        public async Task SiteCounterFunction_IncrementsSiteCounter()
        {
            // Arrange
            var req = new Mock<Microsoft.AspNetCore.Http.HttpRequest>();
            var log = new Mock<ILogger>();
            var context = new Mock<ExecutionContext>();

            // Set up mock environment variables
            var envVars = new System.Collections.Generic.Dictionary<string, string>
    {
        { "BlobContainerConnectionString", "UseDevelopmentStorage=true" }
    };
            context.Setup(c => c.FunctionDirectory).Returns(@"C:\temp\SiteCounter");

            // Set up mock Azure Storage Account
            var mockStorageAccount = new Mock<Microsoft.Azure.Storage.CloudStorageAccount>(MockBehavior.Strict);
            Microsoft.Azure.Storage.CloudStorageAccount storageAccount;
            Microsoft.Azure.Storage.CloudStorageAccount.TryParse("UseDevelopmentStorage=true", out storageAccount);
            var mockBlob = new Mock<Microsoft.Azure.Storage.Blob.CloudBlockBlob>(new System.Uri("http://tempuri.org"));
            mockStorageAccount
                .Setup(s => s.CreateCloudBlobClient())
                .Returns(() =>
                {
                    // Set up mock Azure Storage Blob Client
                    var mockBlobClient = new Mock<Microsoft.Azure.Storage.Blob.CloudBlobClient>(MockBehavior.Strict);
                    mockBlobClient
                        .Setup(c => c.GetContainerReference(It.IsAny<string>()))
                        .Returns(() =>
                        {
                            // Set up mock Azure Storage Container
                            var mockContainer = new Mock<Microsoft.Azure.Storage.Blob.CloudBlobContainer>(MockBehavior.Strict);
                            mockContainer
                                .Setup(c => c.CreateIfNotExistsAsync())
                                .ReturnsAsync(true);
                            mockContainer
                                .Setup(c => c.GetBlockBlobReference(It.IsAny<string>()))
                                .Returns(() =>
                                {
                                    mockBlob
                                        .Setup(b => b.ExistsAsync())
                                        .ReturnsAsync(true);
                                    mockBlob
                                        .Setup(b => b.UploadTextAsync(It.IsAny<string>()))
                                        .Returns(Task.CompletedTask);
                                    mockBlob
                                        .Setup(b => b.DownloadTextAsync())
                                        .ReturnsAsync("1");
                                    return mockBlob.Object;
                                });
                            return mockContainer.Object;
                        });
                    return mockBlobClient.Object;
                });

            // Act
            await SiteCounterFunction.Run(req.Object, log.Object, context.Object);

            // Assert
            mockBlob.Verify(b => b.UploadTextAsync("2"));
        }
    }
}