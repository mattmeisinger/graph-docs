using System;

namespace GraphDocs.Core.Models
{
    public class WorkflowStatus
    {
        public Guid InstanceId { get; set; }
        public string Status { get; set; }
        public object Result { get; set; }
    }
}
