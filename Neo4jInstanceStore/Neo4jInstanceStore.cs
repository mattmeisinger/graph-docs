using System;
using System.Linq;
using System.Xml;

namespace CustomInstanceStore.Neo4j
{
    /// <summary>
    /// Persists instance state data to a Neo4J database.
    /// </summary>
    public class Neo4jInstanceStore : CustomInstanceStoreBase
    {
        Neo4jClient.IGraphClient _client;

        public Neo4jInstanceStore(Neo4jClient.IGraphClient client, Guid storeId) : base(storeId)
        {
            this._client = client;
        }

        public override void Save(Guid instanceId, Guid storeId, XmlDocument doc)
        {
            _client.Cypher
                .WithParams(new
                {
                    instanceId = instanceId.ToString(),
                    storeId = storeId.ToString(),
                    item = doc.OuterXml
                })
                .Merge("(i:SavedWorkflowInstance { InstanceID: {instanceId}, StoreID: {storeId} })")
                .Set("i += { XmlContent: {item} }")
                .ExecuteWithoutResults();
        }
        public override XmlDocument Load(Guid instanceId, Guid storeId)
        {
            string xml = _client.Cypher
                .WithParams(new
                {
                    instanceId = instanceId.ToString(),
                    storeId = storeId.ToString()
                })
                .Match("(i:SavedWorkflowInstance { InstanceID: {instanceId}, StoreID: {storeId} })")
                .Return<string>("i.XmlContent")
                .Results
                .SingleOrDefault();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            return xmlDoc;
        }
    }
}