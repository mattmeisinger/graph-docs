using GraphDocs.Core.Enums;
using GraphDocs.Core.Interfaces;
using GraphDocs.Core.Models;
using GraphDocs.Infrastructure.Workflow;
using Neo4jClient;
using System;
using System.Linq;

namespace GraphDocs.Infrastructure
{
    public class DocumentsWorkflowsService
    {
        private IGraphClient client;
        private WorkflowService workflowService;

        public DocumentsWorkflowsService(IConnectionFactory connFactory, WorkflowService workflowService)
        {
            this.client = connFactory.GetConnection();
            this.workflowService = workflowService;
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
                                Status = WorkflowStatusEnum.Pending.ToString(),
                                Order = item.Order,
                                Bookmark = "",
                                InstanceId = ""
                            }
                        })
                        .Match("(d:Document { ID: { documentId } })")
                        .Create("d<-[:ASSIGNED_TO]-(aw:ActiveWorkflow { activeWorkflow })")
                        .ExecuteWithoutResults();
                }

                // Then set the first records to 'True'
                ActivateWorkflowOnDocument(document);
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

        public void SubmitWorkflowReply(Document document, string workflowName, string bookmark, bool response)
        {
            var workflow = client.Cypher
                .WithParams(new { documentId = document.ID, workflowName = workflowName })
                .Match("(:Document { ID: {documentId} })<-[:ASSIGNED_TO]-(aw:ActiveWorkflow { WorkflowName: {workflowName} })")
                .Return(aw => aw.As<ActiveWorkflow>())
                .Results
                .FirstOrDefault();

            if (workflow == null)
                throw new Exception("Workflow not found");
            if (workflow.Status == WorkflowStatusEnum.Pending.ToString())
                throw new Exception("This workflow has not been started yet. It cannot receive a reply.");
            if (workflow.Status == WorkflowStatusEnum.Completed.ToString())
                throw new Exception("This workflow is already complete. No input is necessary.");

            var status = workflowService.ResumeWorkflow(workflow.WorkflowName, new Guid(workflow.InstanceId), bookmark, response);
            SetStatusOnWorkflow(document.ID, workflow, status);
            ActivateWorkflowOnDocument(document);
        }

        private void ActivateWorkflowOnDocument(Document document)
        {
            document.ActiveWorkflows = client.Cypher
                .WithParams(new { documentId = document.ID })
                .Match("(:Document { ID: {documentId} })<-[:ASSIGNED_TO]-(aw:ActiveWorkflow)")
                .Return(aw => aw.As<ActiveWorkflow>())
                .Results
                .ToArray();

            if (document.ActiveWorkflows.Any(a => a.Status == WorkflowStatusEnum.Rejected.ToString()))
            {
                // Stop processing if any workflows failed. That means this document will never activate.
                return;
            }
            else if (document.ActiveWorkflows.All(a => a.Status == WorkflowStatusEnum.Completed.ToString()))
            {
                // Stop processing and just set the documents 'active' status to 'true' if all workflows are complete.
                client.Cypher
                    .WithParams(new { documentId = document.ID, active = true })
                    .Match("(d:Document { ID: {documentId} })")
                    .Set("d.Active = {active}")
                    .ExecuteWithoutResults();

                return;
            }
            else if (document.ActiveWorkflows.Any(a => a.Status == WorkflowStatusEnum.InProgress.ToString()))
            {
                // At least one workflow is waiting for input from the user, so return without doing anything.
                return;
            }

            // If there is a workflow that is of a status other than pending, then a workflow is either activated,
            // or there is failure on an existing workflow.
            var nonPendingActiveWorkflows = document.ActiveWorkflows.Where(a => a.Status != "Pending" && a.Status != "Completed").ToArray();
            if (nonPendingActiveWorkflows.Any())
                throw new Exception("Workflows cannot be activated for this document because one workflow has the status of: " + nonPendingActiveWorkflows.First().Status);

            // Get the next set of workflows to start, based on order. For instance, if there are two ActiveWorkflow
            // instances with order = 0, and one with order = 1, then the two with order = 0 would be started first. If
            // those two were complete, then the order = 1 would be started.
            var nextWorkflowsToStart = document.ActiveWorkflows
                .Where(a => a.Status == "Pending")
                .GroupBy(a => a.Order)
                .OrderBy(a => a.Key)
                .First()
                .ToArray();

            foreach (var workflow in nextWorkflowsToStart)
            {
                var parameters = workflow.Settings.ToDictionary(a => a.Key, a => (object)a.Value);
                parameters.Add("Document", document);
                var status = workflowService.InitializeWorkflow(workflow.WorkflowName, parameters);
                SetStatusOnWorkflow(document.ID, workflow, status);
            }

            // This method will keep calling itself until there is nothing left to do. It may go through 
            // many workflows before it completes, if none of them require user feedback.
            ActivateWorkflowOnDocument(document);
        }

        private void SetStatusOnWorkflow(string documentId, ActiveWorkflow workflow, WorkflowStatus status)
        {
            var isCompletedWithResultOfFalse = status.Status == WorkflowStatusEnum.Completed && status.Result.HasValue && status.Result.Value == false;
            workflow.Status = isCompletedWithResultOfFalse ? WorkflowStatusEnum.Rejected.ToString() : status.Status.ToString();
            workflow.InstanceId = status.InstanceId.ToString();
            workflow.Bookmark = status.Bookmark ?? "";

            // Save ActiveWorkflow
            client.Cypher
                .WithParams(new
                {
                    documentId = documentId,
                    workflowName = workflow.WorkflowName,
                    workflow = workflow
                })
                .Match("(:Document { ID: {documentId} })<-[:ASSIGNED_TO]-(aw:ActiveWorkflow { WorkflowName: {workflowName} })")
                .Set("aw = {workflow}")
                .ExecuteWithoutResults();
        }
    }
}
