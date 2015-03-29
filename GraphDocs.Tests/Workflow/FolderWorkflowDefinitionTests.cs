using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphDocs.Core.Models;
using GraphDocs.Infrastructure;
using GraphDocs.Infrastructure.Database;

namespace GraphDocs.Tests.Workflow
{
    [TestClass]
    public class FolderWorkflowDefinitionTests
    {
        FoldersDataService folders = new FoldersDataService();
        PathsDataService paths = new PathsDataService(Neo4jConnectionFactory.GetConnection());
        DocumentsDataService documents = new DocumentsDataService();

        public FolderWorkflowDefinitionTests()
        {
            Neo4jConnectionFactory.InitAndEraseAll();
        }

        [TestMethod]
        public void CreateFolderWithWorkflowDefinition()
        {
            folders.Create(new Core.Models.Folder {
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
            Assert.IsTrue(folders.Get("/WorkflowFolder").WorkflowDefinitions.Count() == 1);
        }
    }
}
