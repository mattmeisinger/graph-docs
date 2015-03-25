﻿using System;
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
    public sealed class SimpleEmailNotification : CodeActivity
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
            var to = context.GetValue(EmailRecipients);
            var document = context.GetValue(Document);
            var documentFile = context.GetValue(DocumentFile);
            var from = "GraphDocs@noreply.com";
            var subject = "Approval requested: " + document.Name;
            var body = "<p>Approval requested for GraphDocs document." +
                "<br/>Name : " + document.Name +
                "<br/>Path : " + document.Path +
                "<br/>Tags : " + string.Join(", ", document.Tags) +
                "</p>";

            Utilities.Email.Send(from, to, subject, body, true);
        }
    }
}
