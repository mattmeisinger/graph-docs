using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.Core.Models
{
    public class ActiveWorkflow
    {
        public string WorkflowName { get; set; }
        public Dictionary<string, string> Settings { get; set; }
        public int Order { get; set; }

        public string Status { get; set; }
    }
}
