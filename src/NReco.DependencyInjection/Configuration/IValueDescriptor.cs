using System;

namespace NReco.DependencyInjection.Configuration {

	public interface IValueDescriptor
	{
		object GetValue(IValueFactory factory, Type conversionType);
	}
}
