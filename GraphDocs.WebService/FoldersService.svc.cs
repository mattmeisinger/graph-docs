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
        Folder[] GetRoot();

        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/{folderId}")]
        Folder Get(string folderId);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/{folderId}")]
        Folder Post(string folderId, Folder folder);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/{folderId}")]
        Folder Put(string folderId, Folder folder);

        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/{folderId}")]
        Folder Delete(string folderId);
    }

    public class FoldersService : IFoldersService
    {
        public Folder[] GetRoot()
        {
            return new[] {
                new Folder { Name = "Test", ParentFolderId = 0 },
                new Folder { Name = "Test2", ParentFolderId = 0 }
            };
        }

        public Folder Get(string folderId)
        {
            var id = Convert.ToInt32(folderId);
            return new Folder { Name = "Test", ParentFolderId = id };
        }

        public Folder Post(string folderId, Folder folder)
        {
            return folder;
        }

        public Folder Put(string folderId, Folder folder)
        {
            return folder;
        }

        public Folder Delete(string folderId)
        {
            return null;
        }
    }
}
