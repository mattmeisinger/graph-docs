using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.DataServices
{
    public class PathsDataService
    {
        private GraphClient client;

        public PathsDataService()
        {
            this.client = DatabaseService.GetConnection();
        }

        public PathsDataService(GraphClient client)
        {
            this.client = client;
        }

        public string GetIDFromFolderPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Trim() == "/")
            {
                // Attach to root folder
                var rootId = client.Cypher
                    .Match("(root:Folder { Name: \"Root\" })")
                    .Return<string>("root.ID")
                    .Results
                    .SingleOrDefault();
                if (rootId == null)
                    throw new Exception("Root folder not found.");
                else
                    return rootId;
            }
            else
            {
                var pathPieces = path.Split('/')
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .ToArray();

                var intermediateFolders = pathPieces.Select((a, i) => new { FolderName = a, ParamName = "folder" + i }).ToArray();

                // Match at least the root node
                var matchString = "(root:Folder { Name: \"Root\" })";
                if (intermediateFolders.Any())
                    matchString += string.Join("", intermediateFolders.Select(a => "<-[:CHILD_OF]-(" + a.ParamName + ":Folder{Name:{" + a.ParamName + "}})"));

                // Return the last folder in the list (if any are in the list). If the list is empty, return the root node.
                var paramToReturn = intermediateFolders.Any() ? intermediateFolders.Last().ParamName : "root";

                var folderId = client.Cypher
                    .Match(matchString)
                    .WithParams(intermediateFolders.ToDictionary(a => a.ParamName, a => (object)a.FolderName))
                    .Return<string>(paramToReturn + ".ID")
                    .Results
                    .SingleOrDefault();

                return folderId;
            }
        }


        public string GetIDFromDocumentPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Trim() == "/")
                return null;

            var pathPieces = path.Split('/')
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .ToArray();

            // All except for the last path elements are folders. The last one is the filename.
            var intermediateFolders = pathPieces.Reverse().Skip(1).Reverse().Select((a, i) => new { FolderName = a, ParamName = "folder" + i }).ToArray();

            // The last item is the document
            var documentName = pathPieces.Reverse().Take(1).SingleOrDefault();
            if (string.IsNullOrWhiteSpace(documentName))
                return null;

            // Match the list of folders
            var matchString = "(root:Folder { Name: \"Root\" })";
            if (intermediateFolders.Any())
                matchString += string.Join("", intermediateFolders.Select(a => "<-[:CHILD_OF]-(" + a.ParamName + ":Folder{Name:{" + a.ParamName + "}})"));

            // And finally match the document
            matchString += "<-[:CHILD_OF]-(doc:Document{Name:{docName}})";

            // Set parameters
            var p = intermediateFolders.ToDictionary(a => a.ParamName, a => (object)a.FolderName);
            p.Add("docName", documentName);

            var documentId = client.Cypher
                .Match(matchString)
                .WithParams(p)
                .Return<string>("doc.ID")
                .Results
                .SingleOrDefault();

            return documentId;
        }
    }
}
