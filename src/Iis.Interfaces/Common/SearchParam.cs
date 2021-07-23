using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Common
{
    public class SearchParam
    {
        [JsonProperty("suggestion")]
        public string Suggestion { get; set; }
    }
}
