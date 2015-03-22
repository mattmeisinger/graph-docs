using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.Core.Interfaces
{
    public interface ISettingsService
    {
        public string APIVersionNumber { get; set; }
        public string SiteBaseUrl { get; set; }
        public string SmtpServer { get; set; }
    }
}
