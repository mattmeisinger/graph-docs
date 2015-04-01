using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphDocs.Core.Models;
using GraphDocs.Infrastructure;
using GraphDocs.Infrastructure.Database;
using GraphDocs.Core.Interfaces;

namespace GraphDocs.Tests.Workflow
{
    [TestClass]
    public class DocumentWorkflowDefinitionTests : TestBase
    {
        public DocumentWorkflowDefinitionTests()
            : base()
        {

        }

        [TestMethod]
        public void CreateDocumentInFolderWithWorkflowDefinition()
        {
            folders.Create(new Core.Models.Folder
            {
                Path = "/",
                Name = "WorkflowFolder",
                WorkflowDefinitions = new[] { 
                    new WorkflowDefinition { 
                        WorkflowName = "ApproveDocument",
                        Settings = new Dictionary<string, string> {
                            { "ApproverGroupName", "Approver" },
                            { "EmailRecipients", "mmeisinger@gmail.com" } 
                        }
                    }
                }
            });
            Assert.IsTrue(folders.Get("/WorkflowFolder").WorkflowDefinitions.Count() == 1);

            documents.Create(new Core.Models.Document { Path = "/WorkflowFolder", Name = "test.txt", Tags = new[] { "test1", "testtag2" } });
            var doc = documents.GetByPath("/WorkflowFolder/test.txt");
            Assert.IsTrue(doc.Active == false);
        }
    }
}
