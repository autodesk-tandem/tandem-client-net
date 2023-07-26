# Read asset properties

This sample demonstrates how to read asset properties using REST API.

To read assets from facility you can use `GetFacilityAssetsAsync` method. The method iterates through all models of the facility, reads assets from each model and returns them as `IEnumerable<Asset>`. It's possible to read asset properties using `Asset.Properties` property. Each property is identified using its id - the id is unique within model. Typically the id short string, i.e. `z:1hw`. To obtain user friendly name you need to use property definition which is part of model schema.

First you need to instantiate `TandemClient` - more details can be found [here](Sample01_Authentication.md).
```cs
var auth = new TwoLeggedAuth("YOUR_CLIENT_ID",
  "YOUR_CLIENT_SECRET",
  "data:read");
var client = new TandemClient(() => auth.GetToken());
```

Then you can create map of schemas:
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

Finaly iterate through properties and display their user friendly name:
```cs
var assets = await client.GetFacilityAssetsAsync(facilityId);

foreach (var asset in assets)
{
  Console.WriteLine(asset.Name);
  var schema = schemaMap[asset.ModelId];

  foreach (var prop in asset.Properties)
  {
    var propDef = schema.Attributes.SingleOrDefault(a => string.Equals(a.Id, prop.Key));

    if (propDef == null)
    {
        continue;
    }
    var name = $"{propDef.Category}.{propDef.Name}";
    Console.WriteLine($"  {name}: {prop.Value}");
  }
}
```

This produces output similar to this:
```
IFS END SUNCTION PUMP
  Common.Rooms: Yc_RhzlHT8W8DQ0DvQAw6QAAAADni_H3M69PtrXz8Eg4wIsJABPHdA
  Higher Education.Device ID: P002
  Mechanical.Operational Status: Non-Operational
  Common.Family Type: 0BWkSA8bQd6yxEVCoZf5xAAk9-U
  Common.System Class: 272
  Common.Assembly Code: D3050
  Common.Classification: 3d
```
