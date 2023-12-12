
using System;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Reflection.Emit;

using NReco.DependencyInjection.Configuration;

namespace NReco.DependencyInjection {

	public class ComponentFactory {
		static readonly IComparer<ConstructorInfo> constructorInfoComparer = new ConstructorInfoComparer();

		protected ComponentDescriptor Component { get; private set; }

		public ComponentFactory(ComponentDescriptor component) {
			Component = component;
		}

		public object Create(IComponentContainer container, Type requestedType = null) {
			var instance = CreateInstance(Component, new ContainerValueFactory(container, this));
			if (requestedType != null)
				instance = ConvertTo(instance, requestedType);
			return instance;
		}

		protected virtual object ConvertTo(object o, Type toType) {
			if (o == null)
				return null; // nothing to convert

			if (toType==null || toType==typeof(object) )
				return o; // avoid TypeConvertor 'NotSupportedException'

			// optimization
			if (o != null && (toType == o.GetType() || toType.IsInstanceOfType(o)))
				return o;

			// try component model converters
			var toConverter = TypeDescriptor.GetConverter(toType);
			if (toConverter != null && toConverter.CanConvertFrom(o.GetType()))
				return toConverter.ConvertFrom(o);
			var fromConverter = TypeDescriptor.GetConverter(o.GetType());
			if (fromConverter != null && fromConverter.CanConvertTo(toType))
				return fromConverter.ConvertTo(o, toType);
			throw new InvalidCastException(String.Format("Cannot convert from {0} to {1}", o.GetType(), toType));
		}

		protected virtual object CreateInstance(ComponentDescriptor componentDescriptor, IValueFactory factory) {
			if (componentDescriptor.ImplementationType == null)
				throw new ArgumentNullException("componentDescriptor.ImplementationType", "Cannot create instance: ImplementationType is null.");
			try {
				object instance = null;
				
				var definedArgsCount = componentDescriptor.ConstructorArgs != null ? componentDescriptor.ConstructorArgs.Length : 0;
				// find an appropriate constructor and create instance
				var constructorsArr = componentDescriptor.ImplementationType.GetConstructors();
				IEnumerable<ConstructorInfo> constructors = constructorsArr;
				// order is important if at least one argument is present
				if (constructorsArr.Length > 0 && definedArgsCount > 0) { 
					var constructorsList = new List<ConstructorInfo>();
					foreach (var constructor in constructors) {
						var cArgs = constructor.GetParameters();
						if (cArgs!=null && cArgs.Length == componentDescriptor.ConstructorArgs.Length)
							constructorsList.Add(constructor);
					}
					constructorsList.Sort( constructorInfoComparer );
					constructors = constructorsList.ToArray();
				}

				Exception lastTryException = null;
				foreach (ConstructorInfo constructor in constructors) {
					ParameterInfo[] args = constructor.GetParameters();
					// should be always 'not null'. Just check
					if (args == null)
						throw new NullReferenceException("ConstructorInfo.GetParameters returns null for type = " + componentDescriptor.ImplementationType.ToString() );

					if (definedArgsCount>0 && args.Length<componentDescriptor.ConstructorArgs.Length) continue; // not enough args
						
					// compose constructor arguments
					object[] constructorArgs = new object[args.Length];
					try {
						for (int i = 0; i < constructorArgs.Length; i++) {
							var argValue = definedArgsCount > 0 && i < componentDescriptor.ConstructorArgs.Length ? componentDescriptor.ConstructorArgs[i] : null;
							var argType = args[i].ParameterType;
							constructorArgs[i] = argValue!=null ? argValue.GetValue(factory, argType) : factory.GetComponentInstance(null, argType);
						}
					} catch (Exception ex) {
						lastTryException = ex;
						// try next constructor ...
						continue;
					}
						
					instance = constructor.Invoke( constructorArgs );
					break;
				}
				if (instance == null && lastTryException!=null)
					throw new Exception(
						String.Format("Cannot find contructor for {0} (args={1}) ", componentDescriptor.ImplementationType, definedArgsCount),
						lastTryException);
				
				// instance created?
				if (instance==null)
					throw new MissingMethodException( componentDescriptor.ImplementationType.ToString(), "constructor" );

				// fill properties
				if (componentDescriptor.Properties!=null)
					for (int i=0; i<componentDescriptor.Properties.Length; i++) {
						// find property
						var propertyInitInfo = componentDescriptor.Properties[i];
						try {
							SetObjectProperty(componentDescriptor.ImplementationType, instance, propertyInitInfo.Name, factory, propertyInitInfo.Value);
						} catch(Exception e) {
							throw new Exception(string.Format("Cannot initialize component property: {1}.{0}", propertyInitInfo.Name,componentDescriptor.Name),e);
						}
					}

				// inject properties marked with dependency attr
				if (componentDescriptor.InjectDependencyAttr) {
					var publicProps = componentDescriptor.ImplementationType.GetProperties();
					for (int i = 0; i < publicProps.Length; i++) {
						var p = publicProps[i];
						if (p.IsDefined(typeof(DependencyAttribute), false)) {
							// skip if already injected
							var alreadyInjected = false;
							if (componentDescriptor.Properties!=null)
								for (int j = 0; j < componentDescriptor.Properties.Length; j++)
									if (componentDescriptor.Properties[j].Name == p.Name) {
										alreadyInjected = true;
										break;
									}
							if (!alreadyInjected) {
								var depAttrs = (DependencyAttribute[])p.GetCustomAttributes(typeof(DependencyAttribute), false);
								if (depAttrs.Length > 0) {
									var depAttr = depAttrs[0];
									var depValue = new ValueDescriptor(depAttr.Name != null 
											? factory.GetComponentInstance(new ComponentDescriptor(depAttr.Name, null), p.PropertyType)
											: factory.GetComponentInstance(null, p.PropertyType));
									try {
										SetObjectProperty(componentDescriptor.ImplementationType, instance, p.Name, factory, depValue);
									} catch (Exception e) {
										throw new Exception(string.Format("Cannot initialize component property (marked as dependency): {1}.{0}", p.Name, componentDescriptor.Name), e);
									}
								}
							}
						}
					}
				}
				
				// if init method defined, call it
				if (componentDescriptor.InitMethod!=null) {
					MethodInfo initMethod = componentDescriptor.ImplementationType.GetMethod( componentDescriptor.InitMethod, new Type[0] );
					if (initMethod==null)
						throw new MissingMethodException( componentDescriptor.ImplementationType.ToString(), componentDescriptor.InitMethod );
					initMethod.Invoke( instance, null );
				}
				
				return instance;
			} catch (Exception ex) {
				throw new Exception( String.Format("Cannot create object with type={0} name={1}",
					componentDescriptor.ImplementationType.ToString(), componentDescriptor.Name ), ex);
			}
		}

