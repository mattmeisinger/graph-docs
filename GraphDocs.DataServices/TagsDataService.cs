using GraphDocs.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.DataServices
{
    public class TagsDataService
    {
        private GraphClient client;
        public TagsDataService()
        {
            client = DatabaseService.GetConnection();
        }

        public Tag GetOrCreateTagByName(string name)
        {

            return client.Cypher
                .WithParams(new
                {
                    name
                })
                .Merge("(t:Tag { Name = {name})")
                .Return(t => t.As<Tag>())
                .Results
                .Single();
        }
    }
}
