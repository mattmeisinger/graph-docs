using System;
using System.Runtime.Serialization;

namespace GraphDocs.Models
{
    [DataContract]
    public class Tag
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Path
        {
            get
            {
                return "/tags/" + Name;
            }
        }
    }
}
