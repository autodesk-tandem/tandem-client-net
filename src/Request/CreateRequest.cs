using Newtonsoft.Json;

namespace Autodesk.Tandem.Client.Request
{
    public class CreateRequest
    {
        [JsonProperty("muts")]
        public object[][] Mutations { get; set; }

        [JsonProperty("desc")]
        public string Description { get; set; }
    }
}
