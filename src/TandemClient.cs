using Newtonsoft.Json;
using TandemSDK.Models;

namespace TandemSDK
{
    public class TandemClient
    {
        private readonly Func<string> _getToken;
        private readonly HttpClient _client;

        public TandemClient(Func<string> tokenCallback)
        {
            _getToken = tokenCallback;
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://tandem.autodesk.com/")
            };
        }

        public async Task<Facility> GetFacilityAsync(string facilityId)
        {
            var token = _getToken();
            var result = await GetAsync<Facility>(token, $"api/v1/twins/{facilityId}");

            return result;
        }

        public async Task<FacilityClassification> GetFacilityClassificationAsync(string facilityId)
        {
            var token = _getToken();
            var result = await GetAsync<FacilityClassification>(token, $"api/v1/twins/{facilityId}/classification");

            return result;
        }

        public async Task<IDictionary<string, Facility>> GetGroupFacilitiesAsync(string groupId)
        {
            var token = _getToken();
            var result = await GetAsync<IDictionary<string, Facility>>(token, $"api/v1/groups/{groupId}/twins");

            return result;
        }

        public async Task<Models.Group[]> GetGroupsAsync()
        {
            var token = _getToken();
            var result = await GetAsync<Models.Group[]>(token, $"api/v1/groups");

            return result;
        }

        public async Task<FacilityClassification> GetModelRootAsync(string modelId)
        {
            var token = _getToken();
            var result = await GetAsync<FacilityClassification>(token, $"api/v1/modeldata/{modelId}/model");

            return result;
        }

        public async Task<ModelSchema> GetModelSchemaAsync(string modelId)
        {
            var token = _getToken();
            var result = await GetAsync<ModelSchema>(token, $"api/v1/modeldata/{modelId}/schema");

            return result;
        }

        public async Task<Room[]> GetRoomsAsync(string modelId)
        {
            var schema = await GetModelSchemaAsync(modelId);
            var response = await ScanAsync(modelId,
                new string[]
                {
                    ColumnFamilies.Standard,
                    ColumnFamilies.Refs
                });
            var levelAttr = schema.Attributes.SingleOrDefault(a => string.Equals(a.Fam, ColumnFamilies.Refs) && string.Equals(a.Col, ColumnNames.Level));
            var result = new List<Room>();
            var levelKeys = new List<string>();
            var elementToLevelMap = new Dictionary<string, string>();

            foreach (var item in response.Items)
            {
                var room = new Room
                {
                    Key = item.Key,
                    Name = item.Name
                };

                if (item.Flags == ElementFlags.Room)
                {
                    if ((levelAttr != null) && item.Properties.TryGetValue(levelAttr.Id, out string level))
                    {
                        var levelKey = Utils.Encoding.FromShortKey(level, ElementFlags.FamilyType);

                        room.LevelKey = levelKey;
                    }
                    result.Add(room);
                }
            }
            // get level details
            foreach (var room in result)
            {
                var levelDetails = response.Items.SingleOrDefault(i => string.Equals(i.Key, room.LevelKey));

                if ((levelDetails != null) && ((levelDetails.Flags & ElementFlags.Level) == ElementFlags.Level))
                {
                    room.Level = levelDetails.Name;
                }
            }
            return result.ToArray();
        }

