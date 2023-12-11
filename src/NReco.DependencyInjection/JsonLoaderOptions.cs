using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;


namespace NReco.DependencyInjection {

	public class JsonLoaderOptions {

		public ITypeResolver TypeResolver { get; set; } = new TypeResolver();

		internal void Validate() {
			if (TypeResolver == null)
				throw new ArgumentNullException("TypeResolver");
		}

    }


}
