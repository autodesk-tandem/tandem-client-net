# Autodesk Tandem REST API client for .NET

## Overview
This is sample implementation of .NET wrapper for Autodesk Tandem REST API. It enables you to easily integrate Tandem REST API into your .NET application.

### Requirements
* Visual Studio 2022

### Dependencies
* [Newtonsoft.Json](https://www.newtonsoft.com/json)

## Getting started
The main object for interacting with Autodesk Tandem is `TandemClient`. To use `TandemClient` you need to create new instance and provide funcition to obtain access token:
```cs
var client = new TandemClient(() => GetToken());
```

For more details related to authentication follow [this sample](./samples/Sample01_Authentication.md).

The `TandemClient` object provides methods to get data from Tandem database, for example `GetFacilityAsync` provides details about given facility:
```cs
var facility = await client.GetFacilityAsync("urn:adsk.dtt:ZS-qm-sbQWWJnB5tcwBjhQ");

foreach (var item in facility.Links)
{
    Console.WriteLine($"{item.Label}:{item.ModelId}");
}
```
## Examples
* [Read asset properties](./samples/Sample02_ReadAssetProperties.md)
