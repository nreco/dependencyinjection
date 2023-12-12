using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MvcCoreApp {
	
	public class DemoController : Controller {

		ILogger<DemoController> Logger;
		MyService MyService;

		public DemoController(ILogger<DemoController> logger, MyService myService) {
			Logger = logger;
			MyService = myService;
		}

		public IActionResult DemoPage() {
			var res = MyService.Execute();
			return Ok( new { MyServiceResult = res });
		}

	}
}
