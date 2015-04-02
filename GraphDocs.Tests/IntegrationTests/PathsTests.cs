using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace GraphDocs.Tests.IntegrationTests
{
    [TestClass]
    public class PathsTests : TestBase
    {
        public PathsTests()
            : base()
        {
            folders.Create(new Core.Models.Folder { Path = "/", Name = "TestFolder" });
            folders.Create(new Core.Models.Folder { Path = "/", Name = "Test1" });
            folders.Create(new Core.Models.Folder { Path = "/Test1", Name = "Test2a" });
            folders.Create(new Core.Models.Folder { Path = "/Test1", Name = "Test2b" });
            folders.Create(new Core.Models.Folder { Path = "/Test1/Test2a", Name = "Test3a" });
            folders.Create(new Core.Models.Folder { Path = "/Test1/Test2a/Test3a", Name = "Test4a" });
            documents.Create(new Core.Models.Document { Path = "/", Name = "doc1.txt", Tags = new[] { "Tag1" } });
            documents.Create(new Core.Models.Document { Path = "/Test1", Name = "doc2.txt", Tags = new[] { "Tag1", "Tag2" } });
            documents.Create(new Core.Models.Document { Path = "/Test1/Test2a/Test3a", Name = "doc3.txt", Tags = new[] { "Tag1", "Tag2", "Tag3" } });
            documentFiles.Create(new Core.Models.DocumentFile { DocumentPath = "/Test1/Test2a/Test3a/doc3.txt", Data = Encoding.UTF8.GetBytes("Test text in a file."), MimeType = "text/plain" });
        }

        [TestMethod]
        public void GetFolderID_Root()
        {
            var id = paths.GetIDFromFolderPath("/");
            Assert.IsNotNull(id);
        }

        [TestMethod]
        public void GetFolderID_Level1Folder()
        {
            var rootNodeId = paths.GetIDFromFolderPath("/");
            var folderNodeId = paths.GetIDFromFolderPath("/TestFolder");
            Assert.IsNotNull(rootNodeId);
            Assert.IsNotNull(folderNodeId);
            Assert.IsTrue(rootNodeId != folderNodeId);
        }

        [TestMethod]
        public void GetFolderID_Level2Folder()
        {
            var rootNodeId = paths.GetIDFromFolderPath("/");
            var folderNodeId = paths.GetIDFromFolderPath("/Test1/Test2a");
            Assert.IsNotNull(rootNodeId);
            Assert.IsNotNull(folderNodeId);
            Assert.IsTrue(rootNodeId != folderNodeId);
        }

        [TestMethod]
        public void GetFolderID_FolderDoesNotExist()
        {
            var folderNodeId = paths.GetIDFromFolderPath("/FolderThatDoesNotExist");
            Assert.IsNull(folderNodeId);
            Assert.IsTrue(folderNodeId == null);
        }

        [TestMethod]
        public void GetDocumentID_DocumentsExistAtLevels()
        {
            var id1 = paths.GetIDFromDocumentPath("/doc1.txt");
            var id2 = paths.GetIDFromDocumentPath("/Test1/doc2.txt");
            var doc = documents.GetByPath("/Test1/doc2.txt");
            doc.Tags = doc.Tags.Union(new[] { "Tag4", "Tag5" }).Where(a => a != "Tag3").ToArray();
            documents.Save(doc);
            Assert.IsNotNull(id1);
            Assert.IsNotNull(id2);
            Assert.IsFalse(id1 == id2);
        }
    }
}
