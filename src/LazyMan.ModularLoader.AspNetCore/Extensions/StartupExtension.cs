using LazyMan.ModularLoader.AspNetCore.Abstractions;
using LazyMan.ModularLoader.AspNetCore.Extensions;
using LazyMan.ModularLoader.AspNetCore.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StartupExtension
    {
        public static IServiceCollection AddModulesManager(this IServiceCollection services, Action<ModuleOptions> action = null)
        {
            services = services ?? throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IModuleManager, FolderBaseModuleManager>();
            if(action != null)
            {
                services.Configure<ModuleOptions>(action);
            }
            else
            {
                services.AddOptions<ModuleOptions>();
            }
            return services;
        }

        public static void UseSubAppModules(this IApplicationBuilder app, PathString modulePrefix = default)
        {
            app = app ?? throw new ArgumentNullException();

            //app.Map(modulePrefix, subapp =>
            //{
            //    subapp.UseRouting();
            //    subapp.UseEndpoints(b =>
            //    {
            //        b.MapGet("/", async http =>
            //         {
            //             await http.Response.WriteAsync(http.Request.PathBase);
            //         });
            //    });
            //});
            //return;

            var moduleManager = app.ApplicationServices.GetRequiredService<IModuleManager>();
            var modules = moduleManager.GetModules();
            foreach (var module in modules)
            {
                var path = modulePrefix.Add("/" + module.ModuleName);
                app.Map(path, subapp =>
                {
                    var option = new SubAppOptions
                    {
                        SubAppPrefix = modulePrefix,
                        ModuleManifest = module,
                    };
                    var manager = subapp.ApplicationServices.GetRequiredService<IModuleManager>();
                    var logger = subapp.ApplicationServices.GetRequiredService<ILogger<SubAppMiddleware>>();
                    subapp.UseMiddleware<SubAppMiddleware>(option,manager,logger);
                    subapp.UseRouting();
                    subapp.UseEndpoints(builder => builder.MapGet("/appinfo", async (http) =>
                       {
                           http.Response.ContentType = "application/json;charset=utf-8";
                           await http.Response.WriteAsync(JsonSerializer.Serialize(new 
                           { 
                               pathBase = http.Request.PathBase,
                               moduleName = module.ModuleName
                           }));
                       }));
                });
            }
        }
    }
}
