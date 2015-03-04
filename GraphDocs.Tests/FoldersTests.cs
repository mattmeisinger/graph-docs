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
            ds.Create(new Models.Folder { Path = "/Test1", Name = "Test2a" });
            ds.Create(new Models.Folder { Path = "/Test1", Name = "Test2b" });
            ds.Create(new Models.Folder { Path = "/Test1/Test2a", Name = "Test3a" });
            ds.Create(new Models.Folder { Path = "/Test1/Test2a/Test3a", Name = "Test4a" });
        }

        [TestMethod]
        public void GetFolderID_Root()
        {
            var id = ds.GetIDFromFolderPath("/");
            Assert.IsNotNull(id);
        }

        [TestMethod]
        public void GetFolderID_Level1Folder()
        {
            var rootNodeId = ds.GetIDFromFolderPath("/");
            var folderNodeId = ds.GetIDFromFolderPath("/TestFolder");
            Assert.IsNotNull(rootNodeId);
            Assert.IsNotNull(folderNodeId);
            Assert.IsTrue(rootNodeId != folderNodeId);
        }

        [TestMethod]
        public void GetFolderID_Level2Folder()
        {
            var rootNodeId = ds.GetIDFromFolderPath("/");
            var folderNodeId = ds.GetIDFromFolderPath("/Test1/Test2a");
            Assert.IsNotNull(rootNodeId);
            Assert.IsNotNull(folderNodeId);
            Assert.IsTrue(rootNodeId != folderNodeId);
        }

        [TestMethod]
        public void GetFolderID_FolderDoesNotExist()
        {
            var folderNodeId = ds.GetIDFromFolderPath("/FolderThatDoesNotExist");
            Assert.IsNull(folderNodeId);
            Assert.IsTrue(folderNodeId == null);
        }

        [TestMethod]
        public void GetFolder_Root()
        {
            var folder = ds.Get("/");
            Assert.IsNotNull(folder);
            Assert.IsTrue(folder.ChildFolders.Length == 2);
        }

        [TestMethod]
        public void GetFolder_FolderExists()
        {
            var folder = ds.Get("/Test1");
            Assert.IsNotNull(folder);
            Assert.IsTrue(folder.ChildFolders.Length == 2);
        }

        [TestMethod]
        public void GetFolder_Level2FolderExists()
        {
            var folder = ds.Get("/Test1/Test2a");
            Assert.IsNotNull(folder);
            Assert.IsTrue(folder.ChildFolders.Length == 1);
        }

        [TestMethod]
        public void DeleteFolder_FolderExists()
        {
            // Create a folder and make sure it exists
            ds.Create(new Models.Folder { Path = "/", Name = "FolderToDelete" });
            var id = ds.GetIDFromFolderPath("/FolderToDelete");
            Assert.IsNotNull(id);

            // Now delete the folder and make sure it does not exist any more
            ds.Delete("/FolderToDelete");
            id = ds.GetIDFromFolderPath("/FolderToDelete");
            Assert.IsNull(id);
        }

        [TestMethod]
        public void DeleteFolder_WithChildren()
        {
            ds.Create(new Models.Folder { Path = "/", Name = "FolderToDelete" });
            ds.Create(new Models.Folder { Path = "/FolderToDelete", Name = "SubFolder" });
            ds.Create(new Models.Folder { Path = "/FolderToDelete/SubFolder", Name = "SubSubFolder" });
            var id = ds.GetIDFromFolderPath("/FolderToDelete");
            Assert.IsNotNull(id);
            id = ds.GetIDFromFolderPath("/FolderToDelete/SubFolder");
            Assert.IsNotNull(id);

            ds.Delete("/FolderToDelete");
            id = ds.GetIDFromFolderPath("/FolderToDelete");
            Assert.IsNull(id);
            id = ds.GetIDFromFolderPath("/FolderToDelete/SubFolder");
            Assert.IsNull(id);
        }

        [TestMethod]
        public void CreateFolder_NewFolder()
        {
            ds.Create(new Models.Folder { Path = "/", Name = "NewFolder-D" });
            var id = ds.GetIDFromFolderPath("/NewFolder-D");
            Assert.IsNotNull(id);
        }

        [TestMethod]
        public void CreateFolder_NewSubFolder()
        {
            ds.Create(new Models.Folder { Path = "/", Name = "NewFolder-E" });
            ds.Create(new Models.Folder { Path = "/NewFolder-E", Name = "NewSubFolder-E" });
            var id = ds.GetIDFromFolderPath("/NewFolder-E/NewSubFolder-E");
            Assert.IsNotNull(id);
        }

        [TestMethod]
        public void CreateFolder_NewSubSubFolder()
        {
            ds.Create(new Models.Folder { Path = "/", Name = "NewFolder-F" });
            ds.Create(new Models.Folder { Path = "/NewFolder-F", Name = "NewSubFolder-F" });
            ds.Create(new Models.Folder { Path = "/NewFolder-F/NewSubFolder-F", Name = "NewSubSubFolder-F" });
            var id = ds.GetIDFromFolderPath("/NewFolder-F/NewSubFolder-F/NewSubSubFolder-F");
            Assert.IsNotNull(id);
        }
    }
}
