using System;
using System.Activities;
using System.Activities.DurableInstancing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Runtime.DurableInstancing;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace ConsoleApplication1
{
    public class New4JWorkflowInstanceStore : InstanceStore
    {
        Guid ownerInstanceID;

        public New4JWorkflowInstanceStore(string neo4jConnStr)
            : this(neo4jConnStr, Guid.NewGuid())
        {

        }

        public New4JWorkflowInstanceStore(string neo4jConnStr, Guid id)
        {
            ownerInstanceID = id;
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
            IDictionary<XName, InstanceValue> data = null;

            //The CreateWorkflowOwner command instructs the instance store to create a new instance owner bound to the instanace handle
            if (command is CreateWorkflowOwnerCommand)
            {
                context.BindInstanceOwner(ownerInstanceID, Guid.NewGuid());
            }

            //The SaveWorkflow command instructs the instance store to modify the instance bound to the instance handle or an instance key
            else if (command is SaveWorkflowCommand)
            {
                SaveWorkflowCommand saveCommand = (SaveWorkflowCommand)command;
                data = saveCommand.InstanceData;

                //Save(data);
            }

            //The LoadWorkflow command instructs the instance store to lock and load the instance bound to the identifier in the instance handle
            else if (command is LoadWorkflowCommand)
            {
                //var instance = _workflowInstanceService.GetById(this.ownerInstanceID);
                //data = LoadInstanceData(instance.InstanceData);
                data = new Dictionary<XName, InstanceValue>();

                //load the data into the persistence Context
                context.LoadedInstance(InstanceState.Initialized, data, null, null, null);
            }

            return new CompletedAsyncResult<bool>(true, callback, state);
        }

        protected override bool EndTryCommand(IAsyncResult result)
        {
            return CompletedAsyncResult<bool>.End(result);
        }

        ////Reads data from xml file and creates a dictionary based off of that.
        //IDictionary<XName, InstanceValue> LoadInstanceData(string content)
        //{
        //    var data = new Dictionary<XName, InstanceValue>();

        //    var stringBuilder = new StringBuilder();
        //    var sReader = new StringReader(content);

        //    XmlReader rdr = XmlReader.Create(sReader);
        //    XmlDocument doc = new XmlDocument();
        //    doc.Load(rdr);

        //    XmlNodeList instances = doc.GetElementsByTagName("InstanceValue");
        //    foreach (XmlElement instanceElement in instances)
        //    {
        //        XmlElement keyElement = (XmlElement)instanceElement.SelectSingleNode("descendant::key");
        //        System.Xml.Linq.XName key = (System.Xml.Linq.XName)DeserializeObject(keyElement);

        //        XmlElement valueElement = (XmlElement)instanceElement.SelectSingleNode("descendant::value");
        //        object value = DeserializeObject(valueElement);
        //        InstanceValue instVal = new InstanceValue(value);

        //        data.Add(key, instVal);
        //    }

        //    return data;
        //}

        //object DeserializeObject(XmlElement element)
        //{
        //    object deserializedObject = null;

        //    MemoryStream stm = new MemoryStream();

        //    element.WriteContentTo(wtr);
        //    wtr.Flush();
        //    stm.Position = 0;

        //    deserializedObject = serializer.Deserialize(stm);

        //    return deserializedObject;
        //}

        ////Saves the persistance data to an xml file.
        //void Save(IDictionary<System.Xml.Linq.XName, InstanceValue> instanceData)
        //{
        //    //string fileName = IOHelper.GetFileName(this.ownerInstanceID);
        //    var instance = _workflowInstanceService.GetById(this.ownerInstanceID);
        //    if (instance == null)
        //        instance = _workflowInstanceService.CreateWorkflowInstance(this.ownerInstanceID);

        //    XmlDocument doc = new XmlDocument();
        //    doc.LoadXml("<InstanceValues/>");

        //    foreach (KeyValuePair<System.Xml.Linq.XName, InstanceValue> valPair in instanceData)
        //    {
        //        XmlElement newInstance = doc.CreateElement("InstanceValue");

        //        XmlElement newKey = SerializeObject("key", valPair.Key, doc);
        //        newInstance.AppendChild(newKey);

        //        XmlElement newValue = SerializeObject("value", valPair.Value.Value, doc);
        //        newInstance.AppendChild(newValue);

        //        doc.DocumentElement.AppendChild(newInstance);
        //    }
        //    doc.GetElementsByTagName("WorkflowInstanceId")[0].InnerText = this.ownerInstanceID.ToString();
        //    instance.InstanceData = doc.OuterXml;
        //    _workflowInstanceService.Save(instance);
        //}

        //XmlElement SerializeObject(string elementName, object o, XmlDocument doc)
        //{
        //    NetDataContractSerializer s = new NetDataContractSerializer();
        //    XmlElement newElement = doc.CreateElement(elementName);
        //    MemoryStream stm = new MemoryStream();

        //    s.Serialize(stm, o);
        //    stm.Position = 0;
        //    StreamReader rdr = new StreamReader(stm);
        //    newElement.InnerXml = rdr.ReadToEnd();

        //    return newElement;
        //}

    }
}
