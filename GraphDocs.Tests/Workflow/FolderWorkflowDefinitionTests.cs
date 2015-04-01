using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphDocs.Core.Models;

namespace GraphDocs.Tests.Workflow
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
        public void CreateFolderWithWorkflowDefinition()
        {
            Assert.IsTrue(folders.Get("/WorkflowFolder").WorkflowDefinitions.Count() == 1);
        }

        [TestMethod]
        public void CreateDocumentThatNeedsWorkflow()
        {
            documents.Create(new Document { Path = "/WF", Name = "doc.txt" });
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
            documentsWorkflows.SubmitWorkflowReply(doc, activeWorkflow.WorkflowName, activeWorkflow.Bookmark, true);
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
            documentsWorkflows.SubmitWorkflowReply(doc, activeWorkflow.WorkflowName, activeWorkflow.Bookmark, false);
            Assert.IsFalse(documents.GetByPath("/WF/doc2.txt").Active);
        }
    }
}
