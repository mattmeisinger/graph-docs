using GraphDocs.Core.Interfaces;
using GraphDocs.Workflow.Neo4jInstanceStore;
using Neo4jClient;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphDocs.Infrastructure.Workflow
{
    public class WorkflowService : IWorkflowService
    {
        Guid storeId;
        IGraphClient client;
        string activityDefinitionsPath;
        public WorkflowService(IGraphClient client, Guid storeId, string activityDefinitionsPath)
        {
            this.client = client;
            this.activityDefinitionsPath = activityDefinitionsPath;
            this.storeId = new Guid("c068fd97-117e-4bac-b93a-613d7baaa088");
            Console.WriteLine("Store ID: " + storeId);
        }

        public string[] GetAvailableWorkflows()
        {
            // First, get all compiled workflows from the Workflow.Core project
            var assembly = typeof(GraphDocs.Workflow.Core.EmailApprovalRequest).Assembly;
            Type target = typeof(Activity);
            var activitiesInAssembly = assembly.GetTypes()
                .Where(a => target.IsAssignableFrom(a))
                .Select(a => a.Name)
                .ToArray();

            var activitiesInFolder = System.IO.Directory.EnumerateFiles(activityDefinitionsPath)
                .Select(a => a)
                .ToArray();

            return activitiesInAssembly.Union(activitiesInFolder).ToArray();
        }

        public Activity GetWorkflow(string workflowName)
        {
            throw new NotImplementedException();
        }

        public Guid InitializeWorkflow(string workflowName)
        {
            var store = new Neo4jInstanceStore(client, storeId);
            Activity workflow = GetWorkflow(workflowName);
            var instanceId = CreateAndStartWorkflowInstance(store, workflow);
            Console.WriteLine("Started instance #" + instanceId);
            store.Dispose();
            return instanceId;
        }

        public void ResumeWorkflow(string workflowName, Guid workflowInstanceId, string bookmarkName = "OrderNameBookmark")
        {
            var name = Console.ReadLine();
            Activity workflow = GetWorkflow(workflowName);
            var store2 = new Neo4jInstanceStore(client, storeId);
            ResumeWorkflowInstance(store2, workflow, workflowInstanceId, bookmarkName, name);
            store2.Dispose();
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
