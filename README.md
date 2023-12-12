# NReco.DependencyInjection
Extension to standard .NET DI-container that implements declarative IoC configuration (JSON) and components factory.

NuGet | Tests
--- | --- 
[![NuGet Release](https://img.shields.io/nuget/v/NReco.DependencyInjection.svg)](https://www.nuget.org/packages/NReco.DependencyInjection/) | ![Tests](https://github.com/nreco/dependencyinjection/actions/workflows/dotnet-test.yml/badge.svg) 

* components (services) may be defined via JSON config (can be in appsettings.json or loaded from a separate file)
* you can mix IoC JSON config with classic in-code services definitions
* property injections
* supports mix of explicit / implicit (by-type) injections
* compatible with all modern .NET Core (3.1+) / NET6+ apps

## How to use
```
services.LoadComponentsFromJsonFile("components.json");
```
To load services definitions from 'appsettings.json' section:
```
services.LoadComponentsFromJsonFile("appsettings.json", "Components");
```

## Declarative component JSON definition
```
{
  "Type": "SomeNamespace.ComponentClass,ComponentAssembly",  // required. assembly name may be omitted
  "Name": "c1",  // optional. If set, registered as 'keyed' service
  "Lifetime" : "Singleton",  // Optional. Can be: Transient, Scoped, Singleton (Transient by default)
  "InjectDependencyAttr": false,  // Optional. enable by-type injections for properties marked with [Dependency] (false by default)
  "Constructor": [ /* constructor injections */  ],  // Optional 
  "Properties": {  // Optional. Explicit properties injections
     "PropService": { },  // means: resolve service by property type 
	 "PropStr": "strVal",  // inject simple value
	 "PropRefTo1": {"$ref": "c2"}, // inject keyed service "c2", use property type as service type 
	 "PropRefTo2": {
	   "$ref": "c2",  // inject keyed service "c2" (type="ServiceType")
 	   "ServiceType": "SomeNamespace.ServiceType"
	 },
     "PropArr": [ 1, 2, 3 ],  // inject array of values (or refs to another services)
	 "PropDict": {
	   "KeyStr": "KeyValue"   // inject dictionary (string->object)
	 },
	 "InlineService": {
	   "Type": "SomeNamespace.ComponentClass"  // not registered in 'services' but injections are supported
	 }
  }
}
```

## License
Copyright 2023 Vitaliy Fedorchenko and contributors

Distributed under the MIT license