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
        public static MaterialLoadData MapLoadData(string loadData)
        {
            var result = new Domain.Materials.MaterialLoadData();
            var json = JObject.Parse(loadData);

            if (json.ContainsKey("from")) result.From = (string)json["from"];
            if (json.ContainsKey("code")) result.Code = (string)json["code"];
            if (json.ContainsKey("coordinates")) result.Coordinates = (string)json["coordinates"];
            if (json.ContainsKey("loadedBy")) result.LoadedBy = (string)json["loadedBy"];
            if (json.ContainsKey("receivingDate")) result.ReceivingDate = (DateTime?)json["receivingDate"];
            if (json.ContainsKey("objects")) result.Objects = json["objects"].Value<JArray>().ToObject<List<string>>();
            if (json.ContainsKey("tags")) result.Tags = json["tags"].Value<JArray>().ToObject<List<string>>();
            if (json.ContainsKey("states")) result.States = json["states"].Value<JArray>().ToObject<List<string>>();

            return result;
        }
    }
}
