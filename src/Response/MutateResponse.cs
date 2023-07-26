using Newtonsoft.Json;

namespace Autodesk.Tandem.Client.Response
{
    public class MutateResponse
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }
}
