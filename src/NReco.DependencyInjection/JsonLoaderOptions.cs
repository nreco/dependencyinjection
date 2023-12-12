using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using NReco.DependencyInjection.Configuration;
using System.Text.Json;

namespace NReco.DependencyInjection {

	public class JsonLoaderOptions {

		public ITypeResolver TypeResolver { get; set; } = new TypeResolver();

		public ComponentDescriptor ComponentDefaults { get; set; } = new ComponentDescriptor();

		public JsonDocumentOptions JsonDocOptions { get; private set; } = new JsonDocumentOptions() { 
			AllowTrailingCommas = true, 
			CommentHandling = JsonCommentHandling.Skip
		};

		internal void Validate() {
			if (TypeResolver == null)
				throw new ArgumentNullException("TypeResolver");
		}

    }


}
