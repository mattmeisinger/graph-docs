using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.Models
{
    [DataContract]
    public class Document
    {
        [DataMember]
        public string Filename { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public Tag[] Tags { get; set; }
    }
}
