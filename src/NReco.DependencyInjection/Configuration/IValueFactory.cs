using System;

namespace NReco.DependencyInjection.Configuration {

	public interface IValueFactory
	{
		object GetValue(object value, Type requiredType);
		object GetComponentInstance(ComponentDescriptor componentDescriptor, Type requiredType);
	}
}
