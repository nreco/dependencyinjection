using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using System.IO;
using NReco.DependencyInjection;
using NReco.DependencyInjection.Configuration;
using System.Text.Json;

namespace NReco.DependencyInjection.Tests {

	public class JsonLoaderTests {

		[Fact]
		public void Load() {
			var jsLoader = new JsonLoader(new JsonLoaderOptions());

			var test1json = @"[ 
				{""Type"":""String"", ""Name"":""testStr""},
				{
				 ""Type"":""NReco.DependencyInjection.Tests.JsonLoaderTests+AComponent"",
				 ""ServiceType"":""NReco.DependencyInjection.Tests.JsonLoaderTests+IMyComponent"",
				 ""Constructor"": [5, ""strVal"", {""$ref"":""refComponentName""}]
                }
			]";
			var jDoc1 = JsonDocument.Parse(test1json);
			var test1 = jsLoader.Load(jDoc1.RootElement);

			Assert.Equal(2, test1.Count);
			Assert.Equal(typeof(string), test1[0].ImplementationType);
			Assert.Equal("testStr", test1[0].Name);

			Assert.Equal(typeof(JsonLoaderTests.AComponent), test1[1].ImplementationType);
			Assert.Equal(typeof(JsonLoaderTests.IMyComponent), test1[1].ServiceType);

		}


		public interface IMyComponent { }

		public class AComponent : IMyComponent {
			TextWriter Output;
			public AComponent(TextWriter output) {
				Output = output;
			}
			public void WriteMessage(string message) {
				Output.WriteLine($"Message: {message}");
			}
		}

	}

}
