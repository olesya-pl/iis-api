using Iis.Interfaces.Meta;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaMeta : ISchemaMeta
    {
        public int? SortOrder { get; set; }
        public bool? ExposeOnApi { get; set; }
        public bool? HasFewEntities { get; set; }
        public string Title { get; set; }
        public IFormField FormField { get; set; }
        public IContainerMeta Container { get; set; }
        public bool Multiple { get; set; }
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
            if (jObj.ContainsKey("Inversed"))
            {
                var jInversed = (JObject)jObj["Inversed"];
                Inversed = jInversed.ToObject<SchemaInversedMeta>();

            }

        }
    }
}
