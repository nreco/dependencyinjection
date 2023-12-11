using System;

namespace NReco.DependencyInjection.Configuration {

	public class ClassTypeDescriptor : IValueDescriptor
	{
		public Type Value;
		
		public ClassTypeDescriptor(Type value)
		{
			Value = value;
		}
		
		public object GetValue(IValueFactory factory, Type conversionType) {
			return Value;
		}
		
	}
}
