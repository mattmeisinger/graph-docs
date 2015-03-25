using System;
using System.Activities;
using System.Activities.DurableInstancing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Runtime.DurableInstancing;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Threading;
using WorkflowActivities;
using GraphDocs.Workflow.Neo4jInstanceStore;
using Neo4jClient;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var neoConnectionString = "http://localhost:7474/db/data";
            var neoConnection = new GraphClient(new Uri(neoConnectionString));

            //setup persistence
            var storeId = new Guid("c068fd97-117e-4bac-b93a-613d7baaa088");
            Console.WriteLine("Store ID: " + storeId);
            var store = new Neo4jInstanceStore(neoConnection, storeId);
            var instanceId = CreateAndStartWorkflowInstance(store, new TestWorkflow2());
            Console.WriteLine("Instance ID: " + instanceId);
            store.Dispose();

            //resume
            var bookmarkName = "OrderNameBookmark";
            var name = Console.ReadLine();
            var store2 = new Neo4jInstanceStore(neoConnection, storeId);
            ResumeWorkflowInstance(store2, new TestWorkflow2(), instanceId, bookmarkName, name);
            store2.Dispose();

            Console.ReadLine();
        }

        private static Guid CreateAndStartWorkflowInstance(InstanceStore store, Activity workflowDefinition)
        {
            var instanceUnloaded = new AutoResetEvent(false);
            var app = new WorkflowApplication(workflowDefinition)
            {
                InstanceStore = store,
                PersistableIdle = (e) =>
                {
                    return PersistableIdleAction.Unload;
                },
                Unloaded = (workflowApplicationEventArgs) =>
                {
                    Console.WriteLine("WorkflowApplication has Unloaded\n");
                    instanceUnloaded.Set();
                }
            };
            Guid newInstanceId = app.Id;
            app.Run();
            instanceUnloaded.WaitOne();
            return newInstanceId;
        }

        private static void ResumeWorkflowInstance(InstanceStore store, Activity workflowDefinition, Guid instanceId, string bookmarkName, object bookmarkValue)
        {
            var instanceUnloaded = new AutoResetEvent(false);
            var app = new WorkflowApplication(workflowDefinition)
            {
                InstanceStore = store,
                Completed = (arg) =>
                {
                    Console.WriteLine("\nWorkflowApplication has Completed in the {0} state.", arg.CompletionState);
                },
                Unloaded = (arg) =>
                {
                    Console.WriteLine("WorkflowApplication has Unloaded\n");
                    instanceUnloaded.Set();
                }
            };
            app.Load(instanceId);
            app.ResumeBookmark(bookmarkName, bookmarkValue);
            instanceUnloaded.WaitOne();
        }
    }
}
