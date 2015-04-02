using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GraphDocs.Tests.IntegrationTests
{
    [TestClass]
    public class DocumentsTests : TestBase
    {
        public DocumentsTests()
            : base()
        {
            folders.Create(new Core.Models.Folder { Path = "/", Name = "TestFolder" });
            folders.Create(new Core.Models.Folder { Path = "/", Name = "Test1" });
            folders.Create(new Core.Models.Folder { Path = "/Test1", Name = "Test2a" });
            folders.Create(new Core.Models.Folder { Path = "/Test1", Name = "Test2b" });
            folders.Create(new Core.Models.Folder { Path = "/Test1/Test2a", Name = "Test3a" });
            folders.Create(new Core.Models.Folder { Path = "/Test1/Test2a/Test3a", Name = "Test4a" });
            documents.Create(new Core.Models.Document { Path = "/", Name = "doc1.txt" });
            documents.Create(new Core.Models.Document { Path = "/Test1", Name = "doc2.txt" });
        }

        [TestMethod]
        public void SaveDocument_WithTags()
        {
            documents.Create(new Core.Models.Document { Path = "/Test1", Name = "doc3.txt", Tags = new[] { "TestTag1", "TestTag2" } });
        }
    }
}
