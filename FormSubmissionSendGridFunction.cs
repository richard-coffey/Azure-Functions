using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System.Collections.Generic;
using System.IO;

namespace FormSubmission
{
    public static class FormSubmissionSendGridFunction
    {
        [FunctionName("FormSubmissionSendGridFunction")]
        public static void Run([CosmosDBTrigger(databaseName: "AzureServerlessCV", collectionName: "FormSubmission", ConnectionStringSetting = "DatabaseConnectionString")] IReadOnlyList<dynamic> documents, ILogger log)
        {
            if (documents != null && documents.Count > 0)
            {
                log.LogInformation("Function triggered by new document in Cosmos DB");

                // Get the first document from the trigger
                dynamic document = documents[0];

                // Get the "name" and "email" fields from the document
                string name = document.name;
                string email = document.email;

                // Set up the email message
                SendGridMessage message = new SendGridMessage();
                message.AddTo(email);
                message.SetFrom("contact@richardcoffey.com");
                message.SetSubject("CV Request - Richard Coffey");
                message.AddContent(MimeType.Text, $"Hi {name},\n\nThank you for viewing my CV. You can find a copy of my CV as a PDF attached to this email. To contact me about any possible job opportunities then please reply to this email.\n\nBest regards,\n\nRichard Coffey");

                // Attach a file from blob storage to the email
                string storageConnectionString = Environment.GetEnvironmentVariable("BlobContainerConnectionString");
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("cv-pdf");
                CloudBlockBlob blob = container.GetBlockBlobReference("Richard Coffey - CV (December 2022).pdf");

                // Download the blob as a stream
                MemoryStream memoryStream = new MemoryStream();
                blob.DownloadToStreamAsync(memoryStream).Wait();

                // Convert the stream to a byte array
                memoryStream.Position = 0;
                byte[] blobBytes = memoryStream.ToArray();

                // Add the attachment to the email message
                message.AddAttachment(blob.Name, Convert.ToBase64String(blobBytes));

                // Get the SendGrid API key from the app settings
                string apiKey = Environment.GetEnvironmentVariable("SendGridApiKey");

                // Create a SendGrid client
                SendGridClient client = new SendGridClient(apiKey);

                // Send the email
                client.SendEmailAsync(message).Wait();

                log.LogInformation("Email sent successfully");
            }
        }
    }
}
