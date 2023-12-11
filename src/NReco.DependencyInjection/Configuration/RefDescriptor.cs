using System;

namespace NReco.DependencyInjection.Configuration {

	public class RefDescriptor : IValueDescriptor
	{
		public ComponentDescriptor ComponentRef { get; private set; }
		
		public string ComponentMethod { get; private set; }

		public RefDescriptor(ComponentDescriptor componentRef) : this(componentRef, null) {
		}

		public RefDescriptor(ComponentDescriptor componentRef, string method) {
			ComponentRef = componentRef;
			ComponentMethod = method;
		}
		
		public object GetValue(IValueFactory factory, Type conversionType) {
			if (ComponentMethod!=null) {
				var targetInstance = factory.GetComponentInstance(ComponentRef, typeof(object) );
				var delegFactory = new DelegateFactory(targetInstance, ComponentMethod);
				if (typeof(Delegate).IsAssignableFrom(conversionType)) {
					delegFactory.DelegateType = conversionType;
				}
				return factory.GetValue( 
					delegFactory.GetObject(),
					conversionType
				);
			} else {
				return factory.GetComponentInstance(ComponentRef, conversionType);
			}
		}
	}
}
