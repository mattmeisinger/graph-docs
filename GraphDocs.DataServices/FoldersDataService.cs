using GraphDocs.DataServices.Utilities;
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
    public class FoldersDataService
    {
        private GraphClient client;
        private PathsDataService paths;

        public FoldersDataService()
        {
            client = DatabaseService.GetConnection();
            paths = new PathsDataService(client);
        }

        public Folder Get(string path)
        {
            path = PathUtilities.ReformatPath(path);
            var folderId = paths.GetIDFromFolderPath(path);
            var folder = client.Cypher
                .WithParams(new { folderId })
                .Match("(folder:Folder { ID: {folderId} })")
                .Return<Folder>("folder")
                .Results
                .SingleOrDefault();

            if (folder == null)
                return null;

            folder.Path = path;
            folder.ChildFolders = client.Cypher
                .WithParams(new { folderId })
                .Match("(folder:Folder { ID: {folderId} })<-[:CHILD_OF]-(child:Folder)")
                .Return(child => child.As<Folder>())
                .Results
                .ToArray();
            foreach (var item in folder.ChildFolders)
                item.Path = PathUtilities.Join(folder.Path, item.Name);

            folder.ChildDocuments = client.Cypher
                .WithParams(new { folderId })
                .Match("(folder:Folder { ID: {folderId} })<-[:CHILD_OF]-(child:Document)")
                .Return(child => child.As<Document>())
                .Results
                .ToArray();
            foreach (var item in folder.ChildDocuments)
                item.Path = PathUtilities.Join(folder.Path, item.Name);

            return folder;
        }

        public void Delete(string path)
        {
            path = PathUtilities.ReformatPath(path);
            var id = paths.GetIDFromFolderPath(path);
            if (id == null)
                throw new Exception("Unable to delete, folder path not found: " + path);

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
            folder.Path = PathUtilities.ReformatPath(folder.Path);

            // Check to see if folder already exists, and throw an exception if so.
            var pathToCreate = PathUtilities.Join(folder.Path, folder.Name);
            var existingId = paths.GetIDFromFolderPath(pathToCreate);
            if (existingId != null)
                throw new Exception("Folder already exists.");

            if (folder.ID == null)
                folder.ID = Guid.NewGuid().ToString();
            var parentId = paths.GetIDFromFolderPath(folder.Path);

            // Attach to root folder
            client.Cypher
                .WithParams(new
                {
                    parentId = parentId,
                    newFolder = new
                    {
                        folder.ID,
                        folder.Name
                    }
                })
                .Match("(parent:Folder { ID: {parentId} })")
                .Create("parent<-[:CHILD_OF]-(folder:Folder {newFolder})")
                .ExecuteWithoutResults();
        }

        public void Save(Folder f)
        {
            f.Path = PathUtilities.ReformatPath(f.Path);

            // If the ID is passed in, save based on that. Otherwise, save based on the path passed in.
            f.ID = f.ID ?? paths.GetIDFromFolderPath(f.Path);

            client.Cypher
                .WithParams(new
                {
                    folderId = f.ID,
                    folder = new
                    {
                        f.ID,
                        f.Name
                    }
                })
                .Match("(folder:Folder { ID: {folderId} })")
                .Set("folder = {folder}")
                .ExecuteWithoutResults();
        }
    }
}
