using Newtonsoft.Json;

namespace Autodesk.Services.Tandem.Response
{
    public class MutateResponse
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }
}
