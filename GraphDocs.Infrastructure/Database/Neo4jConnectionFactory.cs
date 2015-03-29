using System;
using System.Linq;
using Neo4jClient;
using GraphDocs.Core.Models;
using GraphDocs.Core.Interfaces;
using GraphDocs.Infrastructure.Database.Converters;

namespace GraphDocs.Infrastructure.Database
{
    public class Neo4jConnectionFactory
    {
        public static IGraphClient GetConnection()
        {
            var connectionString = "http://localhost:7474/db/data";
            var client = new GraphClient(new Uri(connectionString));
            try
            {
                client.Connect();
            }
            catch (Exception)
            {
                throw new Exception("Unable to connect to database at: " + connectionString);
            }
            client.JsonConverters.Add(new WorkflowDefinitionConverter());
            client.JsonConverters.Add(new ActiveWorkflowConverter());
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
