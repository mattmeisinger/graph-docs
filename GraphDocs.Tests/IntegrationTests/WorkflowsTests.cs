using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphDocs.Core.Models;

namespace GraphDocs.Tests.Workflow.IntegrationTests
{
    [TestClass]
    public class FolderWorkflowDefinitionTests : TestBase
    {
        public FolderWorkflowDefinitionTests() : base()
        {
            folders.Create(new Folder
            {
                Path = "/",
                Name = "WF",
                WorkflowDefinitions = new WorkflowDefinition[] {
                    new WorkflowDefinition {
                        Order = 1,
                        WorkflowName = "ApproveDocument",
                        Settings = new Dictionary<string, string> {
                            { "EmailRecipients", "mmeisinger@gmail.com" },
                            { "ApproverGroupName", "Group1" }
                        }
                    },
                    new WorkflowDefinition {
                        Order = 2,
                        WorkflowName = "MyWF2", // This one is a compiled workflow
                        Settings = new Dictionary<string, string> {
                            { "EmailRecipients", "mmeisinger@gmail.com" },
                            { "ApproverGroupName", "Group1" }
                        }
                    }
                }
            });
            folders.Create(new Folder
            {
                Path = "/",
                Name = "AdHocWF",
                WorkflowDefinitions = new WorkflowDefinition[] {
                    new WorkflowDefinition {
                        Order = 1,
                        WorkflowName = "AdHocWorkflow",
                        Settings = new Dictionary<string, string> {
                            { "EmailRecipients", "mmeisinger@gmail.com" },
                            { "ApproverGroupName", "Group1" }
                        }
                    }
                }
            });
            folders.Create(new Folder
            {
                Path = "/",
                Name = "CompiledWF",
                WorkflowDefinitions = new WorkflowDefinition[] {
                    new WorkflowDefinition {
                        Order = 1,
                        WorkflowName = "MyWF2",
                        Settings = new Dictionary<string, string> {
                            { "EmailRecipients", "mmeisinger@gmail.com" },
                            { "ApproverGroupName", "Group1" }
                        }
                    }
                }
            });
            folders.Create(new Folder
            {
                Path = "/",
                Name = "WorkflowFolder",
                WorkflowDefinitions = new[] {
                    new WorkflowDefinition {
                        WorkflowName = "ApproveDocument",
                        Settings = new Dictionary<string, string> {
                            { "EmailRecipients", "mmeisinger@gmail.com" }
                        }
                    }
                }
            });
        }

        [TestMethod]
        public void CreateDocumentInFolderWithWorkflowDefinition()
        {
            folders.Create(new Folder
            {
                Path = "/",
                Name = "WF1",
                WorkflowDefinitions = new[] {
                    new WorkflowDefinition {
                        WorkflowName = "ApproveDocument",
                        Settings = new Dictionary<string, string> {
                            { "ApproverGroupName", "Approver" },
                            { "EmailRecipients", "mmeisinger@gmail.com" },
                            { "ReplyUrlTemplate", "http://localhost:9241/v1/workflowreply/{instanceId}/{bookmarkName}/{response}" }
                        }
                    }
                }
            });
            Assert.IsTrue(folders.Get("/WF1").WorkflowDefinitions.Count() == 1);

            documents.Create(new Document { Path = "/WF1", Name = "test.txt", Tags = new[] { "test1", "testtag2" } });
            var doc = documents.GetByPath("/WF1/test.txt");
            Assert.IsTrue(doc.Active == false);
        }

        [TestMethod]
        public void CreateFolderWithWorkflowDefinition()
        {
            Assert.IsTrue(folders.Get("/WorkflowFolder").WorkflowDefinitions.Count() == 1);
        }

        [TestMethod]
        public void CreateDocumentThatNeedsWorkflow_Compiled()
        {
            documents.Create(new Document { Path = "/CompiledWF", Name = "test.txt" });
            var doc = documents.GetByPath("/CompiledWF/test.txt");
            Assert.IsFalse(doc.Active, "This doc should be stuck in a workflow, but instead it skipped the workflow.");
        }

        [TestMethod]
        public void CreateDocumentThatNeedsWorkflow_AdHoc()
        {
            documents.Create(new Document { Path = "/AdHocWF", Name = "test.txt" });
            var doc = documents.GetByPath("/AdHocWF/test.txt");
            Assert.IsFalse(doc.Active, "This doc should be stuck in a workflow, but instead it skipped the workflow.");
        }

        [TestMethod]
        public void CreateDocumentThatNeedsWorkflow_NotActiveUntilApproved()
        {
            // Create document that is not approved until feedback is given
            documents.Create(new Document { Path = "/WF", Name = "doc2.txt" });
            var doc = documents.GetByPath("/WF/doc2.txt");
            var activeWorkflow = doc.ActiveWorkflows.First();
            Assert.IsFalse(documents.GetByPath("/WF/doc2.txt").Active);

            // Simulate feedback coming in
            documentsWorkflows.SubmitWorkflowReply(activeWorkflow.InstanceId, activeWorkflow.Bookmark, true);
            Assert.IsFalse(documents.GetByPath("/WF/doc2.txt").Active);

            doc = documents.GetByPath("/WF/doc2.txt");
            activeWorkflow = doc.ActiveWorkflows.Last();
            documentsWorkflows.SubmitWorkflowReply(activeWorkflow.InstanceId, activeWorkflow.Bookmark, true);
            Assert.IsTrue(documents.GetByPath("/WF/doc2.txt").Active);
        }

        [TestMethod]
        public void CreateDocumentThatNeedsWorkflow_NotActiveBecauseRejected()
        {
            // Create document that is not approved until feedback is given
            documents.Create(new Document { Path = "/WF", Name = "doc2.txt" });
            var doc = documents.GetByPath("/WF/doc2.txt");
            var activeWorkflow = doc.ActiveWorkflows.First();
            Assert.IsFalse(documents.GetByPath("/WF/doc2.txt").Active);

            // Simulate feedback coming in
            documentsWorkflows.SubmitWorkflowReply(activeWorkflow.InstanceId, activeWorkflow.Bookmark, false);
            Assert.IsFalse(documents.GetByPath("/WF/doc2.txt").Active);
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
            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "EmailRecipients", "mmeisinger@gmail.com" },
                { "Document", new Document { Active = true, Name = "MyFile", Tags = new[] { "Item", "Tag2" } } },
                { "DocumentFile", new DocumentFile { } }
            };
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
            var status = workflows.InitializeWorkflow(workflowName, parameters);
            Assert.IsNotNull(status.InstanceId);

            var result = workflows.ResumeWorkflow(workflowName, status.InstanceId, "Approval-Group1", true);
            Assert.IsTrue(result.Result is bool);
            Assert.IsTrue((bool)result.Result == true);
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
            var status = workflows.InitializeWorkflow(workflowName, parameters);
            Assert.IsNotNull(status.InstanceId);

            var result = workflows.ResumeWorkflow(workflowName, status.InstanceId, "Approval-Group1", false);
            Assert.IsTrue(result.Result is bool);
            Assert.IsTrue((bool)result.Result == false);
        }
    }
}
