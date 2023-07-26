using Newtonsoft.Json;

namespace Autodesk.Tandem.Client.Request
{
    public class ResetStreamsSecretsRequest
    {
        [JsonProperty("keys")]
        public string[] Keys { get; set; }

        [JsonProperty("hardReset")]
        public bool HardReset { get; set; }
    }
}
