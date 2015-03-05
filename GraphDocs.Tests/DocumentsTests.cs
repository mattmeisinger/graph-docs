using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphDocs.DataServices;

namespace GraphDocs.Tests
{
    [TestClass]
    public class DocumentsTests
    {
        FoldersDataService folders = new FoldersDataService();
        DocumentsDataService documents = new DocumentsDataService();
        PathsDataService paths = new PathsDataService();

        public DocumentsTests()
        {
            DatabaseService.InitAndEraseAll();
            folders.Create(new Models.Folder { Path = "/", Name = "TestFolder" });
            folders.Create(new Models.Folder { Path = "/", Name = "Test1" });
            folders.Create(new Models.Folder { Path = "/Test1", Name = "Test2a" });
            folders.Create(new Models.Folder { Path = "/Test1", Name = "Test2b" });
            folders.Create(new Models.Folder { Path = "/Test1/Test2a", Name = "Test3a" });
            folders.Create(new Models.Folder { Path = "/Test1/Test2a/Test3a", Name = "Test4a" });
            documents.Create(new Models.Document { Path = "/", Name = "doc1.txt" });
            documents.Create(new Models.Document { Path = "/Test1", Name = "doc2.txt" });
        }

        [TestMethod]
        public void SaveDocument_WithTags()
        {
            documents.Create(new Models.Document { Path = "/Test1", Name = "doc3.txt", Tags = new[] { "TestTag1", "TestTag2" } });
        }
    }
}
