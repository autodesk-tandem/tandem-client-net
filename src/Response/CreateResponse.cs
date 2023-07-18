using Newtonsoft.Json;

namespace TandemSDK.Response
{
    public class CreateResponse
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }
}
