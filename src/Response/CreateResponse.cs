using Newtonsoft.Json;

namespace Autodesk.Services.Tandem.Response
{
    public class CreateResponse
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }
}
