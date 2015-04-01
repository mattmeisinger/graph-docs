﻿using GraphDocs.Core.Enums;
using GraphDocs.Core.Interfaces;
using GraphDocs.Core.Models;
using GraphDocs.Workflow.Neo4jInstanceStore;
using Neo4jClient;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GraphDocs.Infrastructure.Workflow
{
    public class WorkflowService
    {
        IGraphClient client;
        string workflowFolder;
        Guid workflowStoreId;
        public WorkflowService(string workflowFolder, Guid workflowStoreId, IConnectionFactory connFactory)
        {
            this.client = connFactory.GetConnection();
            this.workflowFolder = workflowFolder;
            this.workflowStoreId = workflowStoreId;
            Console.WriteLine("Store ID: " + workflowStoreId);
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

            var activitiesInFolder = System.IO.Directory.EnumerateFiles(this.workflowFolder)
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

            var matchingFilename = System.IO.Directory.EnumerateFiles(workflowFolder)
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

        public WorkflowStatus InitializeWorkflow(string workflowName, IDictionary<string, object> parameters)
        {
            return runWorkflow(workflowName, parameters, null, null, false);
        }

        public WorkflowStatus ResumeWorkflow(string workflowName, Guid? workflowInstanceId, string bookmarkName, bool response)
        {
            return runWorkflow(workflowName, null, workflowInstanceId, bookmarkName, response);
        }

        private WorkflowStatus runWorkflow(string workflowName, IDictionary<string, object> parameters, Guid? workflowInstanceId, string bookmarkName, bool response)
        {
            Activity workflowDefinition = GetWorkflow(workflowName);
            using (var store = new Neo4jInstanceStore(client, workflowStoreId))
            {
                WorkflowStatusEnum status = WorkflowStatusEnum.InProgress;
                string bookmark = null;
                bool? result = null;
                var instanceUnloaded = new AutoResetEvent(false);

                var app = parameters == null ? new WorkflowApplication(workflowDefinition) : new WorkflowApplication(workflowDefinition, parameters);
                app.InstanceStore = store;
                app.PersistableIdle = (e) =>
                {
                    return PersistableIdleAction.Unload;
                };
                app.Completed = (arg) =>
                {
                    Console.WriteLine("Workflow has Completed in the {0} state.", arg.CompletionState);
                    status = WorkflowStatusEnum.Completed;

                    // If the result of the workflow is a boolean, use that value, otherwise the workflow must be so 
                    // simple that it doesn't return anything, so we'll set the result to true.
                    if (arg.Outputs.ContainsKey("Result") && arg.Outputs["Result"] is bool)
                        result = (bool)arg.Outputs["Result"];
                    else
                        result = true;
                };
                app.Unloaded = (arg) =>
                {
                    Console.WriteLine("Workflow has Unloaded.");
                    instanceUnloaded.Set();
                };
                app.Idle = (idle) =>
                {
                    bookmark = idle.Bookmarks.Select(a => a.BookmarkName).FirstOrDefault();
                    Console.WriteLine("Workflow has Idled.");
                };
                if (workflowInstanceId.HasValue)
                {
                    app.Load(workflowInstanceId.Value);
                    app.ResumeBookmark(bookmarkName, response);
                }
                else
                {
                    workflowInstanceId = app.Id;
                    app.Run();
                    Console.WriteLine("Started instance #" + workflowInstanceId.Value);
                }

                // Wait for the workflow to complete before returning the data
                instanceUnloaded.WaitOne();
                return new WorkflowStatus
                {
                    InstanceId = workflowInstanceId.Value,
                    Status = status,
                    Bookmark = bookmark,
                    Result = result
                };
            }
        }
    }
}
