using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;

namespace FormSubmission
{
    public static class FormSubmissionSendGridFunction
    {
        [FunctionName("FormSubmissionSendGridFunction")]
        public static void Run([CosmosDBTrigger(databaseName: "AzureServerlessCV", collectionName: "FormSubmission", ConnectionStringSetting = "DatabaseConnectionString")] IReadOnlyList<dynamic> documents, ILogger log)
        {
            if (documents != null && documents.Count > 0)
            {
                // Get the first document from the trigger
                dynamic document = documents[0];

                // Get the "name" and "email" fields from the document
                string name = document.name;
                string email = document.email;

                // Set up the email message
                SendGridMessage message = new SendGridMessage();
                message.AddTo(email);
                message.SetFrom("contact@richardcoffey.com");
                message.SetSubject("New form submission");
                message.AddContent(MimeType.Text, $"Hi {name},\n\nThank you for submitting the form.\n\nBest regards,\n\nExample Team");

                // Get the SendGrid API key from the app settings
                string apiKey = Environment.GetEnvironmentVariable("SendGridApiKey");

                // Create a SendGrid client
                SendGridClient client = new SendGridClient(apiKey);

                // Send the email
                log.LogInformation("Sending email to " + email);
                client.SendEmailAsync(message).Wait();
                log.LogInformation("Email sent successfully");
            }
        }
    }
}
