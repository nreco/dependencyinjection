﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NReco.DependencyInjection;

namespace MvcCoreApp {
    public class Startup
    {
        IWebHostEnvironment HostingEnv;

        public Startup(IWebHostEnvironment env)
        {
			HostingEnv = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => {
                var loggingSection = Configuration.GetSection("Logging");
                loggingBuilder.AddConfiguration(loggingSection);
                loggingBuilder.AddConsole();
            });

            services.AddMvc(options => {
                options.EnableEndpointRouting = false;
            });

			var appSettingsJsonPath = Path.Combine(HostingEnv.ContentRootPath, "appsettings.json");
			services.LoadComponentsFromJsonFile(appSettingsJsonPath, "Components");

		}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Demo}/{action=DemoPage}");
            });
        }
    }
}
