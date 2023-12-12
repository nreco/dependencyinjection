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

		string test1json = @"[ 
				{""Type"":""String"", ""Name"":""testStr""},
				{
				 ""Type"":""NReco.DependencyInjection.Tests.JsonLoaderTests+AComponent"",
				 ""ServiceType"":""NReco.DependencyInjection.Tests.JsonLoaderTests+IMyComponent"",
				 ""Constructor"": [5, ""strVal"", {""$ref"":""refComponentName""}]
                },
				{
				 ""Type"":""NReco.DependencyInjection.Tests.JsonLoaderTests+AComponent"",
				 ""Properties"": {
				  ""PropA"" : {},
				  ""PropB"" : { 
				   ""Type"" : ""System.Collections.Hashtable"",
				   ""Constructor"" : [ { ""A"" : ""AVal"", ""B"" : ""BVal"", ""\\Type"": ""T"" } ]
				  }
				 }
				}
			]";

		[Fact]
		public void Load() {
			var jsLoader = new JsonLoader(new JsonLoaderOptions());

			var jDoc1 = JsonDocument.Parse(test1json);
			var test1 = jsLoader.Load(jDoc1.RootElement);

			Assert.Equal(3, test1.Count);

			Assert.Equal(typeof(string), test1[0].ImplementationType);
			Assert.Equal("testStr", test1[0].Name);

			Assert.Equal(typeof(JsonLoaderTests.AComponent), test1[1].ImplementationType);
			Assert.Equal(typeof(JsonLoaderTests.IMyComponent), test1[1].ServiceType);
			Assert.Equal(3, test1[1].ConstructorArgs.Length);
			Assert.Equal(5M, ((ValueDescriptor)test1[1].ConstructorArgs[0]).Value);
			Assert.Equal("strVal", ((ValueDescriptor)test1[1].ConstructorArgs[1]).Value);
			Assert.Equal("refComponentName", ((RefDescriptor)test1[1].ConstructorArgs[2]).ComponentRef.Name);

			Assert.Equal(2, test1[2].Properties.Length);
			Assert.True(test1[2].Properties[0].Value is RefDescriptor refDescr1 && refDescr1.ComponentRef==null);
			Assert.True(test1[2].Properties[1].Value is RefDescriptor refDescr2
				&& refDescr2.ComponentRef!=null
				&& refDescr2.ComponentRef.ImplementationType == typeof(Hashtable)
				&& refDescr2.ComponentRef.ConstructorArgs.Length==1
				&& refDescr2.ComponentRef.ConstructorArgs[0] is DictionaryDescriptor arg1DictDescr 
				&& arg1DictDescr.Values[2].Key=="Type"
				&& arg1DictDescr.Values[2].Value is ValueDescriptor);
		}

		[Fact]
		public void Load_Defaults() {

			var jsLoader = new JsonLoader(new JsonLoaderOptions());
			var jsonWithDefaults = @"{
				""Defaults"": { ""InjectDependencyAttr"":false, ""Lifetime"": ""Singleton"" },
				""Components"": " + test1json + " }";
			var jDoc1 = JsonDocument.Parse(jsonWithDefaults);
			var test1 = jsLoader.Load(jDoc1.RootElement);

			Assert.False(test1[0].InjectDependencyAttr);
			Assert.Equal(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton, test1[0].Lifetime);
		}

		public interface IMyComponent { }

		public class AComponent : IMyComponent {
			// just a stub, for parse test real constructor/properties are not needed
		}

	}

}
