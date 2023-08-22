# Read type properties

This sample is follow up to previous (example)[Sample02_ReadAssetProperties.md].

In some cases it's necessary to read properties not only from specific element, but also from its type. To obtain type information of an element you need to find its family type (display name = `Common.FamilyType`, id = `l:t`). This returns short key which can be used to obtain type properties.

The workflow is similar to previous example - instantiate `TandemClient` - more details can be found [here](Sample01_Authentication.md).
```cs
var auth = new TwoLeggedAuth("YOUR_CLIENT_ID",
  "YOUR_CLIENT_SECRET",
  "data:read");
var client = new TandemClient(() => auth.GetToken());
```

Then you can create map of schemas. This helps to reduce number of calls to the server:
```cs
var facilityId = "YOUR_FACILITY_URN";
var facility = await client.GetFacilityAsync(facilityId);
var schemaMap = new Dictionary<string, ModelSchema>();

foreach (var link in facility.Links)
{
  var schema = await client.GetModelSchemaAsync(link.ModelId);

  schemaMap[link.ModelId.Replace(Prefixes.Model, string.Empty)] = schema;
}
```

Finaly iterate through elements and display their type name:
```cs
var assets = await client.GetFacilityAssetsAsync(facilityId);

foreach (var asset in assets)
{
  var schema = schemaMap[asset.ModelId];

  Console.WriteLine(asset.Name);
  if (!asset.Properties.TryGetValue(QualifiedColumns.FamilyType, out string? familyType))
  {
    continue;
  }
  var key = Encoding.FromShortKey(familyType, KeyFlags.Logical);
  var assetType = assetTypes.SingleOrDefault(t => string.Equals(t.ModelId, asset.ModelId) && string.Equals(t.Key, key));

  if (assetType == null)
  {
    assetType = await client.GetElementAsync(asset.ModelId, key);
    assetTypes.Add(assetType);
  }
  Console.WriteLine($" Type: {assetType.Name}");
}
```

This produces output similar to this:
```
AirHandler_v03
 Type: ERV-2
```
Note that asset types are also cached so we query asset type only once.
