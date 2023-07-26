using Newtonsoft.Json;

namespace Autodesk.Tandem.Client.Request
{
    public class ScanRequest
    {
        [JsonProperty("families")]
        public string[] Families { get; set; }

        [JsonProperty("keys")]
        public string[] Keys { get; set; }

        [JsonProperty("includeHistory")]
        public bool IncludeHistory { get; set; }

        [JsonProperty("skipArrays")]
        public bool SkipArrays { get; set;  }
    }
}
