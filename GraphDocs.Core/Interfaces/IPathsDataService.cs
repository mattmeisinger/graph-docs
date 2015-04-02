namespace GraphDocs.Core.Interfaces
{
    public interface IPathsDataService
    {
        string GetIDFromDocumentPath(string path);
        string GetIDFromFolderPath(string path);
        string GetPathFromDocumentID(string id);
    }
}