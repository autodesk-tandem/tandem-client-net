using Newtonsoft.Json;

namespace TandemSDK.Request
{
    public class ResetStreamsSecretsRequest
    {
        [JsonProperty("keys")]
        public string[] Keys { get; set; }

        [JsonProperty("hardReset")]
        public bool HardReset { get; set; }
    }
}
