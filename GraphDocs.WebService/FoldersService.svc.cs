using GraphDocs.DataServices;
using GraphDocs.Models;
using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace GraphDocs.WebService
{
    [ServiceContract]
    public interface IFoldersService
    {
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/")]
        Folder GetRoot();

        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/{path}")]
        Folder Get(string path);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/{path}")]
        Folder Post(string path, Folder folder);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/{path}")]
        Folder Put(string path, Folder folder);

        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/{path}")]
        void Delete(string path);
    }

    public class FoldersService : IFoldersService
    {
        FoldersDataService ds = new FoldersDataService();

        public Folder GetRoot()
        {
            return ds.Get("/");
        }

        public Folder Get(string path)
        {
            return ds.Get(path);
        }

        public Folder Post(string path, Folder folder)
        {
            folder.ID = null;
            folder.Path = path;
            ds.Save(folder);
            return folder;
        }

        public Folder Put(string path, Folder folder)
        {
            folder.Path = path;
            ds.Save(folder);
            return folder;
        }

        public void Delete(string path)
        {
            ds.Delete(path);
        }
    }
}
