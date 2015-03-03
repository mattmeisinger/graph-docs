using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphDocs.DataServices;

namespace GraphDocs.Tests
{
    [TestClass]
    public class FoldersTests
    {
        public FoldersTests()
        {
            var ds = new FoldersDataService();
            ds.DeleteAll();
            DatabaseService.Init();
            ds.Create(new Models.Folder { Path = "/", Name = "TestFolder" });
            ds.Create(new Models.Folder { Path = "/", Name = "Test1" });
        }

        [TestMethod]
        public void GetFolder_Root()
        {
            var ds = new FoldersDataService();
            var id = ds.GetIDFromFolderPath("/");
            Assert.IsNotNull(id);
        }

        [TestMethod]
        public void GetFolder_Level1Folder()
        {
            var ds = new FoldersDataService();
            var rootNodeId = ds.GetIDFromFolderPath("/");
            var folderNodeId = ds.GetIDFromFolderPath("/TestFolder");
            Assert.IsNotNull(rootNodeId);
            Assert.IsNotNull(folderNodeId);
            Assert.IsTrue(rootNodeId != folderNodeId);
        }

        [TestMethod]
        public void GetFolder_FolderDoesNotExist()
        {
            var ds = new FoldersDataService();
            var folderNodeId = ds.GetIDFromFolderPath("/FolderThatDoesNotExist");
            Assert.IsNull(folderNodeId);
            Assert.IsTrue(folderNodeId == null);
        }
    }
}
