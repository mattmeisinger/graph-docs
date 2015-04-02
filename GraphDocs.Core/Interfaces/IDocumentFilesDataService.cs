using GraphDocs.Core.Models;

namespace GraphDocs.Core.Interfaces
{
    public interface IDocumentFilesDataService
    {
        void Create(DocumentFile d);
        void Delete(string path);
        DocumentFile Get(string path);
        void Save(DocumentFile d);
    }
}