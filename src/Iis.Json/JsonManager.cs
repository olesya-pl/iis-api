using System;
using System.Collections.Generic;
using System.Text;
using Iis.Interfaces.JsonManager;
using Newtonsoft.Json.Linq;

namespace Iis.Json
{
    public class IisJsonManager: IJsonManager
    {
        public Dictionary<string, string> ConvertToKeyValues(string jsonText)
        {
            var jObject = JObject.Parse(jsonText);
            var result = new Dictionary<string, string>();
            foreach (var child in jObject.Children())
            {

            }
            return result;
        }
    }
}
