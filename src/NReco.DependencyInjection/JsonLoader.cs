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
			if (Options.ComponentDefaults == null)
				Options.ComponentDefaults = new ComponentDescriptor();
			switch (rootElem.ValueKind) {
				case JsonValueKind.Array:
					return ParseFromArray(rootElem);
				case JsonValueKind.Object:
					// process as config object that may contain defaults
					var componentsEl = GetJsonObjProp(rootElem, "Components", true);
					if (componentsEl.Value.ValueKind != JsonValueKind.Array)
						throw new JsonException("Components entry should have an Array type.");
					var defaultsEl = GetJsonObjProp(rootElem, "Defaults", false);
					if (defaultsEl.HasValue) {
						Options.ComponentDefaults = ParseComponentDescriptor(defaultsEl.Value, false);
					}
					return ParseFromArray(componentsEl.Value);
			}
			throw new JsonException($"Cannot load components from type: {rootElem.ValueKind} (expected: Array).");
		}

		List<ComponentDescriptor> ParseFromArray(JsonElement arrElem) {
			return arrElem.EnumerateArray().Select(elem => ParseComponentDescriptor(elem)).ToList();
		}

		ComponentDescriptor ParseComponentDescriptor(JsonElement elem, bool typeRequired = true) {
			if (elem.ValueKind != JsonValueKind.Object)
				throw new JsonException($"Invalid type for component descriptor: {elem.ValueKind} (expected: Object).");
			var implTypeStr = GetJsonPropStringValue(GetJsonObjProp(elem, "Type", typeRequired), null);
			var implType = implTypeStr!=null ? Options.TypeResolver.ResolveType(implTypeStr) : null;
			var name = GetJsonPropStringValue(GetJsonObjProp(elem, "Name", false), null);
			var serviceTypeStr = GetJsonPropStringValue(GetJsonObjProp(elem, "ServiceType", false), null);
			var serviceType = serviceTypeStr != null ? Options.TypeResolver.ResolveType(serviceTypeStr, implType) : null;
			var initMethod = GetJsonPropStringValue(GetJsonObjProp(elem, "InitMethod", false), null);
			var lifetimeStr = GetJsonPropStringValue(GetJsonObjProp(elem, "Lifetime", false), null);
			var lifetime = lifetimeStr != null ? (ServiceLifetime)Enum.Parse(typeof(ServiceLifetime), lifetimeStr, true) : Options.ComponentDefaults.Lifetime;
			var injectDependencyAttrEl = GetJsonObjProp(elem, "InjectDependencyAttr", false);
			var injectDependencyAttr = GetJsonPropBoolValue(injectDependencyAttrEl, Options.ComponentDefaults.InjectDependencyAttr);
			var c = new ComponentDescriptor(name, implType, lifetime);
			c.InjectDependencyAttr = injectDependencyAttr;
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
					if (!String.IsNullOrEmpty(refStr)) {
						var serviceTypeStr = GetJsonPropStringValue(GetJsonObjProp(el, "ServiceType", false), null);
						var serviceType = serviceTypeStr != null ? Options.TypeResolver.ResolveType(serviceTypeStr) : null;
						return new RefDescriptor(new ComponentDescriptor(refStr, null) { ServiceType = serviceType });
					}
					if (el.EnumerateObject().Count() == 0)
						return new RefDescriptor(null); // resolve by type
					if (!GetJsonObjProp(el, "Type", false).HasValue) {
						var dictValues = el.EnumerateObject()
							.Select( oProp => new DictionaryEntryDescriptor(
								HandleEscapedSpecialProps(oProp.Name),
								ParseValueDescriptor(oProp.Value) ) ).ToArray();
						return new DictionaryDescriptor(dictValues);
					}
					return new RefDescriptor( ParseComponentDescriptor(el) );
			}
			throw new JsonException();
		}

		string HandleEscapedSpecialProps(string propName) {
			if (propName == "\\$ref")
				return "$ref";
			if (propName.Length > 0 && propName[0] == '\\' && propName.Equals("\\type", StringComparison.OrdinalIgnoreCase))
				return propName.Substring(1);
			return propName;
		}


		string GetJsonPropStringValue(JsonElement? pEl, string defaultVal) {
			if (pEl.HasValue)
				return pEl.Value.GetString();
			return defaultVal;
		}

		bool GetJsonPropBoolValue(JsonElement? pEl, bool defaultVal) {
			if (pEl.HasValue)
				switch (pEl.Value.ValueKind) {
					case JsonValueKind.True:
						return true;
					case JsonValueKind.False:
						return false;
					case JsonValueKind.Number:
						return pEl.Value.GetDecimal() != 0;
				}
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
