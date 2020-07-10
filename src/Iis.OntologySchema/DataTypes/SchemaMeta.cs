using Iis.Interfaces.Meta;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaMeta : ISchemaMeta
    {
        public int? SortOrder { get; set; }
        public bool? ExposeOnApi { get; set; }
        public bool? HasFewEntities { get; set; }
        public string Title { get; set; }
        public bool Multiple { get; set; }
        public string Formula { get; set; }
        public string Format { get; set; }
        public EntityOperation[] AcceptsEntityOperations { get; set; } 
        public EntityOperation[] AcceptsEmbeddedOperations { get; set; }
        public string Type { get; set; }
        public string[] TargetTypes { get; set; }
        public IFormField FormField { get; set; }
        public IContainerMeta Container { get; set; }
        public IValidation Validation { get; set; }
        public IInversedRelationMeta Inversed { get; set; }

        public SchemaMeta(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var jObj = JObject.Parse(json);
            if (jObj.ContainsKey("SortOrder"))
            {
                SortOrder = int.Parse(jObj["SortOrder"].ToString());
            }
            if (jObj.ContainsKey("Multiple"))
            {
                Multiple = bool.Parse(jObj["Multiple"].ToString());
            }
            if (jObj.ContainsKey("ExposeOnApi"))
            {
                ExposeOnApi = bool.Parse(jObj["ExposeOnApi"].ToString());
            }
            if (jObj.ContainsKey("HasFewEntities"))
            {
                HasFewEntities = bool.Parse(jObj["HasFewEntities"].ToString());
            }
            if (jObj.ContainsKey("Title"))
            {
                Title = jObj["Title"].ToString();
            }
            if (jObj.ContainsKey("Type"))
            {
                Type = jObj["Type"].ToString();
            }
            if (jObj.ContainsKey("Formula"))
            {
                Formula = jObj["Formula"].ToString();
            }
            if (jObj.ContainsKey("Format"))
            {
                Format = jObj["Format"].ToString();
            }
            if (jObj.ContainsKey("AcceptsEntityOperations"))
            {
                AcceptsEntityOperations = ((JArray)jObj["AcceptsEntityOperations"])
                    .Select(n => (EntityOperation)byte.Parse(n.ToString())).ToArray();
            }
            if (jObj.ContainsKey("AcceptsEmbeddedOperations"))
            {
                AcceptsEmbeddedOperations = ((JArray)jObj["AcceptsEmbeddedOperations"])
                    .Select(n => (EntityOperation)byte.Parse(n.ToString())).ToArray();
            }
            if (jObj.ContainsKey("TargetTypes"))
            {
                TargetTypes = ((JArray)jObj["TargetTypes"])
                    .Select(n => n.ToString()).ToArray();
            }

            if (jObj.ContainsKey("Inversed"))
            {
                Inversed = ((JObject)jObj["Inversed"]).ToObject<SchemaInversedMeta>();
            }
            if (jObj.ContainsKey("FormField"))
            {
                FormField = ((JObject)jObj["FormField"]).ToObject<SchemaFormField>();
            }
            if (jObj.ContainsKey("Container"))
            {
                Container = ((JObject)jObj["Container"]).ToObject<SchemaContainer>();
            }
            if (jObj.ContainsKey("Validation"))
            {
                Validation = ((JObject)jObj["Validation"]).ToObject<SchemaValidation>();
            }
        }
    }
}
