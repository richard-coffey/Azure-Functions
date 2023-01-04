using NUnit.Framework;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using NSubstitute;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using System;
using System.Threading.Tasks;

namespace FormSubmission.Tests
{
    [TestFixture]
    public class FormSubmissionSendGridFunctionTests
    {
        [Test]
        public void Test_FormSubmissionSendGridFunction_Run()
        {
            // Arrange
            ILogger fakeLogger = Substitute.For<ILogger>();

            dynamic document = new
            {
                name = "John Smith",
                email = "johnsmith@example.com"
            };
            IReadOnlyList<dynamic> documents = new List<dynamic> { document };

            // Set up the mock BlobClient
            var mockBlobClient = Substitute.For<CloudBlobClient>();
            CloudBlobContainer container = Substitute.For<CloudBlobContainer>(new Uri("https://fakeuri.com"));
            mockBlobClient.GetContainerReference("cv-pdf").Returns(container);

            // Set up the mock Container
            CloudBlockBlob blob = Substitute.For<CloudBlockBlob>(new Uri("https://fakeuri.com/blob"));
            container.GetBlockBlobReference("Richard Coffey - CV.pdf").Returns(blob);

            // Set up the mock StorageAccount
            var mockStorageAccount = Substitute.For<CloudStorageAccount>();
            mockStorageAccount.CreateCloudBlobClient().Returns(mockBlobClient);

            // Set up the mock SendGridClient
            var mockSendGridClient = Substitute.For<ISendGridClient>();
            mockSendGridClient.SendEmailAsync(Arg.Any<SendGridMessage>()).Returns(Task.FromResult(new Response(System.Net.HttpStatusCode.OK, null, null)));

            // Act
            FormSubmissionSendGridFunction.Run(documents, fakeLogger);

            // Assert
            mockBlobClient.Received(1).GetContainerReference("cv-pdf");
            container.Received(1).GetBlockBlobReference("Richard Coffey - CV.pdf");
            mockSendGridClient.Received(1).SendEmailAsync(Arg.Any<SendGridMessage>());
        }
    }
}