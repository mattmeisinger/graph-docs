using GraphDocs.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GraphDocs.WebService.Controllers
{
    public class WorkflowsController : BaseController
    {
        IWorkflowService workflow;
        public WorkflowsController(IWorkflowService workflow)
        {
            this.workflow = workflow;
        }

        [ActionName("Index"), HttpGet]
        public ActionResult Get()
        {
            // Get list of all available workflows
            return Json(workflow.GetAvailableWorkflows());
        }

        [ActionName("Index"), HttpPost]
        public ActionResult Post(string name)
        {
            if (workflow.GetAvailableWorkflows().Any(a => a.Name == name))
                throw new Exception("A workflow with the name '" + name + "' already exists.");

            if (Request.Files.Count == 0)
                throw new Exception("A workflow definition file must be uploaded.");

            workflow.Create(name, Request.Files[0].InputStream);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [ActionName("Index"), HttpDelete]
        public ActionResult Delete(string name)
        {
            workflow.Delete(name);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}