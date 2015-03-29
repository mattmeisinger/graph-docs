using GraphDocs.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.Core.Interfaces
{
    public interface IWorkflowService
    {
        string[] GetAvailableWorkflows();
        WorkflowStatus InitializeWorkflow(string workflowName, IDictionary<string, object> parameters);
        WorkflowStatus ResumeWorkflow(string workflowName, Guid workflowInstanceId, string bookmarkName, object bookmarkValue);
    }
}
