using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NReco.DependencyInjection.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NReco.DependencyInjection {

	public static class ServiceCollectionExtensions {

		public static IServiceCollection LoadComponentsFromJson(this IServiceCollection services, string configJson) {
			var jsonDoc = JsonDocument.Parse(configJson);
			var opts = new JsonLoaderOptions();
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

			public object GetByName(string name) => SrvPrv.GetKeyedService<object>(name);

			public object GetByType(Type t) => SrvPrv.GetService(t);

		}

	}

}
