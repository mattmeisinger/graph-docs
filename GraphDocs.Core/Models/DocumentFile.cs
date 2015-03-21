namespace GraphDocs.Core.Models
{
    public class DocumentFile
    {
        public byte[] Data { get; set; }
        public string MimeType { get; set; }
        public string DocumentId { get; set; }
        public string DocumentPath { get; set; }
    }
}
