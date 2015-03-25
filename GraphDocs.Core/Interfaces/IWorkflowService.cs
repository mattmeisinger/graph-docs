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
        Guid InitializeWorkflow(string workflowName, IDictionary<string, object> parameters);
        void ResumeWorkflow(string workflowName, Guid workflowInstanceId, string bookmarkName, object bookmarkValue);
    }
}
