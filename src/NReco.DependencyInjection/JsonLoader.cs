using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using NReco.DependencyInjection.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NReco.DependencyInjection {
	
	public class JsonLoader {

		JsonLoaderOptions Options;

		public JsonLoader(JsonLoaderOptions options) {
			Options = options;
		}

		public IList<ComponentDescriptor> Load(JsonElement rootElem) {
			if (rootElem.ValueKind==JsonValueKind.Array) {
				return ParseFromArray(rootElem);
			} else {
				// TBD
				// process as config object that may contain defaults
			}
			throw new JsonException($"Cannot load components from type: {rootElem.ValueKind} (expected: Array).");
		}

		List<ComponentDescriptor> ParseFromArray(JsonElement arrElem) {
			return arrElem.EnumerateArray().Select(elem => ParseComponentDescriptor(elem)).ToList();
		}

		ComponentDescriptor ParseComponentDescriptor(JsonElement elem) {
			if (elem.ValueKind != JsonValueKind.Object)
				throw new JsonException($"Invalid type for component descriptor: {elem.ValueKind} (expected: Object).");
			var implTypeStr = GetJsonPropStringValue(GetJsonObjProp(elem, "Type", true), null);
			var implType = Options.TypeResolver.ResolveType(implTypeStr);
			var name = GetJsonPropStringValue(GetJsonObjProp(elem, "Name", false), null);
			var serviceTypeStr = GetJsonPropStringValue(GetJsonObjProp(elem, "ServiceType", false), null);
			var serviceType = serviceTypeStr != null ? Options.TypeResolver.ResolveType(serviceTypeStr, implType) : null;
			var initMethod = GetJsonPropStringValue(GetJsonObjProp(elem, "InitMethod", false), null);
			var lifetimeStr = GetJsonPropStringValue(GetJsonObjProp(elem, "Lifetime", false), null);
			var lifetime = lifetimeStr != null ? (ServiceLifetime)Enum.Parse(typeof(ServiceLifetime), lifetimeStr, true) : ServiceLifetime.Transient;
			var lazyInitEl = GetJsonObjProp(elem, "LazyInit", false);
			var lazyInit = true;
			if (lazyInitEl.HasValue && (
					lazyInitEl.Value.ValueKind == JsonValueKind.False ||
					(lazyInitEl.Value.ValueKind==JsonValueKind.Number && lazyInitEl.Value.GetDecimal()==0) ))
				lazyInit = false;
			var c = new ComponentDescriptor(name, implType, lifetime, lazyInit);
			if (serviceType != null)
				c.ServiceType = serviceType;
			if (initMethod != null)
				c.InitMethod = initMethod;
			var constructorEl = GetJsonObjProp(elem, "Constructor", false);
			if (constructorEl.HasValue) {
				if (constructorEl.Value.ValueKind != JsonValueKind.Array)
					throw new JsonException($"Incorrect \"Constructor\" value type: {constructorEl.Value.ValueKind} (expected: Array).");
				c.ConstructorArgs = constructorEl.Value.EnumerateArray().Select( el => ParseValueDescriptor(el) ).ToArray();
			}

			var propsEl = GetJsonObjProp(elem, "Properties", false);
			if (propsEl.HasValue) {
				if (propsEl.Value.ValueKind != JsonValueKind.Object)
					throw new JsonException($"Incorrect \"Properties\" value type: {propsEl.Value.ValueKind} (expected: Object).");
				c.Properties = propsEl.Value.EnumerateObject()
					.Select(jsonProp => new ClassPropertyDescriptor(jsonProp.Name, ParseValueDescriptor(jsonProp.Value))).ToArray();
			}

			return c;
		}

		IValueDescriptor ParseValueDescriptor(JsonElement el) {
			switch (el.ValueKind) {
				case JsonValueKind.Null:
					return new ValueDescriptor(null);
				case JsonValueKind.String:
					return new ValueDescriptor(el.GetString());
				case JsonValueKind.Number:
					return new ValueDescriptor(el.GetDecimal());
				case JsonValueKind.True:
					return new ValueDescriptor(true);
				case JsonValueKind.False:
					return new ValueDescriptor(false);
				case JsonValueKind.Array:
					var listValues = el.EnumerateArray().Select(arrEl => ParseValueDescriptor(arrEl)).ToArray();
					return new ListDescriptor(listValues);
				case JsonValueKind.Object:
					var refStr = GetJsonPropStringValue(GetJsonObjProp(el, "$ref", false), null);
					if (!String.IsNullOrEmpty(refStr))
						return new RefDescriptor(new ComponentDescriptor(refStr, null));
					if (el.EnumerateObject().Count() == 0)
						return new RefDescriptor(null); // resolve by type
					if (!GetJsonObjProp(el, "Type", false).HasValue) {
						var dictValues = el.EnumerateObject()
							.Select( oProp => new DictionaryEntryDescriptor(oProp.Name, ParseValueDescriptor(oProp.Value) ) ).ToArray();
						return new DictionaryDescriptor(dictValues);
					}
					return new RefDescriptor( ParseComponentDescriptor(el) );
			}
			throw new JsonException();
		}


		string GetJsonPropStringValue(JsonElement? pEl, string defaultVal) {
			if (pEl.HasValue)
				return pEl.Value.GetString();
			return defaultVal;
		}

		JsonElement? GetJsonObjProp(JsonElement objEl, string propName, bool required = true) {
			if (objEl.TryGetProperty(propName, out var pEl)) {
				return pEl;
			} else {
				var camelCase = Char.ToLowerInvariant(propName[0]) + propName.Substring(1);
				if (objEl.TryGetProperty(camelCase, out var camelCasePropEl))
					return camelCasePropEl;
			}
			if (required)
				throw new Exception($"Property '{propName}' is required.");
			return null;
		}

	}
}
