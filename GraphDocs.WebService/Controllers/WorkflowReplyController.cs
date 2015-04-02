using GraphDocs.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GraphDocs.WebService.Controllers
{
    public class WorkflowReplyController : BaseController
    {
        IDocumentsWorkflowsService workflow;
        IDocumentsDataService documents;
        public WorkflowReplyController(IDocumentsWorkflowsService workflow, IDocumentsDataService documents)
        {
            this.workflow = workflow;
            this.documents = documents;
        }

        [ActionName("Index"), HttpGet]
        public ActionResult Get(string documentId, string workflowName, string bookmarkName, bool response)
        {
            // Get list of all available workflows
            var doc = documents.GetByID(documentId);
            workflow.SubmitWorkflowReply(doc, workflowName, bookmarkName, response);
            return Json(new { Message = "Reply has been submitted." });
        }
    }
}