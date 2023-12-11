using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NReco.DependencyInjection {

	public class TypeResolver : ITypeResolver {

		Dictionary<string, Type> ResolveTypeCache = new Dictionary<string, Type>();
		string[] DefaultNamespaces;

		public TypeResolver() {
			DefaultNamespaces = new[] { "System" };
		}

		public TypeResolver(params string[] defaultNamespaces) {
			DefaultNamespaces = defaultNamespaces;
		}


		protected int FindBracketClose(string s, int start) {
			int nestedLev = 0;
			int idx = start;
			while (idx < s.Length) {
				switch (s[idx]) {
					case '[':
						nestedLev++;
						break;
					case ']':
						if (nestedLev > 0)
							nestedLev--;
						else
							return idx;
						break;
				}
				idx++;
			}
			return -1;
		}

		public Type ResolveType(string typeDescription, Type contextType) {
			if (typeDescription.IndexOf('.')<0) {
				// name only. Try to locate by implementations / parents of the contextType
				var implementedTypes = contextType.FindInterfaces(new TypeFilter((t, o) => true), null);
				Type matchedType = null;
				foreach (var t in implementedTypes) {
					if (t.Name == typeDescription) {
						if (matchedType == null)
							matchedType = t;
						else
							throw new TypeLoadException($"Ambiguous '{typeDescription}' matches both {matchedType.Name} and {t.Name}.");
					}
				}
				if (matchedType != null)
					return matchedType;
			}
			return ResolveType(typeDescription);
		}

		Type ResolveGenericInstanceType(Type genericType, string genericTypePart) {
			if (String.IsNullOrEmpty(genericTypePart))
				return genericType;
			// lets get generic type by generic type definition
			List<Type> genArgType = new List<Type>();
			// get rid of [ ]
			string genericTypeArgs = genericTypePart.Substring(1, genericTypePart.Length - 2);
			int genParamStartIdx = -1;
			while ((genParamStartIdx = genericTypeArgs.IndexOf('[', genParamStartIdx + 1)) >= 0) {
				int genParamEndIdx = FindBracketClose(genericTypeArgs, genParamStartIdx + 1);
				if (genParamEndIdx < 0)
					throw new Exception("Invalid generic type arguments definition " + genericTypePart);
				string genArgTypeStr = genericTypeArgs.Substring(genParamStartIdx + 1, genParamEndIdx - genParamStartIdx - 1);
				genArgType.Add(ResolveType(genArgTypeStr));
				// skip processed
				genParamStartIdx = genParamEndIdx;
			}
			return genericType.MakeGenericType(genArgType.ToArray());
		}

		public virtual Type ResolveType(string typeDescription) {
			if (ResolveTypeCache.TryGetValue(typeDescription, out var cachedType))
				return cachedType;
			var t = ResolveTypeInternal(typeDescription);
			ResolveTypeCache[typeDescription] = t;
			return t;
		}

		Type ResolveTypeInternal(string typeDescription) {
			const char assemblySeparator = ',';

			var typeName = typeDescription;
			int aposPos = typeName.IndexOf('`');
			bool isGenericType = aposPos >= 0;
			string genericTypePart = String.Empty;

			if (isGenericType) {
				int genericStartArgPos = typeName.IndexOf('[', aposPos);
				if (genericStartArgPos > 0) { /* real generic type, not definition */
					genericTypePart = typeName.Substring(genericStartArgPos, typeName.Length - genericStartArgPos);
					int genericPartEnd = FindBracketClose(genericTypePart, 1);
					genericTypePart = genericTypePart.Substring(0, genericPartEnd + 1);
					// get generic type definition str
					typeName = typeName.Replace(genericTypePart, String.Empty);
				}
			}

			string[] parts = typeName.Split(new char[] { assemblySeparator }, 2);

			if (parts.Length > 1) {
				// assembly name provided
				Assembly assembly;
				try {
					assembly = Assembly.Load(parts[1]);
				} catch (Exception ex) {
					throw new TypeLoadException("Cannot load assembly " + parts[1], ex);
				}
				if (assembly == null)
					throw new TypeLoadException("Cannot load assembly " + parts[1]);

				try {
					Type t = assembly.GetType(parts[0], true, false);
					t = ResolveGenericInstanceType(t, genericTypePart);
					return t;
				} catch (Exception ex) {
					throw new TypeLoadException("Cannot resolve type " + typeName, ex);
				}
			} else {
				int lastDotIndex = typeName.LastIndexOf('.');
				if (lastDotIndex >= 0) {
					// try suggest assembly name by namespace
					try {
						return ResolveType(typeDescription + "," + typeName.Substring(0, lastDotIndex));
					} catch {
						//bad suggestion. 
					}
				}
				// finally, find in all loaded assemblies
				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
					Type type = assembly.GetType(typeName, false);
					if (type==null && lastDotIndex<0) {
						// no namespace in the name - try defaults
						if (DefaultNamespaces != null)
							foreach (var ns in DefaultNamespaces) {
								var suggestedType = assembly.GetType("System." + typeName, false);
								if (type == null)
									type = suggestedType;
								else
									throw new TypeLoadException($"Ambiguous '{typeName}' matches both {type} and {suggestedType}.");
							}
					}
					if (type != null)
						return ResolveGenericInstanceType(type, genericTypePart);
				}

			}

			throw new TypeLoadException("Cannot resolve type " + typeDescription);
		}

	}
}
