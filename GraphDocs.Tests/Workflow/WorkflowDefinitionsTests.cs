using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphDocs.Infrastructure.Workflow;
using GraphDocs.Core;
using System.Collections.Generic;
using GraphDocs.Core.Models;

namespace GraphDocs.Tests.Workflow
{
    [TestClass]
    public class WorkflowDefinitionsTests
    {
        WorkflowService workflows;
        public WorkflowDefinitionsTests()
        {
            // Set up class
            workflows = new WorkflowService(new SettingsService
            {
                APIVersionNumber = "v1",
                SiteBaseUrl = "http://localhost",
                SmtpServer = "localhost",
                WorkflowFolder = "WorkflowDefinitions",
                WorkflowStoreId = new Guid("00000000-117e-4bac-b93a-613d7baaa000")
            });
        }

        [TestMethod]
        public void GetListOfWorkflows()
        {
            var items = workflows.GetAvailableWorkflows();
            Assert.IsTrue(items.Any());
        }

        [TestMethod]
        public void SimpleEmailNotification()
        {
            var workflowName = "SimpleEmailNotification";
            IDictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("EmailRecipients", "mmeisinger@gmail.com");
            parameters.Add("Subject", "Test email");
            parameters.Add("Body", "Test email body text.");
            var workflowInstanceId = workflows.InitializeWorkflow(workflowName, parameters);
            Assert.IsNotNull(workflowInstanceId);
        }

        [TestMethod]
        public void DocumentCreatedEmailNotification()
        {
            var workflowName = "DocumentCreatedEmailNotification";
            IDictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("EmailRecipients", "mmeisinger@gmail.com");
            parameters.Add("Document", new Document { Active = true, Name = "MyFile", Tags = new[] { "Item", "Tag2" } });
            parameters.Add("DocumentFile", new DocumentFile { });
            var workflowInstanceId = workflows.InitializeWorkflow(workflowName, parameters);
            Assert.IsNotNull(workflowInstanceId);
        }

        [TestMethod]
        public void InitializeThenResumeWorkflow()
        {
            var workflowName = "ApproveDocument";
            IDictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Document", new Document { Active = true, Name = "MyFile", Tags = new[] { "Item", "Tag2" } });
            parameters.Add("DocumentFile", new DocumentFile { });
            parameters.Add("EmailRecipients", "mmeisinger@gmail.com");
            parameters.Add("ApproverGroupName", "Group1");
            var workflowInstanceId = workflows.InitializeWorkflow(workflowName, parameters);
            Assert.IsNotNull(workflowInstanceId);

            var result = workflows.ResumeWorkflow("ApproveDocument", workflowInstanceId, "Approval-Group1", true);
            Assert.IsTrue(result is bool);
            Assert.IsTrue((bool)result == true);
        }

        [TestMethod]
        public void InitializeThenResumeWorkflow_NotApproved()
        {
            var workflowName = "ApproveDocument";
            IDictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Document", new Document { Active = true, Name = "MyFile", Tags = new[] { "Item", "Tag2" } });
            parameters.Add("DocumentFile", new DocumentFile { });
            parameters.Add("EmailRecipients", "mmeisinger@gmail.com");
            parameters.Add("ApproverGroupName", "Group1");
            var workflowInstanceId = workflows.InitializeWorkflow(workflowName, parameters);
            Assert.IsNotNull(workflowInstanceId);

            var result = workflows.ResumeWorkflow("ApproveDocument", workflowInstanceId, "Approval-Group1", false);
            Assert.IsTrue(result is bool);
            Assert.IsTrue((bool)result == false);
        }
    }
}