		void SetObjectProperty(Type t, object o, string propName, IValueFactory factory, IValueDescriptor valueInfo) {
			var propInfo = t.GetProperty(propName);
			if (propInfo == null)
				throw new MissingMethodException(t.ToString(), propName);
			propInfo.SetValue(o, valueInfo.GetValue(factory, propInfo.PropertyType), null);
		}

		internal class ConstructorInfoComparer : IComparer<ConstructorInfo> {
			public int Compare(ConstructorInfo c1, ConstructorInfo c2) {
				ParameterInfo[] c1Params = c1.GetParameters();
				ParameterInfo[] c2Params = c2.GetParameters();
				if (c1Params.Length != c2Params.Length)
					return c1Params.Length.CompareTo(c2Params.Length);
				// lets analyse types
				for (int i = 0; i < c1Params.Length; i++) {
					bool isXObj = c1Params[i].ParameterType==typeof(object);
					bool isYObj = c2Params[i].ParameterType==typeof(object);
					if (isXObj && isYObj) return 0;
					if (isXObj) return 1;
					if (isYObj) return -1;
				}
				return 0;
			}

		}
		
		internal class ContainerValueFactory : IValueFactory {
			internal IComponentContainer Container;
			ComponentFactory Factory;
			internal ContainerValueFactory(IComponentContainer container, ComponentFactory factory) {
				Container = container;
				Factory = factory;
			}
			object IValueFactory.GetValue(object value, Type requiredType) {
				return Factory.ConvertTo(value, requiredType);
			}

			object IValueFactory.GetComponentInstance(ComponentDescriptor componentDescriptor, Type requiredType) {
				object instance;
				if (componentDescriptor == null) {
					instance = Container.GetByType(requiredType);
				} else if (componentDescriptor.Name != null) {
					instance = Container.GetByName(componentDescriptor.ServiceType ?? requiredType, componentDescriptor.Name);
				} else {
					instance = Factory.CreateInstance(componentDescriptor, this);
				}
				if (requiredType == null)
					return instance;
				return Factory.ConvertTo(instance, requiredType);
			}
		}

	}
}
