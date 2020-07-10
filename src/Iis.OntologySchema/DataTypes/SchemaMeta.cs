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
        public SchemaMeta(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var jObj = JObject.Parse(json);
            if (jObj.ContainsKey("SortOrder"))
            {
                SortOrder = int.Parse(jObj["SortOrder"].ToString());
            }

        }
    }
}
