using System;
using System.Collections.Generic;
using System.Linq;
using GraphDocs.Core.Models;

namespace GraphDocs.WebService.Models
{
    public class FolderItem
    {
        public string name { get; set; }
        public List<WorkflowDefinition> workflowDefinitions { get; set; }

        public class WorkflowDefinition
        {
            public string workflowName;
            public int order;
            public Dictionary<string, string> settings;
        }

        internal Folder ConvertToFolder()
        {
            return new Folder
            {
                Name = name,
                WorkflowDefinitions = workflowDefinitions == null
                    ? null
                    : workflowDefinitions
                        .Select(a => new Core.Models.WorkflowDefinition
                        {
                            WorkflowName = a.workflowName,
                            Order = a.order,
                            Settings = a.settings
                        })
                        .ToArray()
            };
        }
    }
}