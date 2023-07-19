using Newtonsoft.Json;

namespace Autodesk.Services.Tandem.Request
{
    public class ResetStreamsSecretsRequest
    {
        [JsonProperty("keys")]
        public string[] Keys { get; set; }

        [JsonProperty("hardReset")]
        public bool HardReset { get; set; }
    }
}
