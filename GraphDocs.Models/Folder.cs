using System.Runtime.Serialization;

namespace GraphDocs.Models
{
    [DataContract]
    public class Folder
    {
        [DataMember]
        public string Name { get; set; }
    }
}
