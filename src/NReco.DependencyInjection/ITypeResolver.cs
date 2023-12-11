using System;
using System.Collections.Generic;
using System.Text;

namespace NReco.DependencyInjection {

	public interface ITypeResolver {

		Type ResolveType(string typeDescription);

		Type ResolveType(string typeDescription, Type contextType);

	}
}
