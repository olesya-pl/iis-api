using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Api.Controllers.Dto
{
    public class SearchParam
    {
        [JsonProperty("suggestion")]
        public string Suggestion { get; set; }
    }
}
