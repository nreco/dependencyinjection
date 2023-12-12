using System;
using System.ComponentModel;
using System.Collections.Generic;
using Xunit;
using NReco.DependencyInjection;
using NReco.DependencyInjection.Configuration;

namespace NReco.DependencyInjection.Tests
{

	public class ComponentFactoryTests {
		[Fact]
		public void Create() {
			var namedInstances = new Dictionary<string, object>();
			var container = new FakeContainer(namedInstances);

			var c1Factory = new ComponentFactory(new ComponentDescriptor("c1", typeof(EmptyConstructorObj)));
			var c1 = c1Factory.Create(container);
			Assert.True(c1 is EmptyConstructorObj);
			namedInstances["c1"] = c1;

			var c2Factory = new ComponentFactory(
				new ComponentDescriptor("c2", typeof(OneArgConstructorObj)) {
					Properties = new [] { 
						new ClassPropertyDescriptor("Prop", new ValueDescriptor("Test1") ),
						new ClassPropertyDescriptor("IntProp", new ValueDescriptor(5M) )
					}
				});
			var c2 = c2Factory.Create(container);
			Assert.True(c2 is OneArgConstructorObj);
			Assert.True(((OneArgConstructorObj)c2).A is EmptyConstructorObj);
			Assert.Equal("Test1", ((OneArgConstructorObj)c2).Prop);
			Assert.Equal(5, ((OneArgConstructorObj)c2).IntProp);

			var c3Factory = new ComponentFactory(
				new ComponentDescriptor("c3", typeof(PropDepObj)) { InjectDependencyAttr = true } );
			var c3 = c3Factory.Create(container) as PropDepObj;
			Assert.NotNull(c3);
			Assert.NotNull(c3.PropDep);
		}

		public class EmptyConstructorObj {

		}

		public class OneArgConstructorObj {
			public string Prop { get; set; }
			public int IntProp { get; set; }
			public EmptyConstructorObj A;
			public OneArgConstructorObj(EmptyConstructorObj a) {
				A = a;
			}
		}

		public class PropDepObj {
			[Dependency]
			public EmptyConstructorObj PropDep { get; set; }
		}


		public class FakeContainer : IComponentContainer {
			IDictionary<string, object> ByName;

			public FakeContainer(IDictionary<string,object> byName) {
				ByName = byName;
			}

			public object GetByName(Type t, string name) {
				return ByName[name];
			}

			public object GetByType(Type t) {
				foreach (var v in ByName.Values)
					if (t.IsAssignableFrom(v.GetType()))
						return v;
				return null;
			}
		}

	}
}
