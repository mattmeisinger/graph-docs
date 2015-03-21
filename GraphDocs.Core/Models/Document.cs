namespace GraphDocs.Core.Models
{
    public class Document
    {
        public bool HasFile { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string[] Tags { get; set; }
    }
}
