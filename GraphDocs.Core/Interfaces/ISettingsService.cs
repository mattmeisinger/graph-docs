using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.Core.Interfaces
{
    public interface ISettingsService
    {
        string APIVersionNumber { get; set; }
        string SiteBaseUrl { get; set; }
        string SmtpServer { get; set; }
    }
}
