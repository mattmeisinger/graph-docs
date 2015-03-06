using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neo4jClient;
using GraphDocs.Models;

namespace GraphDocs.DataServices
{
    public class DocumentFilesDataService
    {
        private GraphClient client;
        private PathsDataService paths;

        public DocumentFilesDataService()
        {
            client = DatabaseService.GetConnection();
            paths = new PathsDataService(client);
        }

        public DocumentFile Get(string path)
        {
            var documentId = paths.GetIDFromDocumentPath(path);
            var file = client.Cypher
                .WithParams(new { documentId })
                .Match("(d:Document { ID: {documentId} })<-[:FILE_OF]-(file:DocumentFile)")
                .Return<DocumentFile>("file")
                .Results
                .SingleOrDefault();

            return file;
        }

        public void Delete(string path)
        {
            var id = paths.GetIDFromDocumentPath(path);

            client.Cypher
                .WithParams(new { id = id })
                .Match("(:Document { ID: {id} })<-[rel:FILE_OF]-(file:DocumentFile)")
                .Delete("rel, file")
                .ExecuteWithoutResults();
        }

        public void Create(DocumentFile d)
        {
            var documentId = paths.GetIDFromFolderPath(d.FilePath);

            // Attach to root folder
            client.Cypher
                .WithParams(new
                {
                    documentId,
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
            var documentId = paths.GetIDFromFolderPath(d.FilePath);

            client.Cypher
                .WithParams(new
                {
                    documentId,
                    documentFile = new
                    {
                        d.Data,
                        d.MimeType
                    }
                })
                .Match("(:Document { ID: {documentId} })<-[:FILE_FOR]-(file:DocumentFile)")
                .Set("file = {documentFile}")
                .ExecuteWithoutResults();
        }
    }
}
