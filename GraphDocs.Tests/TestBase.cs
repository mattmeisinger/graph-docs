using GraphDocs.Core.Interfaces;
using GraphDocs.Infrastructure;
using GraphDocs.Infrastructure.Database;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GraphDocs.Tests
{
    public class TestBase
    {
        internal IConnectionFactory connFactory;
        internal IFoldersDataService folders;
        internal IPathsDataService paths;
        internal IDocumentsDataService documents;
        internal IDocumentFilesDataService documentFiles;
        internal IDocumentsWorkflowsService documentsWorkflows;
        internal IWorkflowService workflows;

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public TestBase()
        {
            connFactory = new Neo4jConnectionFactory();
            paths = new PathsDataService(connFactory);
            folders = new FoldersDataService(connFactory, paths);
            workflows = new WorkflowService(AssemblyDirectory + "\\" + ConfigurationManager.AppSettings["WorkflowFolder"], new Guid(ConfigurationManager.AppSettings["WorkflowStoreId"]), connFactory);
            documentFiles = new DocumentFilesDataService(connFactory, paths);
            documentsWorkflows = new DocumentsWorkflowsService(connFactory, workflows,documentFiles);
            documents = new DocumentsDataService(connFactory, paths, documentsWorkflows);

            connFactory.InitAndEraseAll();
        }
    }
}