        public async Task<Asset[]> GetTaggedAssetsAsync(string modelId)
        {
            var schema = await GetModelSchemaAsync(modelId);
            var response = await ScanAsync(modelId,
                new string[]
                {
                    ColumnFamilies.Standard,
                    ColumnFamilies.DtProperties,
                    ColumnFamilies.Refs,
                    ColumnFamilies.Xrefs
                });
            var levelAttr = schema.Attributes.SingleOrDefault(a => string.Equals(a.Fam, ColumnFamilies.Refs) && string.Equals(a.Col, ColumnNames.Level));
            var levelOverrideAttr = schema.Attributes.SingleOrDefault(a => string.Equals(a.Fam, ColumnFamilies.Refs) && string.Equals(a.Col, ColumnNames.LevelOverride));
            var systemClassAttr = schema.Attributes.SingleOrDefault(a => string.Equals(a.Fam, ColumnFamilies.Standard) && string.Equals(a.Col, ColumnNames.SystemClass));
            var systemClassOverrideAttr = schema.Attributes.SingleOrDefault(a => string.Equals(a.Fam, ColumnFamilies.Standard) && string.Equals(a.Col, ColumnNames.SystemClassOverride));
            var roomAttr = schema.Attributes.SingleOrDefault(a => string.Equals(a.Fam, ColumnFamilies.Xrefs) && string.Equals(a.Col, ColumnNames.Rooms));
            var result = new List<Asset>();

            foreach (var item in response.Items)
            {
                var userProps = new Dictionary<string, string>();
                string level = string.Empty;
                string levelOverride = string.Empty;
                string room = string.Empty;
                int systemClass = int.MinValue;
                int systemClassOverride = int.MinValue;

                foreach (var prop in item.Properties)
                {
                    var propDef = schema.Attributes.SingleOrDefault(a => string.Equals(a.Id, prop.Key));

                    if (propDef == null)
                    {
                        continue;
                    }
                    if (string.Equals(propDef.Fam, ColumnFamilies.DtProperties))
                    {
                        userProps[propDef.Id] = prop.Value;
                    }
                    if (string.Equals(propDef.Id, levelAttr.Id))
                    {
                        level = Utils.Encoding.FromShortKey(prop.Value, ElementFlags.FamilyType);
                    }
                    if (string.Equals(propDef.Id, levelOverrideAttr.Id))
                    {
                        levelOverride = Utils.Encoding.FromShortKey(prop.Value, ElementFlags.FamilyType);
                    }
                    if (string.Equals(propDef.Id, systemClassAttr.Id))
                    {
                        systemClass = Convert.ToInt32(prop.Value);
                    }
                    if (string.Equals(propDef.Id, systemClassOverrideAttr.Id))
                    {
                        systemClassOverride = Convert.ToInt32(prop.Value);
                    }
                    if (string.Equals(propDef.Id, roomAttr.Id))
                    {
                        room = prop.Value;
                    }
                }
                if (userProps.Count > 0)
                {
                    var newAsset = new Asset
                    {
                        Key = item.Key,
                        Name = item.Name,
                        AssetProperties = userProps
                    };
                    var levelKey = string.IsNullOrEmpty(levelOverride) ? level : levelOverride;

                    if (!string.IsNullOrEmpty(levelKey))
                    {
                        newAsset.LevelKey = levelKey;
                    }
                    var systemClassFlags = systemClassOverride == int.MinValue ? systemClass : systemClassOverride;

                    if (systemClassFlags != int.MinValue)
                    {
                        newAsset.SystemClass = Utils.SystemClass.ToString(systemClassFlags);
                    }
                    result.Add(newAsset);
                    // room
                    if (!string.IsNullOrEmpty(room))
                    {
                        newAsset.RoomKey = room;
                    }
                }
            }
            // add levels
            foreach (var asset in result)
            {
                var levelDetails = response.Items.SingleOrDefault(i => string.Equals(i.Key, asset.LevelKey));

                if ((levelDetails != null) && ((levelDetails.Flags & ElementFlags.Level) == ElementFlags.Level))
                {
                    asset.Level = levelDetails.Name;
                }
            }
            // add rooms
            var roomMap = new Dictionary<string, string>();

            foreach (var asset in result)
            {
                if (string.IsNullOrEmpty(asset.RoomKey))
                {
                    continue;
                }
                if (roomMap.TryGetValue(asset.RoomKey, out string room))
                {
                    asset.Room = room;
                }
                else
                {
                    var (roomModelId, roomElementId) = Utils.Encoding.FromXrefKey(asset.RoomKey);

                    var roomElements = await ScanAsync(roomModelId,
                        new string[]
                        {
                            ColumnFamilies.Standard
                        }, keys: new string[] { roomElementId });

                    var roomElement = roomElements.Items.SingleOrDefault(i => string.Equals(i.Key, roomElementId));

                    asset.Room = roomElement.Name;
                    roomMap[asset.RoomKey] = asset.Room;
                }
            }
            return result.ToArray();
        }

        public async Task<IDictionary<string, Facility>> GetUserFacilitiesAsync()
        {
            var token = _getToken();
            var result = await GetAsync<IDictionary<string, Facility>>(token, $"api/v1/users/@me/twins");

            return result;
        }

        public async Task<ScanResponse> ScanAsync(string modelId, string[] families, string[]? keys = null)
        {
            var token = _getToken();
            var req = new ScanRequest
            {
                Families = families,
                IncludeHistory = false,
                SkipArrays = true
            };

            if (keys != null)
            {
                req.Keys = keys;
            }

            var items = await PostAsync<object[]>(token, $"api/v2/modeldata/{modelId}/scan", req);

            var result = DeserializeScanResponse(items);

            return result;
        }

        private static ScanResponse DeserializeScanResponse(object[] items)
        {
            var result = new ScanResponse();
            var modelElements = new List<ScanResponse.Item>();

            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];

                if (i == 0)
                {
                    result.Version = item.ToString();
                }
                else
                {
                    var items2 = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.ToString());
                    var modelElement = new ScanResponse.Item();

                    foreach (var el in items2)
                    {
                        if (string.Equals(el.Key, "k"))
                        {
                            modelElement.Key = items2["k"];
                        }
                        else if (string.Equals(el.Key, QualifiedColumns.ElementFlags))
                        {
                            modelElement.Flags = Convert.ToInt64(el.Value);
                        }
                        else if (string.Equals(el.Key, QualifiedColumns.CategoryId))
                        {
                            modelElement.CategoryId = Convert.ToInt64(el.Value);
                        }
                        else if (string.Equals(el.Key, QualifiedColumns.Name))
                        {
                            modelElement.Name = el.Value;
                        }
                        else
                        {
                            modelElement.Properties.Add(el.Key, el.Value);
                        }
                    }
                    modelElements.Add(modelElement);
                }
            }
            result.Items = modelElements.ToArray();
            return result;
        }

        private async Task<T> GetAsync<T>(string token, string endPoint)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, endPoint);

            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(req);
            var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();
            var result = serializer.Deserialize<T>(jsonReader);

            return result;
        }

        private async Task<T> PostAsync<T>(string token, string endPoint, object data)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, endPoint);

            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent(JsonConvert.SerializeObject(data), System.Text.Encoding.UTF8, "application/json");
            var response = await _client.SendAsync(req);
            var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();
            var result = serializer.Deserialize<T>(jsonReader);

            return result;
        }
    }
}