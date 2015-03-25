using System;
using System.Linq;
using Neo4jClient;
using GraphDocs.Core.Models;
using GraphDocs.Infrastructure.Utilities;
using GraphDocs.Infrastructure.Database;

namespace GraphDocs.Infrastructure
{
    public class DocumentFilesDataService
    {
        private IGraphClient client;
        private PathsDataService paths;

        public DocumentFilesDataService()
        {
            client = Neo4jConnectionFactory.GetConnection();
            paths = new PathsDataService(client);
        }

        public DocumentFile Get(string path)
        {
            path = PathUtilities.ReformatPath(path);
            var documentId = paths.GetIDFromDocumentPath(path);
            var file = client.Cypher
                .WithParams(new { documentId })
                .Match("(d:Document { ID: {documentId} })<-[:FILE_FOR]-(file:DocumentFile)")
                .Return<DocumentFile>("file")
                .Results
                .SingleOrDefault();

            return file;
        }

        public void Delete(string path)
        {
            path = PathUtilities.ReformatPath(path);
            var id = paths.GetIDFromDocumentPath(path);

            client.Cypher
                .WithParams(new { id = id })
                .Match("(:Document { ID: {id} })<-[rel:FILE_FOR]-(file:DocumentFile)")
                .Delete("rel, file")
                .ExecuteWithoutResults();
        }

        public void Create(DocumentFile d)
        {
            d.DocumentId = d.DocumentId ?? paths.GetIDFromDocumentPath(d.DocumentPath);

            var fileExists = client.Cypher
                .WithParams(new
                {
                    documentId = d.DocumentId
                })
                .Match("(parent:Document { ID: {documentId} })<-[:FILE_FOR]-(f:DocumentFile)")
                .Return<DocumentFile>("f")
                .Results
                .Any();
            if (fileExists)
                throw new Exception("File already exists for this document.");

            // Attach to root folder
            client.Cypher
                .WithParams(new
                {
                    documentId = d.DocumentId,
                    file = new
                    {
                        d.Data,
                        d.MimeType
                    }
                })
                .Match("(parent:Document { ID: {documentId} })")
                .Create("parent<-[:FILE_FOR]-(:DocumentFile {file})")
                .ExecuteWithoutResults();
        }

        public void Save(DocumentFile d)
        {
            var documentId = paths.GetIDFromFolderPath(d.DocumentPath);

            client.Cypher
                .WithParams(new
                {
                    documentId,
                    data = d.Data,
                    mimeType = d.MimeType
                })
                .Match("(:Document { ID: {documentId} })<-[:FILE_FOR]-(file:DocumentFile)")
                .Set("file.Data = {data}, file.MimeType = {mimeType}")
                .ExecuteWithoutResults();
        }
    }
}
