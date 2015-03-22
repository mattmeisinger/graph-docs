using System;
using System.Activities.DurableInstancing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Neo4jWorkflowInstanceStore
{
    public class Neo4jInstanceStore : InstanceStore
    {
        /// <summary>
        /// The current instance of a workflow that this instance store is going to save or retrieve.
        /// </summary>
        Guid workflowInstanceId;
        Neo4jClient.GraphClient client;

        public Neo4jInstanceStore(string connectionString) : this(connectionString, Guid.NewGuid()) { }
        public Neo4jInstanceStore(string connectionString, Guid id)
        {
            client = new Neo4jClient.GraphClient(new Uri(connectionString));
            client.Connect();
            workflowInstanceId = id;
        }

        private void SaveXMLDocument(Guid guid, XmlDocument doc)
        {
            client.Cypher
                .WithParams(new
                {
                    id = guid.ToString(),
                    item = doc.OuterXml
                })
                .Create("(:SavedWorkflowInstance { ID: {id}, XmlContent: {item}})")
                .ExecuteWithoutResults();
        }
        private XmlDocument LoadXMLDocument(Guid guid)
        {
            var xml = client.Cypher
                .WithParams(new
                {
                    id = guid.ToString()
                })
                .Match("(i:SavedWorkflowInstance { ID: {id} })")
                .Return<string>("i.XmlContent")
                .Results
                .SingleOrDefault();

            // convert the string stored in Neo4j into an xml document, and return.
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            return xmlDoc;
        }

        //Synchronous version of the Begin/EndTryCommand functions
        protected override bool TryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout)
        {
            return EndTryCommand(BeginTryCommand(context, command, timeout, null, null));
        }

        //The persistence engine will send a variety of commands to the configured InstanceStore,
        //such as CreateWorkflowOwnerCommand, SaveWorkflowCommand, and LoadWorkflowCommand.
        //This method is where we will handle those commands
        protected override IAsyncResult BeginTryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            IDictionary<System.Xml.Linq.XName, InstanceValue> data = null;

            //The CreateWorkflowOwner command instructs the instance store to create a new instance owner bound to the instanace handle
            if (command is CreateWorkflowOwnerCommand)
            {
                context.BindInstanceOwner(workflowInstanceId, Guid.NewGuid());
            }
            //The SaveWorkflow command instructs the instance store to modify the instance bound to the instance handle or an instance key
            else if (command is SaveWorkflowCommand)
            {
                SaveWorkflowCommand saveCommand = (SaveWorkflowCommand)command;
                data = saveCommand.InstanceData;

                Save(data);
            }
            //The LoadWorkflow command instructs the instance store to lock and load the instance bound to the identifier in the instance handle
            else if (command is LoadWorkflowCommand)
            {
                var xml = LoadXMLDocument(workflowInstanceId);
                data = LoadInstanceDataFromFile(xml);
                //load the data into the persistence Context
                context.LoadedInstance(InstanceState.Initialized, data, null, null, null);
            }

            return new CompletedAsyncResult<bool>(true, callback, state);
        }

        protected override bool EndTryCommand(IAsyncResult result)
        {
            return CompletedAsyncResult<bool>.End(result);
        }

        //Reads data from xml file and creates a dictionary based off of that.
        IDictionary<System.Xml.Linq.XName, InstanceValue> LoadInstanceDataFromFile(XmlDocument doc)
        {
            IDictionary<System.Xml.Linq.XName, InstanceValue> data = new Dictionary<System.Xml.Linq.XName, InstanceValue>();

            NetDataContractSerializer s = new NetDataContractSerializer();

            XmlNodeList instances = doc.GetElementsByTagName("InstanceValue");
            foreach (XmlElement instanceElement in instances)
            {
                XmlElement keyElement = (XmlElement)instanceElement.SelectSingleNode("descendant::key");
                System.Xml.Linq.XName key = (System.Xml.Linq.XName)DeserializeObject(s, keyElement);

                XmlElement valueElement = (XmlElement)instanceElement.SelectSingleNode("descendant::value");
                object value = DeserializeObject(s, valueElement);
                InstanceValue instVal = new InstanceValue(value);

                data.Add(key, instVal);
            }

            return data;
        }

        object DeserializeObject(NetDataContractSerializer serializer, XmlElement element)
        {
            object deserializedObject = null;

            MemoryStream stm = new MemoryStream();
            XmlDictionaryWriter wtr = XmlDictionaryWriter.CreateTextWriter(stm);
            element.WriteContentTo(wtr);
            wtr.Flush();
            stm.Position = 0;

            deserializedObject = serializer.Deserialize(stm);

            return deserializedObject;
        }

        //Saves the persistance data to an xml file.
        void Save(IDictionary<System.Xml.Linq.XName, InstanceValue> instanceData)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<InstanceValues/>");

            foreach (KeyValuePair<System.Xml.Linq.XName, InstanceValue> valPair in instanceData)
            {
                XmlElement newInstance = doc.CreateElement("InstanceValue");

                XmlElement newKey = SerializeObject("key", valPair.Key, doc);
                newInstance.AppendChild(newKey);

                XmlElement newValue = SerializeObject("value", valPair.Value.Value, doc);
                newInstance.AppendChild(newValue);

                doc.DocumentElement.AppendChild(newInstance);
            }
            SaveXMLDocument(this.workflowInstanceId, doc);
        }

        XmlElement SerializeObject(string elementName, object o, XmlDocument doc)
        {
            NetDataContractSerializer s = new NetDataContractSerializer();
            XmlElement newElement = doc.CreateElement(elementName);
            MemoryStream stm = new MemoryStream();

            s.Serialize(stm, o);
            stm.Position = 0;
            StreamReader rdr = new StreamReader(stm);
            newElement.InnerXml = rdr.ReadToEnd();

            return newElement;
        }
    }
}