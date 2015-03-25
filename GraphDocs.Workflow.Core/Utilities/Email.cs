using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.Workflow.Core.Utilities
{
    public class Email
    {
        /// <summary>
        /// Send an email. Multiple recipients may be specified in the 'to' string.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isHtml"></param>
        /// <param name="attachment"></param>
        /// <param name="attachmentFilename"></param>
        public static void Send(string from, string to, string subject, string body, bool isHtml, byte[] attachment = null, string attachmentFilename = null)
        {
            var message = new System.Net.Mail.MailMessage(from, to, subject, body);
            message.IsBodyHtml = isHtml;

            if (attachment != null)
            {

            }

            SmtpClient client = new SmtpClient("localhost");
            client.Send(message);
        }

        /// <summary>
        /// Send emails to each person, but send them each separately so they can't see who the other recipients are.
        /// </summary>
        public static void SendIndividually(string from, string to, string subject, string body, bool isHtml, byte[] attachment = null, string attachmentFilename = null)
        {
            var recipientEmailAddresses = to
                .Split(new[] { ',', ';' })
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => a.Trim())
                .ToArray();
            foreach (var address in recipientEmailAddresses)
            {
                Send(from, address, subject, body, isHtml, attachment, attachmentFilename);
            }
        }
    }
}
