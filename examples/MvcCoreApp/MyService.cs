using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MvcCoreApp {
	public class MyService {

		public ILogger<MyService> Logger;

		public string Id { get; set; }

		public MyService(ILogger<MyService> logger) {
			Logger = logger;
		}

		public string Execute() {
			Logger.LogInformation($"MyService.Execute (Id={Id})");
			return "Executed service Id=" + Id;
		}

	}
}
