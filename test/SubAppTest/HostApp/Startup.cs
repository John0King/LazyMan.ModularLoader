using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using LazyMan.ModularLoader.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HostApp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddModulesManager(o=>
            {
                o.AppModuleFolder = System.IO.Path.Combine(AppContext.BaseDirectory, "app-modules");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    context.Response.Redirect("/m");
                    await Task.CompletedTask;
                });
            });
            var dic = new Dictionary<string, int>();
            // razor bug fix
            AssemblyLoadContext.Default.Resolving += (alc, asbn) =>
            {
                return AssemblyLoadContext.All.OfType<PluginAssemblyLoadContext>()
                .Where(x => x.PluginInfo.PluginName.Equals(asbn.Name, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault()?.LoadFromAssemblyName(asbn);
                //if (AssemblyLoadContext.CurrentContextualReflectionContext == null)
                //{
                //    return null;
                //}
                //if (AssemblyLoadContext.CurrentContextualReflectionContext == alc)
                //{
                //    return null;
                //}
                //if (dic.TryGetValue(asbn.FullName, out var c))
                //{
                //    if (c > 0)
                //    {
                //        return null;
                //    }
                //}
                //if (dic.ContainsKey(asbn.FullName))
                //{
                //    dic[asbn.FullName] = dic[asbn.FullName] + 1;
                //}
                //else
                //{
                //    dic[asbn.FullName] = 0;
                //}
                //return AssemblyLoadContext.CurrentContextualReflectionContext.LoadFromAssemblyName(asbn);
            };
            app.UseSubAppModules("/m");

        }
    }
}
