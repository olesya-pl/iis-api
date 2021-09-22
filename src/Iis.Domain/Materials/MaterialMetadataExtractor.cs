using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.Materials
{
    public class MaterialMetadataExtractor
    {
        JObject _metadata;

        public string Channel
        {
            get
            {
                var source = _metadata.GetValue("source", StringComparison.InvariantCultureIgnoreCase)?.Value<string>();
                if (source.StartsWith("cell."))
                {
                    return _metadata.GetValue("unit", StringComparison.InvariantCultureIgnoreCase)?.Value<string>();
                }
                else if (source.StartsWith("sat."))
                {
                    return _metadata.GetValue("channel", StringComparison.InvariantCultureIgnoreCase)?.Value<string>();
                }
                return null;
            }
        }

        public MaterialMetadataExtractor(JObject metadata)
        {
            _metadata = metadata;
        }

        public MaterialMetadataExtractor(string json)
        {
            _metadata = JObject.Parse(json);
        }
    }
}
