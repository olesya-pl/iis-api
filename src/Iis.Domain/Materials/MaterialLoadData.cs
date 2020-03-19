using Iis.Interfaces.Materials;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.Materials
{
    public class MaterialLoadData : IMaterialLoadData
    {
        public string From { get; set; }
        public string LoadedBy { get; set; }
        public string Coordinates { get; set; }
        public string Code { get; set; }
        public DateTime? ReceivingDate { get; set; }
        public IEnumerable<string> Objects { get; set; } = new List<string>();
        public IEnumerable<string> Tags { get; set; } = new List<string>();
        public IEnumerable<string> States { get; set; } = new List<string>();
        public string ToJson()
        {
            var json = new JObject();
            json["from"] = From;
            json["loadedBy"] = LoadedBy;
            json["coordinates"] = Coordinates;
            json["code"] = Code;
            json["receivingDate"] = ReceivingDate;
            json["objects"] = new JArray(Objects);
            json["tags"] = new JArray(Tags);
            json["states"] = new JArray(States);
            return json.ToString();
        }
    }
}
