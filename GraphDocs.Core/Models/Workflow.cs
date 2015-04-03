using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.Core.Models
{
    public class Workflow
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string[] AvailableSettings { get; set; }
    }
}
