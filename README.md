# NReco.DependencyInjection
Extension to standard .NET DI-container that implements declarative IoC configuration:

* components (services) may be defined via JSON config (can be in appsettings.json or loaded from a separate file)
* you can mix IoC JSON config with classic in-code services definitions
* property injections
* supports mix of explicit / implicit (by-type) injections
* compatible with all modern .NET Core apps (3.1+)

## How to use

```
services.LoadComponentsFromJson(jsonConfigStr);
```