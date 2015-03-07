using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.Models
{
    public class DocumentFile
    {
        public byte[] Data { get; set; }
        public string MimeType { get; set; }
        public string DocumentId { get; set; }
        public string DocumentPath { get; set; }
    }
}
