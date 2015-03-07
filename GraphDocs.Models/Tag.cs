using System;
using System.Runtime.Serialization;

namespace GraphDocs.Models
{
    public class Tag
    {
        public string Name { get; set; }
        public Document[] RelatedDocuments { get; set; }
    }
}
