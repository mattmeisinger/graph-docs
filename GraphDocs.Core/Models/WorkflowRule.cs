using System.Collections.Generic;

namespace GraphDocs.Core.Models
{
    public class WorkflowRule
    {
        public Workflow Workflow { get; set; }
        public Dictionary<string, string> Settings { get; set; }
    }
}
