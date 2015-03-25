using System;
using System.Linq;
using GraphDocs.Core.Models;
using Neo4jClient;
using GraphDocs.Infrastructure.Database;

namespace GraphDocs.Infrastructure
{
    public class TagsDataService
    {
        private IGraphClient client;
        private PathsDataService paths;
        public TagsDataService()
        {
            client = Neo4jConnectionFactory.GetConnection();
            paths = new PathsDataService(client);
        }

        public Tag GetOrCreateTagByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("New tag name cannot be null.");

            return client.Cypher
                .WithParams(new
                {
                    name
                })
                .Merge("(t:Tag { Name: {name})")
                .Return(t => t.As<Tag>())
                .Results
                .Single();
        }

        public Tag[] Get()
        {
            return client.Cypher
                .Match("(t:Tag)")
                .Return(t => t.As<Tag>())
                .Results
                .ToArray();
        }

        public Tag Get(string name)
        {
            var ret = client.Cypher
                .WithParams(new
                {
                    name
                })
                .Merge("(t:Tag { Name: {name} })")
                .Return(t => t.As<Tag>())
                .Results
                .SingleOrDefault();

            if (ret != null)
            {
                ret.RelatedDocuments = client.Cypher
                    .WithParams(new
                    {
                        name
                    })
                    .Match("(:Tag { Name: {name} })-[:DESCRIBES]->(d:Document)")
                    .Return<Document>("d")
                    .Results
                    .ToArray();

                foreach (var item in ret.RelatedDocuments)
                    item.Path = paths.GetPathFromDocumentID(item.ID);
            }
            return ret;
        }

        public void Rename(string oldName, string newName)
        {
            client.Cypher
                .WithParams(new
                {
                    oldName
                })
                .Match("(t:Tag { Name: {oldName} })")
                .Set("t.Name = {newName}")
                .ExecuteWithoutResults();
        }

        public void Delete(string name)
        {
            client.Cypher
                .WithParams(new
                {
                    name
                })
                .Match("(t:Tag { Name: {name} })-[rel*0..]-()")
                .Delete("t, rel")
                .ExecuteWithoutResults();
        }
    }
}
