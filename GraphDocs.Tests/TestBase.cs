using GraphDocs.Core.Interfaces;
using GraphDocs.Infrastructure;
using GraphDocs.Infrastructure.Database;
using GraphDocs.Infrastructure.Workflow;
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
        internal FoldersDataService folders;
        internal PathsDataService paths;
        internal DocumentsDataService documents;
        internal DocumentFilesDataService documentFiles;
        internal DocumentsWorkflowsService documentsWorkflows;
        internal WorkflowService workflows;

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
            documentsWorkflows = new DocumentsWorkflowsService(connFactory, workflows);
            documents = new DocumentsDataService(connFactory, paths, documentsWorkflows);
            documentFiles = new DocumentFilesDataService(connFactory, paths);

            connFactory.InitAndEraseAll();
        }
    }
}
