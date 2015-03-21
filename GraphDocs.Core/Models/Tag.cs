namespace GraphDocs.Core.Models
{
    public class Tag
    {
        public string Name { get; set; }
        public Document[] RelatedDocuments { get; set; }
    }
}
