using Newtonsoft.Json;
using System.Diagnostics;

using TandemSDK.Models;
using TandemSDK.Utils;

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

        public async Task<IEnumerable<Asset>> GetFacilityAssetsAsync(string facilityId)
        {
            var facility = await GetFacilityAsync(facilityId);
            var classification = await GetFacilityClassificationAsync(facilityId);
            var result = new List<Asset>();

            foreach (var link in facility.Links)
            {
                var assets = await GetTaggedAssetsAsync(link.ModelId);

                result.AddRange(assets);
            }
            // resolve  names
            AssignClassificationName(classification, result);
            await AssignRoomNames(result);
            return result;
        }

        public async Task<FacilityClassification> GetFacilityClassificationAsync(string facilityId)
        {
            var token = _getToken();
            var result = await GetAsync<FacilityClassification>(token, $"api/v1/twins/{facilityId}/classification");

            return result;
        }

        public async Task<IEnumerable<Element>> GetFacilityElementsAsync(string facilityId)
        {
            var facility = await GetFacilityAsync(facilityId);
            var classification = await GetFacilityClassificationAsync(facilityId);
            var result = new List<Element>();

            foreach (var link in facility.Links)
            {
                var elements = await GetElementsAsync(link.ModelId);

                result.AddRange(elements);
            }
            // resolve names
            AssignClassificationName(classification, result);
            await AssignRoomNames(result);
            return result;
        }

        public async Task<IEnumerable<Room>> GetFacilityRoomsAsync(string facilityId)
        {
            var facility = await GetFacilityAsync(facilityId);
            var classification = await GetFacilityClassificationAsync(facilityId);
            var result = new List<Room>();

            foreach (var link in facility.Links)
            {
                var rooms = await GetRoomsAsync(link.ModelId);

                result.AddRange(rooms);
            }
            // resolve names
            AssignClassificationName(classification, result);
            return result;
        }


        public async Task<Element[]> GetElementsAsync(string modelId)
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
            var result = new List<Element>();
            var levelMap = new Dictionary<string, int>();
            var count = response?.Items.Length;
            var shortModelId = modelId.Replace(Prefixes.Model, string.Empty);

            for (var i = 0; i < count; i++)
            {
                var item = response?.Items[i];

                if (item == null)
                {
                    continue;
                }
                var room = string.Empty;
                var xroom = string.Empty;
                var assemblyCode = string.Empty;
                var assemblyCodeOverride = string.Empty;
                var classification = string.Empty;
                var classificationOverride = string.Empty;

                foreach (var prop in item.Properties)
                {
                    var propDef = schema.Attributes.SingleOrDefault(a => string.Equals(a.Id, prop.Key));

                    if (propDef == null)
                    {
                        continue;
                    }
                    if (string.Equals(propDef.Id, QualifiedColumns.XRoom))
                    {
                        xroom = prop.Value;
                    }
                    if (string.Equals(propDef.Id, QualifiedColumns.Room))
                    {
                        room = prop.Value;
                    }
                    if (string.Equals(propDef.Id, QualifiedColumns.Classification))
                    {
                        classification = prop.Value;
                    }
                    if (string.Equals(propDef.Id, QualifiedColumns.ClassificationOverride))
                    {
                        classificationOverride = prop.Value;
                    }
                    if (string.Equals(propDef.Id, QualifiedColumns.UniformatClass))
                    {
                        assemblyCode = prop.Value;
                    }
                    if (string.Equals(propDef.Id, QualifiedColumns.UniformatClassOverride))
                    {
                        assemblyCodeOverride = prop.Value;
                    }
                }
                // store index of the level
                if ((item.Flags & ElementFlags.Level) == ElementFlags.Level)
                {
                    levelMap[item.Key] = i;
                }
                var element = new Element(item)
                {
                    ModelId = shortModelId
                };

                if (!string.IsNullOrEmpty(assemblyCode))
                {
                    element.UniformatClassId = assemblyCode;
                }
                if (!string.IsNullOrEmpty(assemblyCodeOverride))
                {
                    element.UniformatClassId = assemblyCodeOverride;
                }
                // resolve assembly code
                if (!string.IsNullOrEmpty(element.UniformatClassId))
                {
                    var code = AssemblyCode.UniformatToAssemblyCode(element.UniformatClassId);

                    element.AssemblyCode = code;
                }
                if (!string.IsNullOrEmpty(classification))
                {
                    element.ClassificationId = classification;
                }
                if (!string.IsNullOrEmpty(classificationOverride))
                {
                    element.ClassificationId = classificationOverride;
                }
                if (!string.IsNullOrEmpty(room))
                {
                    var roomElementKeys = Encoding.FromShortKeyArray(room);

                    for (var j = 0; j < roomElementKeys.Length; j++)
                    {
                        element.Rooms.Add(new RoomRef
                        {
                            ModelId = shortModelId,
                            Key = roomElementKeys[j]
                        });
                    }
                }
                if (!string.IsNullOrEmpty(xroom))
                {
                    var (roomModelIds, roomElementKeys) = Encoding.FromXrefKey(xroom);

                    for (var j = 0; j < roomModelIds.Length; j++)
                    {
                        element.Rooms.Add(new RoomRef
                        {
                            ModelId = roomModelIds[j],
                            Key = roomElementKeys[j]
                        });
                    }
                }
                result.Add(element);
            }
            // resolve level names
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    if (string.IsNullOrEmpty(item.LevelKey))
                    {
                        continue;
                    }
                    if (!levelMap.TryGetValue(item.LevelKey, out int levelIndex))
                    {
                        continue;
                    }
                    var levelDetails = response?.Items[levelIndex];

                    if (levelDetails == null)
                    {
                        continue;
                    }
                    item.Level = levelDetails.Name;
                }
            }
            return result.ToArray();
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
            var items = await GetElementsAsync(modelId);
            var result = new List<Room>();

            foreach (var item in items)
            {
                if (item.Flags != ElementFlags.Room)
                {
                    continue;
                }
                var room = new Room(item);

                result.Add(room);
            }
            return result.ToArray();
        }

        public async Task<Asset[]> GetTaggedAssetsAsync(string modelId)
        {
            var schema = await GetModelSchemaAsync(modelId);
            var elements = await GetElementsAsync(modelId);
            var result = new List<Asset>();

            foreach (var element in elements)
            {
                var userProps = new Dictionary<string, string>();
                int? systemClass = null;
                int? systemClassOverride = null;

                foreach (var prop in element.Properties)
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
                    if (string.Equals(prop.Key, QualifiedColumns.SystemClass))
                    {
                        systemClass = Convert.ToInt32(prop.Value);
                    }
                    if (string.Equals(prop.Key, QualifiedColumns.SystemClassOverride))
                    {
                        systemClassOverride = Convert.ToInt32(prop.Value);
                    }
                }
                if (userProps.Count > 0)
                {
                    var asset = new Asset(element)
                    {
                        AssetProperties = userProps
                    };

                    int? systemClassValue = null;

                    if (systemClass.HasValue)
                    {
                        systemClassValue = systemClass.Value;
                    }
                    if (systemClassOverride.HasValue)
                    {
                        systemClassValue = systemClassOverride.Value;
                    }
                    if (systemClassValue.HasValue)
                    {
                        asset.SystemClass = SystemClass.ToString(systemClassValue.Value);
                    }
                    result.Add(asset);
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

        public async Task<ScanResponse?> ScanAsync(string modelId, string[] families, string[]? keys = null)
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

            if (items == null)
            {
                return null;
            }
            var result = DeserializeScanResponse(items);

            return result;
        }

        private void AssignClassificationName<T>(FacilityClassification classification, IEnumerable<T> items ) where T : IWithClassification
        {
            var classificationMap = new Dictionary<string, string>();

            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.ClassificationId))
                {
                    continue;
                }
                if (classificationMap.TryGetValue(item.ClassificationId, out string? name))
                {
                    item.Classification = name;
                    continue;
                }
                var row = classification.Rows.SingleOrDefault(r => string.Equals(r[0], item.ClassificationId));

                if ((row == null) || (row.Length < 2))
                {
                    continue;
                }
                name = row[1];
                item.Classification = name;
                classificationMap[item.ClassificationId] = name;
