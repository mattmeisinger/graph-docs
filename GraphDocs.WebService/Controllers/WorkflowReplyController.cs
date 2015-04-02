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
        public ActionResult Get(string workflowInstanceId, string bookmarkName, bool response)
        {
            try
            {
                workflow.SubmitWorkflowReply(workflowInstanceId, bookmarkName, response);
                return Json(new
                {
                    Message = "Reply has been submitted.",
                    Error = false
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Message = "Error: " + ex.Message,
                    Error = true
                });
            }
        }
    }
}