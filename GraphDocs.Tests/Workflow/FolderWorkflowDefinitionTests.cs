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
        public void CreateDocumentThatNeedsWorkflow2()
        {
            documents.Create(new Document { Path = "/WF", Name = "doc2.txt" });
            //documentsWorkflows.ResumeWorkflowsForDocument();
        }
    }
}
