using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphDocs.DataServices;

namespace GraphDocs.Tests
{
    [TestClass]
    public class FoldersTests
    {
        [TestMethod]
        public void GetFolder_Root()
        {
            var ds = new FoldersDataService();
            var id = ds.GetNodeIdFromFolderPath("/");
            Assert.IsTrue(id.HasValue);
        }

        [TestMethod]
        public void GetFolder_Level1Folder()
        {
            var ds = new FoldersDataService();
            var rootNodeId = ds.GetNodeIdFromFolderPath("/");
            var folderNodeId = ds.GetNodeIdFromFolderPath("/Test1");
            Assert.IsNotNull(rootNodeId);
            Assert.IsNotNull(folderNodeId);
            Assert.IsTrue(rootNodeId != folderNodeId);
        }

        [TestMethod]
        public void GetFolder_FolderDoesNotExist()
        {
            var ds = new FoldersDataService();
            var folderNodeId = ds.GetNodeIdFromFolderPath("/FolderThatDoesNotExist");
            Assert.IsTrue(folderNodeId == null);
        }

        //[TestMethod]
        //public void Test()
        //{
        //    var ds = new FoldersDataService();
        //    ds.Create("Test1", "/");
        //    ds.Create("Test2", "/Test1");
        //    var folders = ds.Get("/Test1");
        //    //var folders2 = ds.Get("/AnotherFolder/AnotherChildFolder");
        //}
    }
}
