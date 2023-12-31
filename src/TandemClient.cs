﻿using Newtonsoft.Json;

using Autodesk.Tandem.Client.Models;
using Autodesk.Tandem.Client.Request;
using Autodesk.Tandem.Client.Response;
using Autodesk.Tandem.Client.Utils;

namespace Autodesk.Tandem.Client
{
    public class TandemClient
    {
        private readonly TandemClientOptions _options;
        private readonly Func<string> _getToken;
        private readonly HttpClient _client;

        public TandemClient(Func<string> tokenCallback)
            : this(tokenCallback, new TandemClientOptions())
        {
        }

        public TandemClient(Func<string> tokenCallback, TandemClientOptions options)
        {
            _options = options;
            _getToken = tokenCallback;
            _client = new HttpClient
            {
                BaseAddress = new Uri(_options.BaseAddress)
            };
        }

        public async Task<CreateResponse> CreateAsync(string modelId, CreateRequest req)
        {
            var token = _getToken();

            var response = await PostAsync<CreateResponse>(token, $"api/v1/modeldata/{modelId}/create", req);

            return response;
        }

        public async Task<string> CreateStreamAsync(string modelId, string name, string uniformatClass, int categoryId,
            string? classification = null,
            string? parentXref = null,
            string? roomXref = null,
            string? levelRef = null)
        {
            var inputs = new List<object[]>
            {
                new object[]
                {
                    MutateActions.Insert,
                    ColumnFamilies.Standard,
                    ColumnNames.Name,
                    name
                },
                new object[]
                {
                    MutateActions.Insert,
                    ColumnFamilies.Standard,
                    ColumnNames.ElementFlags,
                    ElementFlags.Stream
                },
                new object[]
                {
                    MutateActions.Insert,
                    ColumnFamilies.Standard,
                    ColumnNames.UniformatClass,
                    uniformatClass
                },
                new object[]
                {
                    MutateActions.Insert,
                    ColumnFamilies.Standard,
                    ColumnNames.CategoryId,
                    categoryId
                }
            };

            if (!string.IsNullOrEmpty(classification))
            {
                inputs.Add(new object[]
                {
                    MutateActions.Insert,
                    ColumnFamilies.Standard,
                    ColumnNames.Classification,
                    classification
                });
            }
            if (!string.IsNullOrEmpty(parentXref))
            {
                inputs.Add(new object[]
                {
                    MutateActions.Insert,
                    ColumnFamilies.Xrefs,
                    ColumnNames.Parent,
                    parentXref
                });
            }
            if (!string.IsNullOrEmpty(roomXref))
            {
                inputs.Add(new object[]
                {
                    MutateActions.Insert,
                    ColumnFamilies.Xrefs,
                    ColumnNames.Rooms,
                    roomXref
                });
            }
            if (!string.IsNullOrEmpty(levelRef))
            {
                inputs.Add(new object[]
                {
                    MutateActions.Insert,
                    ColumnFamilies.Refs,
                    ColumnNames.Level,
                    levelRef
                });
            }

            var result = await CreateAsync(modelId, new CreateRequest
            {
                Mutations = inputs.ToArray(),
                Description = "Create stream"
            });

            return result.Key;
        }

        public async Task<Facility> GetFacilityAsync(string facilityId)
        {
            var token = _getToken();
            var result = await GetAsync<Facility>(token, $"api/v1/twins/{facilityId}");

            // the id isn't included in response so it's added manually
            result.Id = facilityId;
            // set default model
            var shortFacilityId = facilityId.Replace(Prefixes.Facility, string.Empty);

            foreach (var link in result.Links)
            {
                var shortModelId = link.ModelId.Replace(Prefixes.Model, string.Empty);

                if (string.Equals(shortFacilityId, shortModelId))
                {
                    link.Default = true;
                    break;
                }
            }
            return result;
        }

