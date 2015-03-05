using GraphDocs.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.DataServices
{
    public class DatabaseService
    {
        public static GraphClient GetConnection()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();
            return client;
        }

        public static void InitAndEraseAll()
        {
            EraseAll();
            Init();
        }

        public static void Init()
        {
            var client = GetConnection();

            // Create root folder if it doesn't exist already
            var rootFolderExists = client.Cypher
                .Match("(folder:Folder)")
                .Where((Folder folder) => folder.Name == "Root")
                .Return(folder => folder.As<Folder>())
                .Results.Any();
            if (!rootFolderExists)
            {
                client.Cypher
                    .WithParams(new
                    {
                        newFolder = new
                        {
                            ID = Guid.NewGuid().ToString(),
                            Name = "Root"
                        }
                    })
                    .Create("(folder:Folder {newFolder})")
                    .ExecuteWithoutResults();
            }
        }

        public static void EraseAll()
        {
            var client = GetConnection();
            client.Cypher
                .Match("(i)")
                .OptionalMatch("(i)-[r]-()")
                .Delete("i, r")
                .ExecuteWithoutResults();
        }
    }
}
