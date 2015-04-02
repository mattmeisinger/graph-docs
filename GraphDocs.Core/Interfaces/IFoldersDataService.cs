using GraphDocs.Core.Models;

namespace GraphDocs.Core.Interfaces
{
    public interface IFoldersDataService
    {
        void Create(Folder f);
        void Delete(string path);
        Folder Get(string path);
        void Save(Folder f);
    }
}