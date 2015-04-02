using GraphDocs.Core.Models;

namespace GraphDocs.Core.Interfaces
{
    public interface ITagsDataService
    {
        void Delete(string name);
        Tag[] Get();
        Tag Get(string name);
        Tag GetOrCreateTagByName(string name);
        void Rename(string oldName, string newName);
    }
}