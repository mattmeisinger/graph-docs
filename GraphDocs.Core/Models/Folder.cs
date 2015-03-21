namespace GraphDocs.Core.Models
{
    public class Folder
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Folder[] ChildFolders { get; set; }
        public Document[] ChildDocuments { get; set; }

        public WorkflowRule WorkflowDefinitions { get; set; }
    }
}
