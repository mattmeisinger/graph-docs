using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphDocs.DataServices;

namespace GraphDocs.Tests
{
    [TestClass]
    public class FoldersTests
    {
        [TestMethod]
        public void Test()
        {
            var ds = new FoldersDataService();
            ds.Create("Test1", "/");
            ds.Create("Test2", "/Test1");
            var folders = ds.Get("/Test1");
            //var folders2 = ds.Get("/AnotherFolder/AnotherChildFolder");
        }
    }
}
