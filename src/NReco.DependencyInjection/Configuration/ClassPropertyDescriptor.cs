using System;

namespace NReco.DependencyInjection.Configuration {

	public class ClassPropertyDescriptor {
		public string Name { get; private set; }
		
		public IValueDescriptor Value { get; private set; }
	
		public ClassPropertyDescriptor(string name, IValueDescriptor value) {
			Name = name;
			Value = value;
		}
		
	}
}
