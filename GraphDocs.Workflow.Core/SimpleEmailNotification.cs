using System;
using System.Collections.Generic;
using System.Linq;
using System.Activities;
using System.Configuration;

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
        public InArgument<string> Subject { get; set; }

        [RequiredArgument]
        public InArgument<string> Body { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            var to = context.GetValue(EmailRecipients);
            var from = ConfigurationManager.AppSettings["EmailFromAddress"];
            var subject = context.GetValue(Subject);
            var body = context.GetValue(Body);

            Utilities.Email.Send(from, to, subject, body, true);
        }
    }
}
