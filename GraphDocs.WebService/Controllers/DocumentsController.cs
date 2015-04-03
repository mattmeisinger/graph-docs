using GraphDocs.Core.Interfaces;
using GraphDocs.WebService.Models;
using GraphDocs.WebService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GraphDocs.WebService.Controllers
{
    public class DocumentsController : BaseController
    {
        private IDocumentsDataService documents;
        public DocumentsController(IDocumentsDataService documents)
        {
            this.documents = documents;
        }

        [ActionName("Index"), HttpGet]
        public ActionResult Get(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == "/")
                throw new Exception("A document path must be provided to retrieve documents.");

            var document = documents.GetByPath(path);
            return Json(new
            {
                data = new
                {
                    id = document.ID,
                    name = document.Name,
                    active = document.Active,
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
                activeWorkflows = document.ActiveWorkflows.OrderBy(a => a.Order).Select(a => new
                {
                    name = a.WorkflowName,
                    status = a.Status,
                    order = a.Order,
                    instanceId = a.InstanceId,
                    bookmark = a.Bookmark,
                    settings = a.Settings.Select(b => new
                    {
                        key = b.Key,
                        value = b.Value
                    }),
                    links = a.Status == "InProgress"
                        ? new Dictionary<string, object>
                        {
                            // Only show these items when the status indicates it is expecting a response
                            { "respondApprove", PathToController("workflowreply", a.InstanceId, a.Bookmark, "true") },
                            { "respondReject", PathToController("workflowreply", a.InstanceId, a.Bookmark, "false") }
                        }
                        : new Dictionary<string, object>()
                }),
                links = new
                {
                    self = PathTo(document.Path),
                    fileDownload = document.HasFile ? PathToController("documentfiles", document.Path) : (string)null
                }
            });
        }

        [ActionName("Index"), HttpPost]
        [ReadJsonBody(Param = "model", JsonDataType = typeof(DocumentItem))]
        public ActionResult Post(string path, DocumentItem model)
        {
            var doc = model.ToDocument();
            doc.Path = path;
            documents.Create(doc);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [ActionName("Index"), HttpPut]
        [ReadJsonBody(Param = "model", JsonDataType = typeof(DocumentItem))]
        public ActionResult Put(string path, string id, DocumentItem model)
        {
            var doc = model.ToDocument();
            doc.Path = path;
            doc.ID = id;
            documents.Save(doc);
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