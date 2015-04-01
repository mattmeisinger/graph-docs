using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GraphDocs.Core.Models;

namespace GraphDocs.Infrastructure.Database.Converters
{
    public class ActiveWorkflowConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Don't use this converter if this object is not a workflow definition. This converter
            // only knows how to handle workflow definitions.
            if (!(value is ActiveWorkflow))
                return;

            var t = JToken.FromObject(value);
            if (t.Type != JTokenType.Object)
            {
                // If this isn't an object (such as 'null'), then pass the parsed content through.
                t.WriteTo(writer);
            }
            else
            {
                var jsonObject = (JObject)t;

                // Get the Settings property off of the object, remove it from the object, then
                // add it back as a Json string property instead of an object, because Neo4J cannot
                // handle properties that are objects.
                var settingsJsonString = jsonObject.Property("Settings").Value.ToString();
                jsonObject.Remove("Settings");
                jsonObject.Add("Settings", settingsJsonString);

                jsonObject.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType != typeof(ActiveWorkflow))
                return null;

            var jsonObject = JObject.Load(reader);

            // Get workflow settings dictionary from the original JSON object
            var settingsJsonString = jsonObject["Settings"].Value<string>();
            var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsJsonString);

            return new ActiveWorkflow
            {
                WorkflowName = jsonObject["WorkflowName"].Value<string>(),
                Order = jsonObject["Order"].Value<int>(),
                Status = jsonObject["Status"].Value<string>(),
                InstanceId = jsonObject["InstanceId"].Value<string>(),
                Bookmark = jsonObject["Bookmark"].Value<string>(),
                Settings = settings
            };
        }

        public override bool CanConvert(Type objectType)
        {
            //Only can convert if it's of the right type
            return objectType == typeof(ActiveWorkflow);
        }
    }
}