;            }
        }

        private async Task AssignRoomNames<T>(IEnumerable<T> items) where T : IWithRooms
        {
            // resolve room names
            var roomMap = new Dictionary<string, int>();
            var roomModelIds = new List<string>();
            var roomElementKeys = new List<string>();

            foreach (var item in items)
            {
                foreach (var room in item.Rooms)
                {
                    var modelId = room.ModelId;
                    var elementKey = room.Key;

                    if (string.IsNullOrEmpty(modelId) || string.IsNullOrEmpty(elementKey))
                    {
                        continue;
                    }
                    var index = roomElementKeys.IndexOf(elementKey);

                    if (index > -1)
                    {
                        if ((index < roomModelIds.Count) && string.Equals(roomModelIds[index], modelId))
                        {
                            continue;
                        }
                    }
                    roomModelIds.Add(modelId);
                    roomElementKeys.Add(elementKey);
                    roomMap[$"{modelId}|{elementKey}"] = roomModelIds.Count - 1;
                }
            }
            var names = await GetRoomNames(roomModelIds.ToArray(), roomElementKeys.ToArray());

            foreach (var item in items)
            {
                foreach (var room in item.Rooms)
                {
                    var key = $"{room.ModelId}|{room.Key}";

                    if (!roomMap.TryGetValue(key, out int index))
                    {
                        continue;
                    }
                    var name = names[index];

                    room.Name = name;
                }
            }
        }

        private async Task<string[]> GetRoomNames(string[] modelIds, string[] elementKeys)
        {
            var roomMap = new Dictionary<string, int>();

            for (var i = 0; i < modelIds.Length; i++)
            {
                roomMap[$"{modelIds[i]}|{elementKeys[i]}"] = i;
            }
            var roomModelIds = modelIds.Distinct();
            var result = new string[modelIds.Length];

            foreach (var roomModelId in roomModelIds)
            {
                var roomKeys = new List<string>();

                for (int i = 0; i < modelIds.Length; i++)
                {
                    var modelId = modelIds[i];
                    var roomKey = elementKeys[i];

                    if (string.Equals(roomModelId, modelId) && !roomKeys.Contains(roomKey))
                    {
                        roomKeys.Add(roomKey);
                    }
                }
                if (roomKeys.Count == 0)
                {
                    continue;
                }
                var roomElements = await ScanAsync(roomModelId,
                    new string[]
                    {
                        ColumnFamilies.Standard
                    }, keys: roomKeys.ToArray());

                if (roomElements == null)
                {
                    continue;
                }
                foreach (var item in roomElements.Items)
                {
                    string key = $"{roomModelId}|{item.Key}";

                    if (!roomMap.TryGetValue(key, out int index))
                    {
                        continue;
                    }
                    result[index] = item.Name;
                }
            }
            return result.ToArray();
        }

        private async Task<T> GetAsync<T>(string token, string endPoint)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, endPoint);

            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            T result = await SendAsync<T>(req);

            return result;
        }

        private async Task<T> PostAsync<T>(string token, string endPoint, object data)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, endPoint);

            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent(JsonConvert.SerializeObject(data), System.Text.Encoding.UTF8, "application/json");
            T result = await SendAsync<T>(req);

            return result;
        }

        private async Task<T> SendAsync<T>(HttpRequestMessage request)
        {
            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"{response.StatusCode}");
            }
            var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();
            var result = serializer.Deserialize<T>(jsonReader);

            return result;
        }

        private static ScanResponse DeserializeScanResponse(object[] items)
        {
            var result = new ScanResponse();
            var modelElements = new List<ElementBase>();
            var revitCategories = GetRevitCategories();

            for (int i = 0; i < items.Length; i++)
            {
                var item = Convert.ToString(items[i]);

                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                if (i == 0)
                {
                    result.Version = item;
                }
                else
                {
                    var items2 = JsonConvert.DeserializeObject<Dictionary<string, string>>(item);

                    if (items2 != null)
                    {
                        var modelElement = new ElementBase();

                        foreach (var el in items2)
                        {
                            if (string.Equals(el.Key, ColumnNames.Key))
                            {
                                modelElement.Key = items2[ColumnNames.Key];
                            }
                            else if (string.Equals(el.Key, QualifiedColumns.ElementFlags))
                            {
                                modelElement.Flags = Convert.ToInt64(el.Value);
                            }
                            else if (string.Equals(el.Key, QualifiedColumns.CategoryId))
                            {
                                modelElement.CategoryId = Convert.ToInt64(el.Value);
                                var id = Convert.ToString(-modelElement.CategoryId - 2000000);

                                if (!string.IsNullOrEmpty(id) && revitCategories.TryGetValue(id, out string? category))
                                {
                                    modelElement.Category = category;
                                }
                            }
                            else if (string.Equals(el.Key, QualifiedColumns.Name))
                            {
                                modelElement.Name = el.Value;
                            }
                            else if (string.Equals(el.Key, QualifiedColumns.Level))
                            {
                                modelElement.LevelKey = Utils.Encoding.FromShortKey(el.Value, ElementFlags.FamilyType);
                            }
                            else
                            {
                                modelElement.Properties.Add(el.Key, el.Value);
                            }
                        }
                        modelElements.Add(modelElement);
                    }
                }
            }
            result.Items = modelElements.ToArray();
            return result;
        }

        private static IDictionary<string, string> GetRevitCategories()
        {
            string categories = System.Text.Encoding.Default.GetString(Resources.cat_id_to_name);

            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(categories);

            return result;
        }
    }
}
