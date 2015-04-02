using System;
using System.Linq;
using System.Activities;
using GraphDocs.Core.Models;
using System.Configuration;
using System.Net;

namespace GraphDocs.Workflow.Core
{
    public sealed class ApproveDocument : NativeActivity<bool>
    {
        /// <summary>
        /// Comma- or semicolon-separated list of email addresses.
        /// </summary>
        public InArgument<string> EmailRecipients { get; set; }

        public InArgument<Document> Document { get; set; }

        public InArgument<DocumentFile> DocumentFile { get; set; }

        public InArgument<string> ApproverGroupName { get; set; }

        public InArgument<string> ReplyUrlTemplate { get; set; }

        // NativeActivity derived activities that do asynchronous operations by calling 
        // one of the CreateBookmark overloads defined on System.Activities.NativeActivityContext 
        // must override the CanInduceIdle property and return true.
        protected override bool CanInduceIdle { get { return true; } }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(NativeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            var to = context.GetValue(EmailRecipients);
            var approverGroupName = ApproverGroupName.Get(context);
            var bookmarkName = "Approval-" + approverGroupName;
            var document = context.GetValue(Document);
            var documentFile = context.GetValue(DocumentFile);
            var from = ConfigurationManager.AppSettings["EmailFromAddress"];
            var subject = "Approval requested: " + document.Name;

            var instanceId = context.WorkflowInstanceId;
            var replyUrlTemplate = context.GetValue(ReplyUrlTemplate);
            var body = "<p>Approval requested for GraphDocs document." +
                "<br/>Name : " + document.Name +
                "<br/>Path : " + document.Path +
                "<br/>Tags : " + string.Join(", ", document.Tags ?? new string[] { }) +
                "</p>" +
                "<p>Can be approved by: " + ApproverGroupName.Get(context) + "</p>" +
                "<p><a href=\"" + getReplyUrl(replyUrlTemplate, instanceId, bookmarkName, true) + "\" style=\"font-weight: bold;\">Approve</a></p>" +
                "<p><a href=\"" + getReplyUrl(replyUrlTemplate, instanceId, bookmarkName, false) + "\">Reject</a></p>";

            // TODO Attach file if it exists

            Utilities.Email.Send(from, to, subject, body, true);

            // Create a Bookmark and wait for it to be resumed.
            context.CreateBookmark(bookmarkName, new BookmarkCallback(OnResumeBookmark));
        }

        public void OnResumeBookmark(NativeActivityContext context, Bookmark bookmark, object obj)
        {
            // When the Bookmark is resumed, assign its value to the Result argument. Then 
            // we can use logic to branch based on whether it was approved or not.
            var isApproved = (bool)obj;
            Result.Set(context, isApproved);
        }

        private string getReplyUrl(string replyUrlTemplate, Guid instanceId, string bookmarkName, bool response)
        {
            if (string.IsNullOrWhiteSpace(replyUrlTemplate))
            {
                replyUrlTemplate = ConfigurationManager.AppSettings["SiteBaseUrl"] + "/workflowreply/{instanceId}/{bookmarkName}/{response}";
            }

            return replyUrlTemplate
                .Replace("{instanceId}", instanceId.ToString())
                .Replace("{bookmarkName}", bookmarkName)
                .Replace("{response}", response.ToString());
        }
    }
}
