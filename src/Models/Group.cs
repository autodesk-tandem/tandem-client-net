using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.Services.Tandem.Models
{
    public class Group
    {
        public class AccountSettingsDetails
        {

            [JsonProperty("type")]
            public string Type { get; set; }
            
            [JsonProperty("assetLimit")]
            public int AssetLimit { get; set; }
            
            [JsonProperty("streamLimit")]
            public int StreamLimit { get; set; }

            [JsonProperty("expiryDate")]
            public DateTimeOffset ExpiryDate { get; set; }

            [JsonProperty("createDate")]
            public DateTimeOffset CreateDate { get; set; }
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("urn")]
        public string URN { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("accessLevel")]
        public string AccessLevel { get; set; }

        [JsonProperty("twins")]
        public IDictionary<string, string> Twins { get; set; }

        [JsonProperty("accountSettings")]
        public AccountSettingsDetails AccountSettings { get; set; }
    }
}
