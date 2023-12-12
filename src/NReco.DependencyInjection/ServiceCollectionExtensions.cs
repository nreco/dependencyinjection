using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NReco.DependencyInjection.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NReco.DependencyInjection {

	public static class ServiceCollectionExtensions {

		public static IServiceCollection LoadComponentsFromJsonFile(this IServiceCollection services, string configJsonFile, Action<JsonLoaderOptions> initOptions = null)
			=> LoadComponentsFromJsonFile(services, configJsonFile, null, initOptions);

		public static IServiceCollection LoadComponentsFromJsonFile(this IServiceCollection services, string configJsonFile, string sectionPath, Action<JsonLoaderOptions> initOptions = null) {
			using (var fs = new FileStream(configJsonFile, FileMode.Open, FileAccess.Read)) {
				var opts = new JsonLoaderOptions();
				initOptions?.Invoke(opts);
				var jsonDoc = JsonDocument.Parse(fs, opts.JsonDocOptions);
				var rootEl = jsonDoc.RootElement;
				if (!String.IsNullOrEmpty(sectionPath)) {
					var propNames = sectionPath.Split('.');
					foreach (var propName in propNames) {
						if (rootEl.ValueKind == JsonValueKind.Object) {
							rootEl = rootEl.GetProperty(propName);
							continue;
						}
						throw new JsonException($"Cannot navigate to '{sectionPath}'.");
					}
				}
				var loader = new JsonLoader(opts);
				var components = loader.Load(rootEl);
				RegisterComponents(services, components);
				return services;
			}
		}

		public static IServiceCollection LoadComponentsFromJson(this IServiceCollection services, string configJson, Action<JsonLoaderOptions> initOptions = null) {
			var opts = new JsonLoaderOptions();
			initOptions?.Invoke(opts);
			var jsonDoc = JsonDocument.Parse(configJson, opts.JsonDocOptions);
			var loader = new JsonLoader(opts);
			var components = loader.Load(jsonDoc.RootElement);
			RegisterComponents(services, components);
			return services;
		}



		static void RegisterComponents(IServiceCollection services, IEnumerable<ComponentDescriptor> components) {
			foreach (var c in components) {
				var factory = new ComponentFactory(c);
				Func<IServiceProvider, object, object> create = (srvPrv, key) => {
					return factory.Create(new ServiceProviderComponentFactory(srvPrv), c.ImplementationType);
				};
				var descriptor = ServiceDescriptor.DescribeKeyed(c.ServiceType ?? c.ImplementationType, c.Name, create, c.Lifetime);
				services.Add(descriptor);
			}
		}

		internal class ServiceProviderComponentFactory : IComponentContainer {
			IServiceProvider SrvPrv;

			internal ServiceProviderComponentFactory(IServiceProvider srvPrv) {
				SrvPrv = srvPrv;
			}

			public object GetByName(Type t, string name) => SrvPrv.GetRequiredKeyedService(t, name);

			public object GetByType(Type t) => SrvPrv.GetService(t);

		}

	}

}
