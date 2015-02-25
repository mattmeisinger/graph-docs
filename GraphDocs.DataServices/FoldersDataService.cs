using GraphDocs.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.DataServices
{
    public class FoldersDataService
    {
        private GraphClient client;
        public FoldersDataService()
        {
            client = DatabaseService.GetConnection();
        }

        public Folder Get(string path = "/")
        {
            if (string.IsNullOrWhiteSpace(path))
                path = "/";

            var pathPieces = path.Split('/')
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .ToArray();

            var intermediateFolders = pathPieces.Select((a, i) => new { FolderName = a, ParamName = "folder" + i }).ToArray();

            // Match at least the root node
            var matchString = "(root:Folder{Name:\"Root\"})";
            if (intermediateFolders.Any())
                matchString += string.Join("", intermediateFolders.Select(a => "<-[:CHILD_OF]-(" + a.ParamName + ":Folder{Name:{" + a.ParamName + "}})"));

            // Get documents that are children of the last folder
            matchString += "<-[:CHILD_OF]-(doc:Document)";

            // Return the last folder in the list (if any are in the list). If the list is empty, return the root node.
            var paramToReturn = intermediateFolders.Any() ? intermediateFolders.Last().ParamName : "root";

            var folder = client.Cypher
                .Match(matchString)
                .WithParams(intermediateFolders.ToDictionary(a => a.ParamName, a => (object)a.FolderName))
                .Return<Folder>(paramToReturn)
                .Results
                .SingleOrDefault();

            if (folder == null)
                return null;

            folder.Path = "/" + string.Join("/", pathPieces);
            folder.ChildFolders = client.Cypher
                .Match(matchString + "<-[:CHILD_OF]-(child:Folder)")
                .WithParams(intermediateFolders.ToDictionary(a => a.ParamName, a => (object)a.FolderName))
                .Return((child, docs) => child.As<Folder>())
                .Results
                .ToArray();
            foreach (var item in folder.ChildFolders)
                item.Path = folder.Path + "/" + item.Name;

            folder.ChildDocuments = client.Cypher
                .Match(matchString + "<-[:CHILD_OF]-(child:Document)")
                .WithParams(intermediateFolders.ToDictionary(a => a.ParamName, a => (object)a.FolderName))
                .Return(child => child.As<Document>())
                .Results
                .ToArray();

            return folder;
        }

        public void Create(string folderName, string parentFolderPath)
        {
            if (folderName == "Root")
                throw new Exception("'Root' is a reserved folder name and cannot be used.");
            if (string.IsNullOrWhiteSpace(parentFolderPath) || parentFolderPath.Trim() == "/")
            {
                // Attach to root folder
                client.Cypher
                    .Match("(root:Folder { Name: 'Root' })")
                    .Create("root<-[:CHILD_OF]-(folder:Folder {newFolder})")
                    .WithParam("newFolder", new { Name = folderName })
                    .ExecuteWithoutResults();
            }
            else
            {

            }
        }

        public int? GetNodeIdFromFolderPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Trim() == "/")
            {
                // Attach to root folder
                return client.Cypher
                    .Match("(root:Folder { Name: 'Root' })")
                    .Return<int?>("id(root)")
                    .Results
                    .SingleOrDefault();
            }
            else
            {
                var pathPieces = path.Split('/')
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .ToArray();

                var intermediateFolders = pathPieces.Select((a, i) => new { FolderName = a, ParamName = "folder" + i }).ToArray();

                // Match at least the root node
                var matchString = "(root:Folder{Name:\"Root\"})";
                if (intermediateFolders.Any())
                    matchString += string.Join("", intermediateFolders.Select(a => "<-[:CHILD_OF]-(" + a.ParamName + ":Folder{Name:{" + a.ParamName + "}})"));

                // Return the last folder in the list (if any are in the list). If the list is empty, return the root node.
                var paramToReturn = intermediateFolders.Any() ? intermediateFolders.Last().ParamName : "root";

                var folderId = client.Cypher
                    .Match(matchString)
                    .WithParams(intermediateFolders.ToDictionary(a => a.ParamName, a => (object)a.FolderName))
                    .Return<int?>("id(" + paramToReturn + ")")
                    .Results
                    .SingleOrDefault();

                return folderId;
            }
        }
    }
}
