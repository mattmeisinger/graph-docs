using CustomInstanceStore.Neo4j;
using GraphDocs.Core.Enums;
using GraphDocs.Core.Interfaces;
using GraphDocs.Core.Models;
using Neo4jClient;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xaml;

namespace GraphDocs.Infrastructure
{
    public class WorkflowService : IWorkflowService
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

        public Core.Models.Workflow[] GetAvailableWorkflows()
        {
            // First, get all compiled workflows from the Workflow.Core project
            var exampleWorkflowClass = typeof(GraphDocs.Infrastructure.Workflow.SimpleEmailNotification);
            var assembly = exampleWorkflowClass.Assembly;
            Type target = typeof(Activity);
            var activitiesInAssembly = assembly.GetTypes()
                .Where(a => target.IsAssignableFrom(a))
                .Where(a => a.Namespace == exampleWorkflowClass.Namespace) // Should be exactly the same namespace as the example
                .ToArray();
            var activityNamesInAssembly = activitiesInAssembly.Select(a => new Core.Models.Workflow { Name = a.Name, Type = "Built-In" }).ToArray();

            var activitiesInFolder = Directory.EnumerateFiles(workflowFolder)
                .Where(a => a.EndsWith(".xaml"))
                .Select(a => a.Split('\\').Last().Replace(".xaml", ""))
                .Select(a => new Core.Models.Workflow { Name = a, Type = "User-Defined" })
                .ToArray();

            var allWorkflows = activityNamesInAssembly.Union(activitiesInFolder).ToArray();

            foreach (var workflow in allWorkflows)
            {
                workflow.AvailableSettings = getAvailableSettingsForWorkflow(GetWorkflow(workflow.Name));
            }

            return allWorkflows;
        }

        private string[] getAvailableSettingsForWorkflow(Activity wf)
        {
            if (wf is DynamicActivity)
            {
                // Dynamic activities are not compiled (yet), so we can't use reflection to get their
                // properties like we can normal activities.
                return (wf as DynamicActivity).Properties.Select(a => a.Name).ToArray();
            }
            else
            {
                var ret = new List<string>();
                var properties = wf.GetType().GetProperties();
                foreach (var property in properties)
                {
                    if (property.PropertyType.Name.StartsWith("InArgument") || property.PropertyType.Name.StartsWith("InOutArgument"))
                    {
                        ret.Add(property.Name);
                    }
                }
                return ret.ToArray();
            }
        }

        public Activity GetWorkflow(string workflowName)
        {
            var workflowCoreAssembly = typeof(GraphDocs.Infrastructure.Workflow.SimpleEmailNotification).Assembly;
            Type target = typeof(Activity);
            var match = workflowCoreAssembly.GetTypes()
                .Where(a => target.IsAssignableFrom(a) && a.Name == workflowName)
                .FirstOrDefault();

            if (match != null)
                return (Activity)Activator.CreateInstance(match);

            var workflowFiles = Directory.EnumerateFiles(workflowFolder).ToArray();
            var matchingFilename = workflowFiles
                .Where(a => a.EndsWith("\\" + workflowName + ".xaml"))
                .FirstOrDefault();
            if (matchingFilename != null)
            {
                // Load workflow from XAML file. Need to reference the core assembly as the LocalAssembly
                // while loading though or the custom activities will not work.
                //var o = XamlServices.Parse(@"<ApproveDocument xmlns=""clr-namespace:GraphDocs.Infrastructure.Workflow""/>");
                //var workflowText = System.IO.File.ReadAllText(matchingFilename);
                //return ActivityXamlServices.Load(new System.IO.MemoryStream(Encoding.Default.GetBytes(workflowText)));
                //var o = XamlServices.Parse();

                var readerSettings = new XamlXmlReaderSettings { LocalAssembly = System.Reflection.Assembly.GetExecutingAssembly() };
                var reader = new XamlXmlReader(matchingFilename, readerSettings);

                // This has to be a dynamic activity and have compile expressions = true, or it will appear to work but the workflow will never start.
                var activityLoadSettings = new ActivityXamlServicesSettings { CompileExpressions = true };
                var dynamicActivity = ActivityXamlServices.Load(reader, activityLoadSettings) as DynamicActivity;

                return dynamicActivity;
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

                WorkflowApplication app;
                
                if (parameters == null)
                {
                    app = new WorkflowApplication(workflowDefinition);
                }
                else
                {
                    // Only pass in parameters that the appear in the workflow
                    var parametersDefinedInThisWorkflow = getAvailableSettingsForWorkflow(workflowDefinition);
                    var applicableParameters = parameters
                        .Where(a => parametersDefinedInThisWorkflow.Contains(a.Key))
                        .ToDictionary(a => a.Key, a => a.Value);
                    app = new WorkflowApplication(workflowDefinition, applicableParameters);
                }
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

        public void Create(string name, Stream inputStream)
        {
            var path = Path.Combine(workflowFolder, name + ".xaml");
            using (var filestream = File.Create(path))
            {
                inputStream.CopyTo(filestream);
            }
        }

        public void Delete(string name)
        {
            var path = Path.Combine(workflowFolder, name + ".xaml");
            if (!File.Exists(path))
                throw new Exception("Workflow file '" + name + "' does not exist, or is not a user-defined workflow.");

            File.Delete(path);
        }
    }
}
