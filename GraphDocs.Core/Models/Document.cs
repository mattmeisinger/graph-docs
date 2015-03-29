﻿namespace GraphDocs.Core.Models
{
    public class Document
    {
        // Active is only set to true once all 'Create' workflows are completed successfully.
        public bool Active { get; set; }

        public bool HasFile { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string[] Tags { get; set; }

        public ActiveWorkflow[] ActiveWorkflows { get; set; }
    }
}
