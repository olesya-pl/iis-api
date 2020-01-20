using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace EmlJsonUploader
{
    public static class Program
    {
        // usage :
        // dotnet EmlJsonUploader.dll 'mutation($input:MaterialInput!){createMaterial(input:$input){id}}' Material,Parent mail.json | curl -X POST -H "Content-Type: application/json" -d @- http://localhost:5000/
        public static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.Error.WriteLine("Usage: dotnet EmlJsonUploader.dll Query ExcludeNodes FilePath");
                return 1;
            }

            var query = args[0];
            var excludeNodes = args[1].Split(',', '|', ';');
            var filePath = args[2];
            var text = File.ReadAllText(filePath);
            var jo = JObject.Parse(text);
            var nodes = (JArray) jo["Metadata"]["Features"]["Nodes"];
            var toRemove = nodes.Where(n => excludeNodes.Contains(n["Relation"].Value<string>())).ToList();
            foreach (var rn in toRemove)
                nodes.Remove(rn);
            foreach (var node in nodes)
                ((JObject)node).Add("Type", "EmailSign");
            ChangePropertiesToLowerCase(jo);
            var result = new JObject();
            result.Add("query", query);
            var variables = new JObject();
            variables.Add("input", jo);
            result.Add("variables", variables);
            Console.Write(result);
            return 0;
        }

        private static void ChangePropertiesToLowerCase(JToken jToken)
        {
            if (jToken.Type == JTokenType.Object)
                ChangePropertiesToLowerCase((JObject) jToken);
            if (jToken.Type == JTokenType.Array)
                ChangePropertiesToLowerCase((JArray) jToken);
        }

        private static void ChangePropertiesToLowerCase(JObject jsonObject)
        {
            foreach (var property in jsonObject.Properties().ToList())
            {
                var token = property.Value;
                ChangePropertiesToLowerCase(token);
                property.Replace(new JProperty(ProcessName(property.Name), token));
            }
        }

        private static void ChangePropertiesToLowerCase(JArray jsonArray)
        {
            foreach (var token in jsonArray)
                ChangePropertiesToLowerCase(token);
        }

        private static string ProcessName(string original) => original.ToLower()[0] + original.Substring(1);
    }
}
