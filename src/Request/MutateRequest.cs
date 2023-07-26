using Newtonsoft.Json;

namespace Autodesk.Tandem.Client.Request
{
    public class MutateRequest
    {
        [JsonProperty("keys")]
        public string[] Keys { get; set; }

        [JsonProperty("muts")]
        public object[][] Mutations { get; set; }

        [JsonProperty("desc")]
        public string Description { get; set; }
    }
}
