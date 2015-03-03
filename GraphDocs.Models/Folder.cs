using System;
using System.Runtime.Serialization;

namespace GraphDocs.Models
{
    [DataContract]
    public class Folder
    {
        public string FolderID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public Folder[] ChildFolders { get; set; }

        [DataMember]
        public Document[] ChildDocuments { get; set; }
    }
}
