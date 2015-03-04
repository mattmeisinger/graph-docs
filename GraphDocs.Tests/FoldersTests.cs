using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphDocs.DataServices;

namespace GraphDocs.Tests
{
    [TestClass]
    public class FoldersTests
    {
        FoldersDataService ds = new FoldersDataService();

        public FoldersTests()
        {
            ds.DeleteAll();
            DatabaseService.Init();
            ds.Create(new Models.Folder { Path = "/", Name = "TestFolder" });
            ds.Create(new Models.Folder { Path = "/", Name = "Test1" });
        }

        [TestMethod]
        public void GetFolder_Root()
        {
            var id = ds.GetIDFromFolderPath("/");
            Assert.IsNotNull(id);
        }

        [TestMethod]
        public void GetFolder_Level1Folder()
        {
            var rootNodeId = ds.GetIDFromFolderPath("/");
            var folderNodeId = ds.GetIDFromFolderPath("/TestFolder");
            Assert.IsNotNull(rootNodeId);
            Assert.IsNotNull(folderNodeId);
            Assert.IsTrue(rootNodeId != folderNodeId);
        }

        [TestMethod]
        public void GetFolder_FolderDoesNotExist()
        {
            var folderNodeId = ds.GetIDFromFolderPath("/FolderThatDoesNotExist");
            Assert.IsNull(folderNodeId);
            Assert.IsTrue(folderNodeId == null);
        }

        [TestMethod]
        public void DeleteFolder_FolderExists()
        {
            ds.Create(new Models.Folder { Path = "/", Name = "FolderToDelete" });
            var id = ds.GetIDFromFolderPath("/FolderToDelete");
            Assert.IsNotNull(id);
            ds.Delete("/FolderToDelete");
            id = ds.GetIDFromFolderPath("/FolderToDelete");
            Assert.IsNull(id);
        }
    }
}
