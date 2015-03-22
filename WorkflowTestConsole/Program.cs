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
using Neo4jWorkflowInstanceStore;
using System.Threading;
using WorkflowActivities;

namespace ConsoleApplication1
{
    class Program
    {
        static AutoResetEvent instanceUnloaded = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            var app = new WorkflowApplication(new TestWorkflow2());

            //setup persistence
            var neoConnectionString = "http://localhost:7474/db/data";
            var store = new Neo4jWorkflowInstanceStore.Neo4jInstanceStore(neoConnectionString);
            InstanceHandle handle = store.CreateInstanceHandle();
            InstanceView view = store.Execute(handle, new CreateWorkflowOwnerCommand(), TimeSpan.FromSeconds(30));
            handle.Free();
            store.DefaultInstanceOwner = view.InstanceOwner;
            app.InstanceStore = store;
            app.PersistableIdle = (e) =>
            {
                return PersistableIdleAction.Unload;
            };
            app.Unloaded = (workflowApplicationEventArgs) =>
            {
                Console.WriteLine("WorkflowApplication has Unloaded\n");
                instanceUnloaded.Set();
            };

            Guid id = app.Id;
            app.Run();
            Console.WriteLine("Host thread: " + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
            instanceUnloaded.WaitOne();

            //resume
            string name = Console.ReadLine();

            app = new WorkflowApplication(new TestWorkflow2());
            app.InstanceStore = store;

            app.Completed = (arg) =>
            {
                Console.WriteLine("\nWorkflowApplication has Completed in the {0} state.", arg.CompletionState);
            };
            app.Unloaded = (arg) =>
            {
                Console.WriteLine("WorkflowApplication has Unloaded\n");
                instanceUnloaded.Set();
            };

            app.Load(id);

            app.ResumeBookmark("OrderNameBookmark", name);
            Console.WriteLine("Host thread: " + Thread.CurrentThread.ManagedThreadId.ToString());
            instanceUnloaded.WaitOne();

            Console.ReadLine();
        }
    }
}
