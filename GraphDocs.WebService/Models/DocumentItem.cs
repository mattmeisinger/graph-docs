using GraphDocs.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GraphDocs.WebService.Models
{
    public class DocumentItem
    {
        public string name { get; set; }
        public string[] tags { get; set; }

        internal Document ToDocument()
        {
            return new Document
            {
                Name = name,
                Tags = tags
            };
        }
    }
}