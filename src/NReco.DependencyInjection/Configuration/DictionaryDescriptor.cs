using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NReco.DependencyInjection.Configuration {

	public class DictionaryDescriptor : IValueDescriptor {

		public DictionaryEntryDescriptor[] Values;
		bool isOnlyConstValues = false;
		ReadOnlyDictionary<string,object> cachedConstDictionary = null;
		
		public DictionaryDescriptor(DictionaryEntryDescriptor[] values) {
			Values = values;
			isOnlyConstValues = values.All(v => v.Value is ValueDescriptor);
		}
		
		public object GetValue(IValueFactory factory, Type conversionType) {
			// try to find in cache
			if (isOnlyConstValues && cachedConstDictionary != null) {
				return ConvertTo(cachedConstDictionary, factory, conversionType);
			}

			// try to create instance of desired type
			var dict = new Dictionary<string, object>(Values.Length);
			foreach (var v in Values)
				dict[v.Key] = v.Value.GetValue(factory, typeof(object));
			// cache
			if (isOnlyConstValues) {
				cachedConstDictionary = new ReadOnlyDictionary<string, object>(dict);
				return ConvertTo(cachedConstDictionary, factory, conversionType);
			}
			return ConvertTo( dict, factory, conversionType );
		}
		
		protected object ConvertTo(IDictionary map, IValueFactory factory, Type conversionType) {
			if (conversionType==typeof(Hashtable))
				return new Hashtable(map); // for compatibility
			if (conversionType==typeof(IDictionary))
				return map;
			return factory.GetValue( map, conversionType );
		}
		
	}
	
	public class DictionaryEntryDescriptor {
		public string Key { get; private set; }
		public IValueDescriptor Value { get; private set; }
	
		public DictionaryEntryDescriptor(string key, IValueDescriptor value) {
			Key = key;
			Value = value;
		}
		
		
	}	
	
}
