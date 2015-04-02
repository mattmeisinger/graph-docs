using System;
using System.Activities;
using System.Collections.Generic;
using GraphDocs.Core.Models;

namespace GraphDocs.Core.Interfaces
{
    public interface IWorkflowService
    {
        string[] GetAvailableWorkflows();
        Activity GetWorkflow(string workflowName);
        WorkflowStatus InitializeWorkflow(string workflowName, IDictionary<string, object> parameters);
        WorkflowStatus ResumeWorkflow(string workflowName, Guid? workflowInstanceId, string bookmarkName, bool response);
    }
}