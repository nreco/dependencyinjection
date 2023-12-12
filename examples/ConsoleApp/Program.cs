using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NReco.DependencyInjection;

namespace ConsoleApp
{
	class Program {
		static void Main(string[] args) {
			var services = new ServiceCollection();
			services.LoadComponentsFromJsonFile("components.json");
			var srvPrv = services.BuildServiceProvider();

			Console.WriteLine("-- component: c1 --");
			var c1 = srvPrv.GetKeyedService<ComponentA>("c1");
			Console.WriteLine( JsonSerializer.Serialize(c1, new JsonSerializerOptions() { WriteIndented = true }));

			Console.WriteLine("-- component: c2 --");
			var c2 = srvPrv.GetKeyedService<IService>("c2");
			Console.WriteLine(JsonSerializer.Serialize(c2, new JsonSerializerOptions() { WriteIndented = true }));

			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}
	}

	public interface IService {
		string Name { get; }
	} 

	public class ComponentA : IService {

		public string Name { get; set; }

		public string[] StrListDependency { get; set; }

		public IService ServiceFromConstructor { get; private set; }

		public ComponentA() { }

		public ComponentA(IService service) {
			ServiceFromConstructor = service;
		}

	}

}
