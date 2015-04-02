using GraphDocs.Core.Models;

namespace GraphDocs.Core.Interfaces
{
    public interface IDocumentsDataService
    {
        void Create(Document d);
        void Delete(string path);
        Document GetByID(string documentId);
        Document GetByPath(string path);
        void Save(Document d);
    }
}