using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.Models
{
    [DataContract]
    public class Tag
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Path
        {
            get
            {
                return "/tags/" + Name;
            }
        }
    }
}
