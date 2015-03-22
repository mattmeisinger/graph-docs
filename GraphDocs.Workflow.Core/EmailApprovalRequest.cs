using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Net.Mail;
using GraphDocs.Core.Models;
using System.Configuration;
using System.Net;

namespace GraphDocs.Workflow.Core
{
    public sealed class EmailApprovalRequest : CodeActivity
    {
        /// <summary>
        /// Comma- or semicolon-separated list of email addresses.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> EmailRecipients { get; set; }

        [RequiredArgument]
        public InArgument<Document> Document { get; set; }

        public InArgument<DocumentFile> DocumentFile { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            var recipientEmailAddresses = context.GetValue(EmailRecipients)
                .Split(new[] { ',', ';' })
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => a.Trim())
                .ToArray();

            var document = context.GetValue(Document);
            var documentFile = context.GetValue(DocumentFile);
            foreach (var recipientEmailAddress in recipientEmailAddresses)
            {
                var to = recipientEmailAddress;
                var from = "GraphDocs@noreply.com";
                var subject = "Approval requested: " + document.Name;
                var body = "<p>Approval requested for GraphDocs document." +
                    "<br/>Name : " + document.Name +
                    "<br/>Path : " + document.Path +
                    "<br/>Tags : " + string.Join(", ", document.Tags) +
                    "</p>" +
                    "<p><a href=\"" + ConfigurationManager.AppSettings["SiteBaseUrl"] + "/Workflow/Approve?id=" + context.WorkflowInstanceId + "?approver=" + WebUtility.UrlEncode(to) + "\" style=\"font-weight: bold;\">Approve</a></p>" +
                    "<p><a href=\"" + ConfigurationManager.AppSettings["SiteBaseUrl"] + "/Workflow/Reject?id=" + context.WorkflowInstanceId + "?approver=" + WebUtility.UrlEncode(to) + "\">Reject</a></p>";

                var message = new System.Net.Mail.MailMessage(from, to, subject, body);
                message.IsBodyHtml = true;

                if (documentFile != null)
                {

                }

                SmtpClient client = new SmtpClient();
                client.Send(message);
            }
        }
    }
}
