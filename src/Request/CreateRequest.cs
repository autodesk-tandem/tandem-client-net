using Newtonsoft.Json;

namespace Autodesk.Services.Tandem.Request
{
    public class CreateRequest
    {
        [JsonProperty("muts")]
        public object[][] Mutations { get; set; }

        [JsonProperty("desc")]
        public string Description { get; set; }
    }
}
