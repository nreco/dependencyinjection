using System;
using Microsoft.Extensions.DependencyInjection;

namespace NReco.DependencyInjection.Configuration {

	public class ComponentDescriptor {

		public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;
	
		public string Name { get; set; }

		public string InitMethod { get; set; }

		public Type ServiceType { get; set; }

		public Type ImplementationType { get; set; }
	
		public IValueDescriptor[] ConstructorArgs { get; set; }
		
		public ClassPropertyDescriptor[] Properties { get; set; }

		public bool InjectDependencyAttr { get; set; } = false;

		public ComponentDescriptor() { }

		public ComponentDescriptor(string name, Type t, ServiceLifetime lifetime = ServiceLifetime.Transient) {
			Name = name;
			ImplementationType = t;
			Lifetime = lifetime;
		}


	}
}
