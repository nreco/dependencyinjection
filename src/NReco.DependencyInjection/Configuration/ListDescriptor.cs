using System;
using System.Collections.Generic;
using System.Linq;

namespace NReco.DependencyInjection.Configuration {

	public class ListDescriptor : IValueDescriptor {
		public IValueDescriptor[] Values;
		bool isOnlyConstValues = false;
		IDictionary<Type,Array> cachedTypedArrays = null;
		
		public ListDescriptor(IValueDescriptor[] values) {
			Values = values;
			isOnlyConstValues = values.All(v => v is ValueDescriptor);
		}
		
		public object GetValue(IValueFactory factory, Type conversionType) {
			// try to find in consts cache
			if (isOnlyConstValues && cachedTypedArrays != null &&
				conversionType.IsArray && cachedTypedArrays.ContainsKey(conversionType.GetElementType())) {
				return cachedTypedArrays[conversionType.GetElementType()].Clone();
			}
			
			// try to create instance of desired type
			Type elemType = typeof(object);
			if (conversionType.IsArray)
				elemType = conversionType.GetElementType();
			Array listArray = Array.CreateInstance(elemType,Values.Length);
			
			for (int i=0; i<Values.Length; i++) {
				listArray.SetValue(Values[i].GetValue( factory, elemType), i );
			}
			
			// store in consts cache
			if (isOnlyConstValues && conversionType.IsArray) {
				if (cachedTypedArrays==null) cachedTypedArrays = new Dictionary<Type,Array>();
				cachedTypedArrays[elemType] = (Array)listArray.Clone();
			}
			if (conversionType.IsArray)
				return listArray; // nothing to convert
			return factory.GetValue(listArray, conversionType);
		}
		
	}
}
