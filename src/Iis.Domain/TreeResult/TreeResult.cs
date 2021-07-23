using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.TreeResult
{
    public class TreeResult
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public List<TreeResult> Options { get; set; } = new List<TreeResult>();
        public override string ToString() => Label;
        public JObject GetJsonObject()
        {
            var result = new JObject();
            
            result["label"] = Label;

            if (!string.IsNullOrEmpty(Value))
                result["value"] = Value;

            if (Options.Count > 0)
            {
                var jOptions = new JArray();
                foreach (var option in Options)
                {
                    jOptions.Add(option.GetJsonObject());
                }
                result["options"] = jOptions;
            }

            return result;
        }
    }
}