        public async Task<IEnumerable<Asset>> GetFacilityAssetsAsync(string facilityId, string[]? additionalFamilies = null)
        {
            var facility = await GetFacilityAsync(facilityId);
            var classification = await GetFacilityClassificationAsync(facilityId);
            var result = new List<Asset>();

            foreach (var link in facility.Links)
            {
                var assets = await GetTaggedAssetsAsync(link.ModelId, additionalFamilies);

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

        public async Task<IEnumerable<Element>> GetFacilityElementsAsync(string facilityId, string[]? additionalFamilies = null)
        {
            var facility = await GetFacilityAsync(facilityId);
            var classification = await GetFacilityClassificationAsync(facilityId);
            var result = new List<Element>();

            foreach (var link in facility.Links)
            {
                var elements = await GetElementsAsync(link.ModelId, additionalFamilies);

                result.AddRange(elements);
            }
            // resolve names
            AssignClassificationName(classification, result);
            await AssignRoomNames(result);
            return result;
        }

        public async Task<IEnumerable<Level>> GetFacilityLevelsAsync(string facilityId)
        {
            var facility = await GetFacilityAsync(facilityId);
            var result = new List<Level>();

            foreach (var link in facility.Links)
            {
                var elements = await GetLevelsAsync(link.ModelId);

                result.AddRange(elements);
            }
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

        public async Task<IEnumerable<Models.Stream>> GetFacilityStreamsAsync(string facilityId)
        {
            var model = await GetDefaultModelAsync(facilityId);
            var classification = await GetFacilityClassificationAsync(facilityId);
            var result = new List<Models.Stream>();

            if (model != null)
            {
                var streams = await GetStreamsAsync(model.ModelId);

                result.AddRange(streams);
            }
            await AssignRoomNames(result);
            AssignClassificationName(classification, result);
            return result;
        }

        public async Task<IEnumerable<Models.System>> GetFacilitySystemsAsync(string facilityId)
        {
            var facility = await GetFacilityAsync(facilityId);
            var model = GetDefaultModel(facility);
            var result = new List<Models.System>();

            if (model != null)
            {
                var systems = await GetSystemsAsync(model.ModelId);

                result.AddRange(systems);
            }
            var systemIds = result.Select(s => s.Id);

            foreach (var link in facility.Links)
            {
                if (string.Equals(model?.ModelId, link.ModelId))
                {
                    continue;
                }
                var elements = await GetElementsAsync(link.ModelId, new string[] { ColumnFamilies.Systems });
                var systemElementsMap = GetSystemElementMap(elements);

                foreach (var system in result)
                {
                    if (!string.IsNullOrEmpty(system?.Id) && systemElementsMap.TryGetValue(system.Id, out var elementKeys))
                    {
                        system.ElementCount += elementKeys.Length;
                    }
                }
            }
            return result;
        }

        public async Task<Element> GetElementAsync(string modelId, string elementKey, string[]? additionalFamilies = null)
        {
            var schema = await GetModelSchemaAsync(modelId);
            var families = new List<string>()
            {
                ColumnFamilies.Standard,
                ColumnFamilies.DtProperties,
                ColumnFamilies.Refs,
                ColumnFamilies.Xrefs
            };

            if (additionalFamilies?.Length > 0)
            {
                families.AddRange(additionalFamilies);
            }
            var response = await ScanAsync(modelId, families.ToArray(), new string[] { elementKey });

            if (response == null)
            {
                throw new ApplicationException("Failed to obtain elements");
            }
            var result = ProcessElements(modelId, schema, response.Items);

            if (result.Length != 1)
            {
                throw new ApplicationException("Failed to obtain elements");
            }
            return result[0];
        }

        public async Task<Element[]> GetElementsAsync(string modelId, string[]? additionalFamilies = null, string[]? keys = null)
        {
            var schema = await GetModelSchemaAsync(modelId);
            var families = new List<string>()
            {
                ColumnFamilies.Standard,
                ColumnFamilies.DtProperties,
                ColumnFamilies.Refs,
                ColumnFamilies.Xrefs
            };

            if (additionalFamilies?.Length > 0)
            {
                families.AddRange(additionalFamilies);
            }
            var response = await ScanAsync(modelId, families.ToArray(), keys);

            if (response == null)
            {
                throw new ApplicationException("Failed to obtain elements");
            }
            return ProcessElements(modelId, schema, response.Items);
        }

        public async Task<IDictionary<string, Facility>> GetGroupFacilitiesAsync(string groupId)
        {
            var token = _getToken();
            var result = await GetAsync<IDictionary<string, Facility>>(token, $"api/v1/groups/{groupId}/twins");

            return result;
        }

        public async Task<Group[]> GetGroupsAsync()
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

        public async Task<Level[]> GetLevelsAsync(string modelId)
        {
            var items = await GetElementsAsync(modelId);
            var result = new List<Level>();

            foreach (var item in items)
            {
                if ((item.Flags & ElementFlags.Level) != ElementFlags.Level)
                {
                    continue;
                }
                var level = new Level(item);
                double? elevationValue = null;

                if (item.Properties.TryGetValue(QualifiedColumns.Elevation, out var elevation))
                {
                    elevationValue = Convert.ToDouble(elevation);
                }
                if (item.Properties.TryGetValue(QualifiedColumns.ElevationOverride, out elevation))
                {
                    elevationValue = Convert.ToDouble(elevation);
                }
                if (elevationValue.HasValue)
                {
                    level.Elevation = elevationValue;
                }
                result.Add(level);
            }
            return result.ToArray();
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

        public async Task<Models.Stream[]> GetStreamsAsync(string modelId)
        {
            var items = await GetElementsAsync(modelId);
            var result = new List<Models.Stream>();

            foreach (var item in items)
            {
                if ((item.Flags & ElementFlags.Stream) != ElementFlags.Stream)
                {
                    continue;
                }
                result.Add(new Models.Stream(item));
            }
            return result.ToArray();
        }

        public async Task<Models.System[]> GetSystemsAsync(string modelId)
        {
            var items = await GetElementsAsync(modelId);
            var result = new List<Models.System>();

            foreach (var item in items)
            {
                if ((item.Flags & ElementFlags.System) != ElementFlags.System)
                {
                    continue;
                }
                var system = new Models.System(item);

                system.Id = Encoding.ToSystemId(system.Key);
                int? systemClass = null;
                int? systemClassOverride = null;

                foreach (var prop in item.Properties)
                {
                    if (string.Equals(prop.Key, QualifiedColumns.SystemClass))
                    {
                        systemClass = Convert.ToInt32(prop.Value);
                    }
                    if (string.Equals(prop.Key, QualifiedColumns.SystemClassOverride))
                    {
                        systemClassOverride = Convert.ToInt32(prop.Value);
                    }
                }
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
                    system.SystemClass = SystemClass.ToString(systemClassValue.Value);
                }
                result.Add(system);
            }
            return result.ToArray();
        }

        public async Task<Asset[]> GetTaggedAssetsAsync(string modelId, string[]? additionalFamilies = null)
        {
            var schema = await GetModelSchemaAsync(modelId);
            var elements = await GetElementsAsync(modelId, additionalFamilies);
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
                    var asset = new Asset(element);
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

        public async Task<MutateResponse> MutateAsync(string modelId, MutateRequest req)
        {
            var token = _getToken();

            var response = await PostAsync<MutateResponse>(token, $"api/v1/modeldata/{modelId}/mutate", req);

            return response;
        }

        public async Task ResetStreamsSecretsAsync(string modelId, string[] keys, bool hardReset = false)
        {
            var token = _getToken();

            await PostAsync<object>(token, $"api/v1/models/{modelId}/resetstreamssecrets", new ResetStreamsSecretsRequest
            {
                Keys = keys,
                HardReset = hardReset
            });

            return;
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

        public async Task SendTimeSeriesDataAsync(string modelId, IEnumerable<object> values, string propertyName = "id")
        {
            var token = _getToken();
            var url = $"api/v1/timeseries/models/{modelId}/webhooks/generic?idpath={propertyName}";

            await PostAsync<object>(token, url, values);
        }

        private static void AssignClassificationName<T>(FacilityClassification classification, IEnumerable<T> items ) where T : IWithClassification
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

        private static Facility.Link? GetDefaultModel(Facility facility)
        {
            var shortFacilityId = facility.Id.Replace(Prefixes.Facility, string.Empty);

            foreach (var link in facility.Links)
            {
                var shortModelId = link.ModelId.Replace(Prefixes.Model, string.Empty);

                if (string.Equals(shortFacilityId, shortModelId))
                {
                    return link;
                }
            }
            return null;
        }

        private async Task<Facility.Link?> GetDefaultModelAsync(string facilityId)
        {
            var facility = await GetFacilityAsync(facilityId);

            return GetDefaultModel(facility);
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

        private static IDictionary<string, string[]> GetSystemElementMap(IEnumerable<Element> elements)
        {
            var map = new Dictionary<string, List<string>>();

            foreach (var element in elements)
            {
                var props = element.Properties.Where(p => p.Key.StartsWith($"{ColumnFamilies.Systems}:"));

                foreach (var prop in props)
                {
                    var systemId = prop.Key.Replace($"{ColumnFamilies.Systems}:", string.Empty);

                    if (!map.TryGetValue(systemId, out var list))
                    {
                        list = new List<string>();

                        map[systemId] = list;
                    }
                    list.Add(element.Key);
                }
            }
            var result = new Dictionary<string, string[]>();

            foreach (var item in map)
            {
                result[item.Key] = item.Value.ToArray();
            }
            return result;
        }

        public Element[] ProcessElements(string modelId, ModelSchema schema, ElementBase[] items)
        {
            var result = new List<Element>();
            var levelMap = new Dictionary<string, int>();
            var count = items.Length;
            var shortModelId = modelId.Replace(Prefixes.Model, string.Empty);

            for (var i = 0; i < count; i++)
            {
                var item = items[i];

                if (item == null)
                {
                    continue;
                }
                var room = string.Empty;
                var xroom = string.Empty;
                var parent = string.Empty;
                var xparent = string.Empty;
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
                    if (string.Equals(propDef.Id, QualifiedColumns.Parent))
                    {
                        parent = prop.Value;
                    }
                    if (string.Equals(propDef.Id, QualifiedColumns.XParent))
                    {
                        xparent = prop.Value;
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
                if (!string.IsNullOrEmpty(parent))
                {
                    element.Parent = (element.ModelId, Encoding.FromShortKey(parent, ElementFlags.SimpleElement));
                }
                if (!string.IsNullOrEmpty(xparent))
                {
                    var (parentModelIds, parentKeys) = Encoding.FromXrefKey(xparent);

                    if ((parentModelIds.Length > 0) && (parentKeys.Length > 0))
                    {
                        element.Parent = (parentModelIds[0], parentKeys[0]);
                    }
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
                    var levelDetails = items[levelIndex];

                    if (levelDetails == null)
                    {
                        continue;
                    }
                    item.Level = levelDetails.Name;
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
                        var name = string.Empty;
                        var nameOverride = string.Empty;
                        var level = string.Empty;
                        var levelOverride = string.Empty;

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
                                name = el.Value;
                            }
                            else if (string.Equals(el.Key, QualifiedColumns.NameOverride))
                            {
                                nameOverride = el.Value;
                            }
                            else if (string.Equals(el.Key, QualifiedColumns.Level))
                            {
                                level = Encoding.FromShortKey(el.Value, ElementFlags.FamilyType);
                            }
                            else if (string.Equals(el.Key, QualifiedColumns.LevelOverride))
                            {
                                levelOverride = Encoding.FromShortKey(el.Value, ElementFlags.FamilyType);
                            }
                            else
                            {
                                modelElement.Properties.Add(el.Key, el.Value);
                            }
                        }
                        if (!string.IsNullOrEmpty(name))
                        {
                            modelElement.Name = name;
                        }
                        if (!string.IsNullOrEmpty(nameOverride))
                        {
                            modelElement.Name = nameOverride;
                        }
                        if (!string.IsNullOrEmpty(level))
                        {
                            modelElement.LevelKey = level;
                        }
                        if (!string.IsNullOrEmpty(levelOverride))
                        {
                            modelElement.LevelKey = levelOverride;
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
