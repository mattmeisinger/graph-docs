using System;
using System.Runtime.Serialization;

namespace GraphDocs.Models
{
    [DataContract]
    public class Document
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string[] Tags { get; set; }
    }
}
