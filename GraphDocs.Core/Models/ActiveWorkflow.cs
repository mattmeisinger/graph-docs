using System.Collections.Generic;

namespace GraphDocs.Core.Models
{
    public class ActiveWorkflow
    {
        public string WorkflowName { get; set; }
        public Dictionary<string, string> Settings { get; set; }
        public int Order { get; set; }
        public string Status { get; set; }
        public string InstanceId { get; set; }
        public string Bookmark { get; set; }
    }
}
