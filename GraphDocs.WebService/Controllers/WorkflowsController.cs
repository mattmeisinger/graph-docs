using GraphDocs.Core.Interfaces;
using GraphDocs.Infrastructure.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GraphDocs.WebService.Controllers
{
    public class WorkflowsController : BaseController
    {
        WorkflowService workflow;
        public WorkflowsController(WorkflowService workflow)
        {
            this.workflow = workflow;
        }

        [ActionName("Index"), HttpGet]
        public ActionResult Get()
        {
            // Get list of all available workflows
            return Json(workflow.GetAvailableWorkflows());
        }
    }
}