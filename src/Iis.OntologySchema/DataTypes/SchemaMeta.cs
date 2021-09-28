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
        public string Type { get; set; }
        public string[] TargetTypes { get; set; }
        public IFormField FormField { get; set; }
        public IContainerMeta Container { get; set; }
        public IValidation Validation { get; set; }
        public IInversedMeta Inversed { get; set; }
        public bool? IsAggregated { get; set; }
        public bool? IsImportantRelation { get; set; }
        public string Code { get; set; }
        public bool? Hidden { get; set; }
        public bool Editable { get; set; }

        public SchemaMeta() { }
        public SchemaMeta(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var jObj = JObject.Parse(json);
            if (KeyExists(jObj, "SortOrder"))
            {
                SortOrder = int.Parse(jObj["SortOrder"].ToString());
            }
            if (KeyExists(jObj, "Multiple"))
            {
                Multiple = bool.Parse(jObj["Multiple"].ToString());
            }
            if (KeyExists(jObj, "ExposeOnApi"))
            {
                ExposeOnApi = bool.Parse(jObj["ExposeOnApi"].ToString());
            }
            if (KeyExists(jObj, "HasFewEntities"))
            {
                HasFewEntities = bool.Parse(jObj["HasFewEntities"].ToString());
            }
            if (KeyExists(jObj, "IsAggregated"))
            {
                IsAggregated = bool.Parse(jObj["IsAggregated"].ToString());
            }
            if (KeyExists(jObj, "IsImportantRelation"))
            {
                IsImportantRelation = bool.Parse(jObj["IsImportantRelation"].ToString());
            }
            if (KeyExists(jObj, "Title"))
            {
                Title = jObj["Title"].ToString();
            }
            if (KeyExists(jObj, "Type"))
            {
                Type = jObj["Type"].ToString();
            }
            if (KeyExists(jObj, "Formula"))
            {
                Formula = jObj["Formula"].ToString();
            }
            if (KeyExists(jObj, "Format"))
            {
                Format = jObj["Format"].ToString();
            }
            if (KeyExists(jObj, "AcceptsEntityOperations"))
            {
                AcceptsEntityOperations = ((JArray)jObj["AcceptsEntityOperations"])
                    .Select(n => (EntityOperation)byte.Parse(n.ToString())).ToArray();
            }
            if (KeyExists(jObj, "TargetTypes"))
            {
                TargetTypes = ((JArray)jObj["TargetTypes"])
                    .Select(n => n.ToString()).ToArray();
            }

            if (KeyExists(jObj, "Inversed"))
            {
                Inversed = ((JObject)jObj["Inversed"]).ToObject<SchemaInversedMeta>();
            }
            if (KeyExists(jObj, "FormField"))
            {
                FormField = ((JObject)jObj["FormField"]).ToObject<SchemaFormField>();
            }
            if (KeyExists(jObj, "Container"))
            {
                Container = ((JObject)jObj["Container"]).ToObject<SchemaContainer>();
            }
            if (KeyExists(jObj, "Validation"))
            {
                Validation = ((JObject)jObj["Validation"]).ToObject<SchemaValidation>();
            }
            if (KeyExists(jObj, "Hidden"))
            {
                Hidden = bool.Parse(jObj["Hidden"].ToString());
            }
        }
        private bool KeyExists(JObject jObj, string key) => 
            jObj.ContainsKey(key) && jObj[key].Type != JTokenType.Null;
    }
}
