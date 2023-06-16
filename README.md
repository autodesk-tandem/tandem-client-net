# Autodesk Tandem REST API client for .NET
This is sample implementation of .NET wrapper for Autodesk Tandem REST API.

## Getting started
Create new instance of `TandemClient` class and provide function to obtain authentication token:
```cs
var client = new TandemClient(() => GetToken());
```

Then it provides methods to get data from Tandem database:
```cs
var facility = await client.GetFacilityAsync("urn:adsk.dtt:ZS-qm-sbQWWJnB5tcwBjhQ");

foreach (var item in facility.Links)
{
    Console.WriteLine($"{item.Label}:{item.ModelId}");
}
```
