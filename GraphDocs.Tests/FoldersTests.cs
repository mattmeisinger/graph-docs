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
            var folders = ds.Get("/AnotherFolder/AnotherChildFolder/AnotherChildFolder2");
        }
    }
}
