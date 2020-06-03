using LazyMan.ModularLoader.AspNetCore.Abstractions;
using LazyMan.ModularLoader.AspNetCore.Extensions;
using LazyMan.ModularLoader.AspNetCore.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            services.AddSingleton<IPipelineCacheManager, PipelineCacheManager>();
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
            moduleManager.Initialize();
            var pipelineCacheManager = app.ApplicationServices.GetRequiredService<IPipelineCacheManager>();
            var modules = moduleManager.GetModules();
            app.MapWhen(http => http.Request.Path.StartsWithSegments(modulePrefix), subapp =>
                  {
                      foreach(var module in modules)
                      {
                          subapp.Map(modulePrefix.Add("/" + module.ModuleName), subapp2 =>
                           {
                               var (requestDelegate, service, alc) = pipelineCacheManager.GetOrCache(module.ModuleName);
                               if(requestDelegate != null)
                               {
                                   subapp2.UseRouting();
                                   subapp2.UseEndpoints(builder => builder.MapGet("/appinfo", async (http) =>
                                   {
                                       if (http.Request.PathBase.StartsWithSegments(modulePrefix, out var prefixPath))
                                       {
                                           var moduleName = prefixPath.ToString().Remove(0, 1);
                                           http.Response.ContentType = "application/json;charset=utf-8";
                                           await http.Response.WriteAsync(JsonSerializer.Serialize(new
                                           {
                                               pathBase = http.Request.PathBase.ToUriComponent(),
                                               moduleName = moduleName
                                           }));
                                       }

                                   }));
                                   subapp2.Use(next => async http =>
                                   {
                                       var http2 = new DefaultHttpContextFactory(service).Create(http.Features);
                                       using (alc.EnterContextualReflection())
                                       {
                                           await requestDelegate(http2);
                                       }
                                       
                                   });
                               }
                              
                           });
                      }
                      
                  });
        }
    }
}
