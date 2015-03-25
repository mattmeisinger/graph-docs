using GraphDocs.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.Core
{
    public class SettingsService : ISettingsService
    {
        public string APIVersionNumber { get; set; }
        public string SiteBaseUrl { get; set; }
        public string SmtpServer { get; set; }
        public string WorkflowFolder { get; set; }
        public Guid WorkflowStoreId { get; set; }
    }
}
