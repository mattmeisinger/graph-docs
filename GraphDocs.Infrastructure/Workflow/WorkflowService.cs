﻿using GraphDocs.Core.Interfaces;
using GraphDocs.Infrastructure.Database;
using GraphDocs.Workflow.Neo4jInstanceStore;
using Neo4jClient;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
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
        IGraphClient client;
        ISettingsService settings;
        public WorkflowService(ISettingsService settings)
        {
            this.client = Neo4jConnectionFactory.GetConnection();
            this.settings = settings;
            Console.WriteLine("Store ID: " + settings.WorkflowStoreId);
        }

        public string[] GetAvailableWorkflows()
        {
            // First, get all compiled workflows from the Workflow.Core project
            var exampleWorkflowClass = typeof(GraphDocs.Workflow.Core.SimpleEmailNotification);
            var assembly = exampleWorkflowClass.Assembly;
            Type target = typeof(Activity);
            var activitiesInAssembly = assembly.GetTypes()
                .Where(a => target.IsAssignableFrom(a))
                .Where(a => a.Namespace == exampleWorkflowClass.Namespace) // Should be exactly the same namespace as the example
                .ToArray();
            var activityNamesInAssembly = activitiesInAssembly.Select(a => a.Name).ToArray();

            var activitiesInFolder = System.IO.Directory.EnumerateFiles(settings.WorkflowFolder)
                .Where(a => a.EndsWith(".xaml"))
                .Select(a => a.Split('\\').Last().Replace(".xaml", ""))
                .ToArray();

            return activityNamesInAssembly.Union(activitiesInFolder).ToArray();
        }

        public Activity GetWorkflow(string workflowName)
        {
            var assembly = typeof(GraphDocs.Workflow.Core.SimpleEmailNotification).Assembly;
            Type target = typeof(Activity);
            var match = assembly.GetTypes()
                .Where(a => target.IsAssignableFrom(a) && a.Name == workflowName)
                .FirstOrDefault();

            if (match != null)
                return (Activity)Activator.CreateInstance(match);

            var matchingFilename = System.IO.Directory.EnumerateFiles(settings.WorkflowFolder)
                .Where(a => a.EndsWith("\\" + workflowName + ".xaml"))
                .FirstOrDefault();
            if (matchingFilename != null)
            {
                // Load workflow from XAML file
                Activity activity = ActivityXamlServices.Load(matchingFilename);
                return activity;
            }
            else
            {
                throw new Exception("Unable to find workflow '" + workflowName + "'.");
            }
        }

        public Guid InitializeWorkflow(string workflowName, IDictionary<string, object> parameters)
        {
            using (var store = new Neo4jInstanceStore(client, settings.WorkflowStoreId))
            {
                Activity workflowDefinition = GetWorkflow(workflowName);
                var instanceUnloaded = new AutoResetEvent(false);
                var app = new WorkflowApplication(workflowDefinition, parameters)
                {
                    InstanceStore = store,
                    PersistableIdle = (e) =>
                    {
                        return PersistableIdleAction.Unload;
                    },
                    Unloaded = (workflowApplicationEventArgs) =>
                    {
                        Console.WriteLine("WorkflowApplication has Unloaded.");
                        instanceUnloaded.Set();
                    }
                };
                Guid newInstanceId = app.Id;
                app.Run();
                instanceUnloaded.WaitOne();
                Console.WriteLine("Started instance #" + newInstanceId);
                return newInstanceId;
            }
        }

        public object ResumeWorkflow(string workflowName, Guid workflowInstanceId, string bookmarkName, object bookmarkValue)
        {
            Activity workflowDefinition = GetWorkflow(workflowName);
            using (var store = new Neo4jInstanceStore(client, settings.WorkflowStoreId))
            {
                var instanceUnloaded = new AutoResetEvent(false);
                var app = new WorkflowApplication(workflowDefinition)
                {
                    InstanceStore = store,
                    Completed = (arg) =>
                    {
                        Console.WriteLine("WorkflowApplication has Completed in the {0} state.", arg.CompletionState);
                    },
                    Unloaded = (arg) =>
                    {
                        Console.WriteLine("WorkflowApplication has Unloaded.");
                        instanceUnloaded.Set();
                    }
                };
                app.Load(workflowInstanceId);
                app.ResumeBookmark(bookmarkName, bookmarkValue);
                object ret = null;
                app.Completed = (completedEvent) =>
                {
                    ret = (bool)completedEvent.Outputs["Result"];
                };
                instanceUnloaded.WaitOne();
                return ret;
            }
        }
    }
}
