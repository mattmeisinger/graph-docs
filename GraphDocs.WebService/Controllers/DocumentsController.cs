using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GraphDocs.Infrastructure;

namespace GraphDocs.WebService.Controllers
{
    public class DocumentsController : BaseController
    {
        private DocumentsDataService documents;
        public DocumentsController(DocumentsDataService documents)
        {
            this.documents = documents;
        }

        [ActionName("Index"), HttpGet]
        public ActionResult Get(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == "/")
                throw new Exception("A document path must be provided to retrieve documents.");

            var document = documents.Get(path);
            return Json(new
            {
                data = new
                {
                    id = document.ID,
                    name = document.Name,
                    path = document.Path,
                    tags = document.Tags,
                    hasFile = document.HasFile
                },
                tags = document.Tags.Select(a => new
                {
                    name = a,
                    links = new
                    {
                        self = PathToController("tags", a)
                    }
                }),
                activeWorkflows = document.ActiveWorkflows.Select(a => new
                {

                }),
                links = new
                {
                    self = PathTo(document.Path),
                    fileDownload = document.HasFile ? PathToController("documentfiles", document.Path) : (string)null
                }
            });
        }

        [ActionName("Index"), HttpPost]
        public ActionResult Post(string path, string name, string[] tags)
        {
            documents.Create(new Core.Models.Document { Path = path, Name = name, Tags = tags });
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [ActionName("Index"), HttpPut]
        public ActionResult Put(string path, string name, string[] tags, string id)
        {
            documents.Save(new Core.Models.Document { Path = path, Name = name, Tags = tags, ID = id });
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [ActionName("Index"), HttpDelete]
        public ActionResult Delete(string path)
        {
            documents.Delete(path);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}