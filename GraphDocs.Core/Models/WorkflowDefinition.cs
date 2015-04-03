using System;
using System.Collections.Generic;

namespace GraphDocs.Core.Models
{
    public class WorkflowDefinition
    {
        public string WorkflowName { get; set; }
        public Dictionary<string, string> Settings { get; set; }
        public int Order { get; set; }
    }
}
