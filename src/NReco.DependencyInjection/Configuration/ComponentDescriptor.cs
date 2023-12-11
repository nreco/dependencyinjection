using System;
using Microsoft.Extensions.DependencyInjection;

namespace NReco.DependencyInjection.Configuration {

	public class ComponentDescriptor {

		public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

		public bool LazyInit { get; set; } = true;
	
		public string Name { get; set; }

		public string InitMethod { get; set; }

		public Type ServiceType { get; set; }

		public Type ImplementationType { get; set; }
	
		public IValueDescriptor[] ConstructorArgs { get; set; }
		
		public ClassPropertyDescriptor[] Properties { get; set; }

		public bool InjectDependencyProps { get; set; } = true;

		public ComponentDescriptor() { }

		public ComponentDescriptor(string name, Type t, ServiceLifetime lifetime = ServiceLifetime.Transient, bool lazyInit = true) {
			Name = name;
			ImplementationType = t;
			Lifetime = lifetime;
			LazyInit = lazyInit;
		}


	}
}
