﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphDocs.Core.Models;

namespace GraphDocs.Tests.IntegrationTests
{
    [TestClass]
    public class FoldersTests : TestBase
    {
        public FoldersTests()
            : base()
        {
            folders.Create(new Folder { Path = "/", Name = "TestFolder" });
            folders.Create(new Folder { Path = "/", Name = "Test1" });
            folders.Create(new Folder { Path = "/Test1", Name = "Test2a" });
            folders.Create(new Folder { Path = "/Test1", Name = "Test2b" });
            folders.Create(new Folder { Path = "/Test1/Test2a", Name = "Test3a" });
            folders.Create(new Folder { Path = "/Test1/Test2a/Test3a", Name = "Test4a" });
            documents.Create(new Document { Path = "/", Name = "doc1.txt" });
            documents.Create(new Document { Path = "/Test1", Name = "doc2.txt" });
        }

        [TestMethod]
        public void GetFolder_Root()
        {
            var folder = folders.Get("/");
            Assert.IsNotNull(folder);
            Assert.IsTrue(folder.ChildFolders.Length == 2);
        }

        [TestMethod]
        public void GetFolder_FolderExists()
        {
            var folder = folders.Get("/Test1");
            Assert.IsNotNull(folder);
            Assert.IsTrue(folder.ChildFolders.Length == 2);
        }

        [TestMethod]
        public void GetFolder_Level2FolderExists()
        {
            var folder = folders.Get("/Test1/Test2a");
            Assert.IsNotNull(folder);
            Assert.IsTrue(folder.ChildFolders.Length == 1);
        }

        [TestMethod]
        public void DeleteFolder_FolderExists()
        {
            // Create a folder and make sure it exists
            folders.Create(new Folder { Path = "/", Name = "FolderToDelete" });
            var id = paths.GetIDFromFolderPath("/FolderToDelete");
            Assert.IsNotNull(id);

            // Now delete the folder and make sure it does not exist any more
            folders.Delete("/FolderToDelete");
            id = paths.GetIDFromFolderPath("/FolderToDelete");
            Assert.IsNull(id);
        }

        [TestMethod]
        public void DeleteFolder_WithChildren()
        {
            folders.Create(new Folder { Path = "/", Name = "FolderToDelete" });
            folders.Create(new Folder { Path = "/FolderToDelete", Name = "SubFolder" });
            folders.Create(new Folder { Path = "/FolderToDelete/SubFolder", Name = "SubSubFolder" });
            var id = paths.GetIDFromFolderPath("/FolderToDelete");
            Assert.IsNotNull(id);
            id = paths.GetIDFromFolderPath("/FolderToDelete/SubFolder");
            Assert.IsNotNull(id);

            folders.Delete("/FolderToDelete");
            id = paths.GetIDFromFolderPath("/FolderToDelete");
            Assert.IsNull(id);
            id = paths.GetIDFromFolderPath("/FolderToDelete/SubFolder");
            Assert.IsNull(id);
        }

        [TestMethod]
        public void CreateFolder_NewFolder()
        {
            folders.Create(new Folder { Path = "/", Name = "NewFolder-D" });
            var id = paths.GetIDFromFolderPath("/NewFolder-D");
            Assert.IsNotNull(id);
        }

        [TestMethod]
        public void CreateFolder_NewSubFolder()
        {
            folders.Create(new Folder { Path = "/", Name = "NewFolder-E" });
            folders.Create(new Folder { Path = "/NewFolder-E", Name = "NewSubFolder-E" });
            var id = paths.GetIDFromFolderPath("/NewFolder-E/NewSubFolder-E");
            Assert.IsNotNull(id);
        }

        [TestMethod]
        public void CreateFolder_NewSubSubFolder()
        {
            folders.Create(new Folder { Path = "/", Name = "NewFolder-F" });
            folders.Create(new Folder { Path = "/NewFolder-F", Name = "NewSubFolder-F" });
            folders.Create(new Folder { Path = "/NewFolder-F/NewSubFolder-F", Name = "NewSubSubFolder-F" });
            var id = paths.GetIDFromFolderPath("/NewFolder-F/NewSubFolder-F/NewSubSubFolder-F");
            Assert.IsNotNull(id);
        }
    }
}
