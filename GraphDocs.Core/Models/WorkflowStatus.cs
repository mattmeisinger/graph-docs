using GraphDocs.Core.Enums;
using System;

namespace GraphDocs.Core.Models
{
    public class WorkflowStatus
    {
        public Guid InstanceId { get; set; }
        public WorkflowStatusEnum Status { get; set; }
        public bool? Result { get; set; }
        public string Bookmark { get; set; }
    }
}
