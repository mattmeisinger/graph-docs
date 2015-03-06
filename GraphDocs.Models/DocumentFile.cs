using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.Models
{
    public class DocumentFile
    {
        public string FilePath { get; set; }
        public byte[] Data { get; set; }
        public string MimeType { get; set; }
    }
}
