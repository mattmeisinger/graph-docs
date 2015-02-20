using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.DataServices
{
    public class FoldersDataService
    {
        public static void Test()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();
            client.Cypher
                .Create("(user:User {newUser})")
                .WithParam("newUser", new User { Name = "TestUser1", Address = "Test Address" })
                .ExecuteWithoutResults();
            var results = client.Cypher
                .Match("(user:User)")
                .Return(user => user.As<User>())
                .Results;
        }

        public class User
        {
            public string Name { get; set; }
            public string Address { get; set; }
        }
    }
}
