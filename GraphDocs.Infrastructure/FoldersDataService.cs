using System;
using System.Linq;
using GraphDocs.Infrastructure.Utilities;
using GraphDocs.Core.Models;
using Neo4jClient;
using GraphDocs.Infrastructure.Database;
using System.Dynamic;

namespace GraphDocs.Infrastructure
{
    public class FoldersDataService
    {
        private IGraphClient client;
        private PathsDataService paths;

        public FoldersDataService()
        {
            client = Neo4jConnectionFactory.GetConnection();
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

            folder.WorkflowDefinitions = client.Cypher
                .WithParams(new { folderId })
                .Match("(folder:Folder { ID: {folderId} })<-[:APPLIES_TO]-(wd:WorkflowDefinition)")
                .Return(wd => wd.As<WorkflowDefinition>())
                .Results
                .ToArray();

            return folder;
        }

        public void Delete(string path)
        {
            path = PathUtilities.ReformatPath(path);
            var folderId = paths.GetIDFromFolderPath(path);
            if (folderId == null)
                throw new Exception("Unable to delete, folder path not found: " + path);

            // First delete this folders relationships to parents (probably only one, it's parent folder)
            client.Cypher
                .WithParams(new
                {
                    folderId = folderId
                })
                .Match("(f:Folder { ID: {folderId} })-[rel]->()")
                .Delete("rel")
                .ExecuteWithoutResults();

            // Then delete all descendants and connecting relations
            client.Cypher
                .WithParams(new
                {
                    folderId = folderId
                })
                .Match("(f:Folder { ID: {folderId} })<-[rels*0..]-(child:Folder)")
                .ForEach("(rel in rels | delete rel)")
                .Delete("f, child")
                .ExecuteWithoutResults();

            deleteWorkflowDefinitions(folderId);
        }

        public void Create(Folder f)
        {
            if (f.Name == "Root")
                throw new Exception("'Root' is a reserved folder name and cannot be used.");
            if (string.IsNullOrWhiteSpace(f.Name))
                throw new Exception("Folder name is required.");
            f.Path = PathUtilities.ReformatPath(f.Path);

            // Check to see if folder already exists, and throw an exception if so.
            var pathToCreate = PathUtilities.Join(f.Path, f.Name);
            var existingId = paths.GetIDFromFolderPath(pathToCreate);
            if (existingId != null)
                throw new Exception("Folder already exists.");

            if (f.ID == null)
                f.ID = Guid.NewGuid().ToString();
            var parentId = paths.GetIDFromFolderPath(f.Path);

            // Attach to root folder
            client.Cypher
                .WithParams(new
                {
                    parentId = parentId,
                    newFolder = new
                    {
                        f.ID,
                        f.Name
                    }
                })
                .Match("(parent:Folder { ID: {parentId} })")
                .Create("parent<-[:CHILD_OF]-(folder:Folder {newFolder})")
                .ExecuteWithoutResults();

            saveWorkflowDefinitions(f.ID, f.WorkflowDefinitions);
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

            saveWorkflowDefinitions(f.ID, f.WorkflowDefinitions);
        }

        private void saveWorkflowDefinitions(string folderId, WorkflowDefinition[] workflowDefinitions)
        {
            // If null is passed in, do not edit workflow definitions. If an empty array, all definitions
            // should be deleted. If a non-empty array, then delete existing and add the new items.
            if (workflowDefinitions != null)
            {
                deleteWorkflowDefinitions(folderId);

                dynamic definition = new ExpandoObject();

                foreach (var workflowDefinition in workflowDefinitions)
                {
                    client.Cypher
                        .WithParams(new
                        {
                            folderId = folderId,
                            workflowDefinition = workflowDefinition
                        })
                        .Match("(folder:Folder { ID: {folderId} })")
                        .Create("folder<-[:APPLIES_TO]-(wd:WorkflowDefinition {workflowDefinition})")
                        .ExecuteWithoutResults();
                }
            }
        }

        private void deleteWorkflowDefinitions(string folderId)
        {
            client.Cypher
                .WithParams(new { folderId })
                .Match("(folder:Folder { ID: {folderId} })<-[r:APPLIES_TO]-(wd:WorkflowDefinition)")
                .Delete("r,wd")
                .ExecuteWithoutResults();
        }
    }
}
