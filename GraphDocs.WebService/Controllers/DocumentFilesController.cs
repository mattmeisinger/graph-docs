using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GraphDocs.Core.Interfaces;

namespace GraphDocs.WebService.Controllers
{
    public class DocumentFilesController : BaseController
    {
        private IDocumentFilesDataService documentFiles;
        private IDocumentsDataService documents;
        public DocumentFilesController(IDocumentFilesDataService documentFiles, IDocumentsDataService documents)
        {
            this.documentFiles = documentFiles;
            this.documents = documents;
        }

        [ActionName("Index"), HttpGet]
        public ActionResult Get(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == "/")
                throw new Exception("A document path must be provided to retrieve a file.");

            var file = documentFiles.Get(path);
            if (file == null)
                throw new Exception("File not found: " + path);

            var document = documents.GetByPath(path);
            if (document == null)
                throw new Exception("Document not found." + path);

            return File(file.Data, file.MimeType, document.Name);
        }

        [ActionName("Index"), HttpPost]
        public ActionResult Post(string path, byte[] data, string mimeType, string documentId)
        {
            documentFiles.Create(new Core.Models.DocumentFile { DocumentPath = path, Data = data, MimeType = mimeType, DocumentId = documentId });
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [ActionName("Index"), HttpPut]
        public ActionResult Put(string path, byte[] data, string mimeType, string documentId)
        {
            documentFiles.Save(new Core.Models.DocumentFile { DocumentPath = path, Data = data, MimeType = mimeType, DocumentId = documentId });
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [ActionName("Index"), HttpDelete]
        public ActionResult Delete(string path)
        {
            documentFiles.Delete(path);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}