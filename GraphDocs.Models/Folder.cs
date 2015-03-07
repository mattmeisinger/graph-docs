using System;
using System.Runtime.Serialization;

namespace GraphDocs.Models
{
    public class Folder
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Folder[] ChildFolders { get; set; }
        public Document[] ChildDocuments { get; set; }
    }
}
