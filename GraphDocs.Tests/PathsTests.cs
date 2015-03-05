﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphDocs.DataServices;

namespace GraphDocs.Tests
{
    [TestClass]
    public class PathsTests
    {
        FoldersDataService folders = new FoldersDataService();
        PathsDataService paths = new PathsDataService();

        public PathsTests()
        {
            DatabaseService.InitAndEraseAll();
            folders.Create(new Models.Folder { Path = "/", Name = "TestFolder" });
            folders.Create(new Models.Folder { Path = "/", Name = "Test1" });
            folders.Create(new Models.Folder { Path = "/Test1", Name = "Test2a" });
            folders.Create(new Models.Folder { Path = "/Test1", Name = "Test2b" });
            folders.Create(new Models.Folder { Path = "/Test1/Test2a", Name = "Test3a" });
            folders.Create(new Models.Folder { Path = "/Test1/Test2a/Test3a", Name = "Test4a" });
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
            Assert.IsNotNull(id1);
            Assert.IsNotNull(id2);
            Assert.IsFalse(id1 == id2);
        }
    }
}
