using System;
using System.Collections.Generic;
using System.Text;

namespace NReco.DependencyInjection {
	public interface IComponentContainer {
		object GetByType(Type t);
		object GetByName(Type t, string name);
	}
}
