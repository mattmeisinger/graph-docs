using GraphDocs.Core.Models;
using GraphDocs.Infrastructure.Workflow;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.Infrastructure
{
    public class DocumentsWorkflowsService
    {
        private IGraphClient client;
        private WorkflowService workflowService;

        public DocumentsWorkflowsService(IGraphClient client)
        {
            this.client = client;
            this.workflowService = new WorkflowService();
        }

        public void InitializeWorkflowsForDocument(string folderId, Document document)
        {
            var folderWorkflowDefinitions = client.Cypher
                .WithParams(new { folderId })
                .Match("(folder:Folder { ID: {folderId} })<-[:APPLIES_TO]-(wd:WorkflowDefinition)")
                .Return(wd => wd.As<WorkflowDefinition>())
                .Results
                .ToArray();

            if (folderWorkflowDefinitions.Any())
            {
                // If there are any workflows defined for the folder, then attach ActiveWorkflow
                // records to the document for each one.
                foreach (var item in folderWorkflowDefinitions)
                {
                    client.Cypher
                        .WithParams(new
                        {
                            documentId = document.ID,
                            activeWorkflow = new ActiveWorkflow
                            {
                                WorkflowName = item.WorkflowName,
                                Settings = item.Settings,
                                Status = "Pending"
                            }
                        })
                        .Match("(d:Document { ID: { documentId } })")
                        .Create("d<-[:ASSIGNED_TO]-(aw:ActiveWorkflow { activeWorkflow })")
                        .ExecuteWithoutResults();
                }

                // Then set the first records to 'True'
                this.ActivateWorkflowOnDocument(document);
            }
            else
            {
                // If there are no workflows defined for the folder, just set the Active flag on the document to 'True'
                client.Cypher
                    .WithParams(new
                    {
                        documentId = document.ID
                    })
                    .Match("(d:Document { ID: { documentId } })")
                    .Set("d.Active = 'True'")
                    .ExecuteWithoutResults();
            }
        }

        private void ActivateWorkflowOnDocument(Document document)
        {
            var activeWorkflows = client.Cypher
                .WithParams(new { documentId = document.ID })
                .Match("(d:Document { ID: {documentId} })<-[:ASSIGNED_TO]-(aw:ActiveWorkflow)")
                .Return(aw => aw.As<ActiveWorkflow>())
                .Results
                .ToArray();

            // If there is a workflow that is of a status other than pending, then a workflow is either activated,
            // or there is failure on an existing workflow.
            var nonPendingActiveWorkflows = activeWorkflows.Where(a => a.Status != "Pending" && a.Status != "Complete").ToArray();
            if (nonPendingActiveWorkflows.Any())
                throw new Exception("Workflows cannot be activated for this document because one workflow has the status of: " + nonPendingActiveWorkflows.First().Status);

            // Get the next set of workflows to start, based on order. For instance, if there are two ActiveWorkflow
            // instances with order = 0, and one with order = 1, then the two with order = 0 would be started first. If
            // those two were complete, then the order = 1 would be started.
            var nextWorkflowsToStart = activeWorkflows
                .Where(a => a.Status == "Pending")
                .GroupBy(a => a.Order)
                .OrderBy(a => a.Key)
                .First()
                .ToArray();

            foreach (var workflow in nextWorkflowsToStart)
            {
                var parameters = workflow.Settings.ToDictionary(a => a.Key, a => (object)a.Value);
                parameters.Add("Document", document);
                var workflowID = workflowService.InitializeWorkflow(workflow.WorkflowName, parameters);
            }
        }
    }
}
