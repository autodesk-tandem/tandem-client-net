using Newtonsoft.Json;

namespace Autodesk.Tandem.Client.Models
{
    public class ModelSchema
    {
        public class Attribute
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("fam")]
            public string Fam { get; set; }

            [JsonProperty("col")]
            public string Col { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("category")]
            public string Category { get; set; }

            [JsonProperty("dataType")]
            public long DataType { get; set; }

            [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
            public long? Flags { get; set; }

            [JsonProperty("dataTypeContext", NullValueHandling = NullValueHandling.Ignore)]
            public string DataTypeContext { get; set; }

            [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
            public string DisplayName { get; set; }

            [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
            public string Description { get; set; }

            [JsonProperty("forgeSpec", NullValueHandling = NullValueHandling.Ignore)]
            public string ForgeSpec { get; set; }

            [JsonProperty("forgeSymbol", NullValueHandling = NullValueHandling.Ignore)]
            public string ForgeSymbol { get; set; }

            [JsonProperty("forgeUnit", NullValueHandling = NullValueHandling.Ignore)]
            public string ForgeUnit { get; set; }

            [JsonProperty("context", NullValueHandling = NullValueHandling.Ignore)]
            public string Context { get; set; }
        }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("attributes")]
        public Attribute[] Attributes { get; set; }
    }
}
