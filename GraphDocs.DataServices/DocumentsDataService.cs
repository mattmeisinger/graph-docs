using GraphDocs.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.DataServices
{
    public class DocumentsDataService
    {
        private GraphClient client;

        public DocumentsDataService()
        {
            client = DatabaseService.GetConnection();
        }

        public Document Get(string path)
        {
            var documentId = GetIDFromFolderPath(path);
            var document = client.Cypher
                .WithParams(new { documentId })
                .Match("(d:Document { ID: {documentId} })")
                .Return<Document>("d")
                .Results
                .SingleOrDefault();

            if (document == null)
                return null;

            document.Path = path;
            document.Tags = client.Cypher
                .WithParams(new { documentId })
                .Match("(:Document { ID: {documentId} })<-[:CHILD_OF]-(child:Tag)")
                .Return<Tag>("child")
                .Results
                .ToArray();

            return document;
        }

        public void Delete(string path)
        {
            var id = GetIDFromFolderPath(path);

            // First delete this folders relationships to parents (probably only one, it's parent folder)
            client.Cypher
                .WithParams(new
                {
                    folderId = id
                })
                .Match("(f:Folder { ID: {folderId} })-[rel]->()")
                .Delete("rel")
                .ExecuteWithoutResults();

            // Then delete all descendants and connecting relations
            client.Cypher
                .WithParams(new
                {
                    folderId = id
                })
                .Match("(f:Folder { ID: {folderId} })<-[rels*0..]-(child:Folder)")
                .ForEach("(rel in rels | delete rel)")
                .Delete("f, child")
                .ExecuteWithoutResults();
        }

        public void Create(Folder folder)
        {
            if (folder.Name == "Root")
                throw new Exception("'Root' is a reserved folder name and cannot be used.");
            if (string.IsNullOrWhiteSpace(folder.Name))
                throw new Exception("Folder name is required.");

            if (folder.ID == null)
                folder.ID = Guid.NewGuid().ToString();
            var parentId = GetIDFromFolderPath(folder.Path);

            // Attach to root folder
            client.Cypher
                .WithParams(new
                {
                    parentId = parentId,
                    newFolder = new
                    {
                        folder.Name,
                        folder.ID
                    }
                })
                .Match("(parent:Folder { ID: {parentId} })")
                .Create("parent<-[:CHILD_OF]-(folder:Folder {newFolder})")
                .ExecuteWithoutResults();
        }

        public void Save(Folder folder)
        {
            client.Cypher
                .WithParams(new
                {
                    folderId = folder.ID,
                    folder = folder
                })
                .Match("(folder:Folder { ID: {folderId} })")
                .Set("folder = {folder}")
                .ExecuteWithoutResults();
        }

        public string GetIDFromDocumentPath(string path)
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
        public void DeleteAll()
        {
            client.Cypher
                .Match("(folder:Folder)-[r]-()")
                .Delete("folder, r")
                .ExecuteWithoutResults();
            client.Cypher
                .Match("(folder:Folder)")
                .Delete("folder")
                .ExecuteWithoutResults();
            //((IRawGraphClient)client).ExecuteCypher(new Neo4jClient.Cypher.CypherQuery("MATCH (folder:Folder)-[r]-() DELETE folder, r", new Dictionary<string, object>(), Neo4jClient.Cypher.CypherResultMode.Projection));
        }
    }
}
