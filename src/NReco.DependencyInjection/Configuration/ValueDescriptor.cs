using System;

namespace NReco.DependencyInjection.Configuration {

	public class ValueDescriptor : IValueDescriptor {

		public object Value { get; private set; }
		
		public ValueDescriptor(object value)
		{
			Value = value;
		}
		
		public object GetValue(IValueFactory factory, Type conversionType) {
			return factory.GetValue(Value, conversionType);
		}
		
	}
}
