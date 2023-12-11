using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using NReco.DependencyInjection;

namespace NReco.DependencyInjection.Tests
{

	public class TypeResolverTests {

		[Fact]
		public void ResolveType() {
			var typeResolver = new TypeResolver();
			Assert.Equal(typeof(String), typeResolver.ResolveType("System.String"));
			Assert.Equal(typeof(String), typeResolver.ResolveType("String"));
			Assert.Equal(typeof(TypeResolverTests), typeResolver.ResolveType("NReco.DependencyInjection.Tests.TypeResolverTests"));
			Assert.Equal(typeof(IServiceProvider), typeResolver.ResolveType("System.IServiceProvider,System.ComponentModel"));
			Assert.Equal(typeof(IDictionary<string,object>), typeResolver.ResolveType("System.Collections.Generic.IDictionary`2[[String][Object]]"));

			Assert.Equal(typeof(IEnumerable), typeResolver.ResolveType("IEnumerable", typeof(ArrayList)));
		}


		public void ResolveType_Ambiguous() {
			var typeResolver = new TypeResolver("System", "NReco.DependencyInjection.Tests.A");
			Assert.Throws<TypeLoadException>( ()=> typeResolver.ResolveType("String") );
			Assert.Throws<TypeLoadException>(() => typeResolver.ResolveType("IEnumerable", typeof(A.ClassWithTwoDifferentEnumerable)));
		}

	}

	namespace A {
		public class String { }

		public interface IEnumerable { }

		public class ClassWithTwoDifferentEnumerable : System.Collections.IEnumerable, A.IEnumerable {
			public IEnumerator GetEnumerator() {
				throw new NotImplementedException();
			}
		}
	}
}
