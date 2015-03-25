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
        public void InitializeWorkflow()
        {
            IDictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Document", new Document { Active = true, Name = "MyFile", Tags = new[] { "Item", "Tag2" } });
            parameters.Add("DocumentFile", new DocumentFile { });
            parameters.Add("EmailRecipients", "mmeisinger@gmail.com");
            var workflowInstanceId = workflows.InitializeWorkflow("SimpleEmailNotification", parameters);
            Assert.IsNotNull(workflowInstanceId);
        }
    }
}
