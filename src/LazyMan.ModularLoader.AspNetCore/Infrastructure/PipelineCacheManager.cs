using LazyMan.ModularLoader.AspNetCore.Abstractions;
using LazyMan.ModularLoader.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Text;

namespace LazyMan.ModularLoader.AspNetCore.Infrastructure
{
    public class PipelineCacheManager : IPipelineCacheManager
    {
        public PipelineCacheManager(IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
        }

        private ConcurrentDictionary<string, (RequestDelegate,IServiceProvider, AssemblyLoadContext)> _cahce = new ConcurrentDictionary<string, (RequestDelegate, IServiceProvider, AssemblyLoadContext)>(StringComparer.OrdinalIgnoreCase);
        public IModuleManager ModuleManager { get; }

        public (RequestDelegate,IServiceProvider, AssemblyLoadContext) GetOrCache(string moduleName)
        {
            
            var modules = ModuleManager.GetLoadedModules();
            var info = modules.Where(i => string.Equals(i.Manifest.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (info == null)
            {
                return default;
            }
            using (info.Alc.EnterContextualReflection())
            {
                if (_cahce.TryGetValue(moduleName, out var v))
                {
                    return v;
                }
                //var cdir = Directory.GetCurrentDirectory();
                //Directory.SetCurrentDirectory(Path.GetDirectoryName(info.Manifest.ModuleEntrypoint));
                var entryType = AssemblyHelper.FindEntrypointType(info.ModuleAssembly);
                if (entryType == null)
                {
                    return default;
                }
                var method = entryType.GetMethod("CreateHostBuilder", 0, new[] { typeof(string[]) });
                if (method == null)
                {
                    return default;
                }
                var startupType = AssemblyHelper.FindStartupType(info.ModuleAssembly);
                if (startupType == null)
                {
                    return default;
                }
                var configureMethod = startupType.GetMethod("Configure");
                if (configureMethod == null)
                {
                    return default;
                }
                var types = configureMethod.GetParameters().Select(x => x.ParameterType)
                    .Where(t => t != typeof(IApplicationBuilder));
                var hostBuilder = method.Invoke(null, new[] { new string[0] }) as IHostBuilder;
                hostBuilder.ConfigureWebHost(wb =>
                {
                    wb.UseContentRoot(Path.GetDirectoryName(info.Manifest.ModuleEntrypoint));
                });
                var host = hostBuilder.Build();
                var hostService = host.Services;
                IServiceProvider service = hostService;
                var instance = ActivatorUtilities.CreateInstance(service, startupType);

                //var configureServicesMethod = startupType.GetMethod("ConfigureServices");
                //if (configureServicesMethod != null)
                //{
                //    var sc = new ServiceCollection();
                //    configureServicesMethod.Invoke(instance, new[] { sc });
                //    var cservice = sc.BuildServiceProvider();
                //    //service = new BothServiceProvider(cservice, hostService);
                //}


                var appBuilder = new ApplicationBuilder(service);
                var parameters = types.Select(t => service.GetRequiredService(t)).ToList();
                parameters.Insert(0, appBuilder);
                configureMethod.Invoke(instance, parameters.ToArray());

                var requestDelegate = appBuilder.Build();
                _cahce.TryAdd(moduleName, (requestDelegate, service,info.Alc));




                //Directory.SetCurrentDirectory(cdir);
                return (requestDelegate, service, info.Alc);
            }
                
        }

        public void RemoveCache(string moduleName)
        {
            _cahce.TryRemove(moduleName, out _);
        }
    }
    
    internal class BothServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider provider1;
        private readonly IServiceProvider provider2;

        public BothServiceProvider(IServiceProvider provider1, IServiceProvider provider2)
        {
            this.provider1 = provider1;
            this.provider2 = provider2;
        }

        public object GetService(Type serviceType)
        {
            object o = null;
            try
            {
                o = this.provider1.GetService(serviceType);
                
            }
            catch
            {

            }
            if (o == null)
            {
                return this.provider2.GetService(serviceType);
            }
            return o;
        }
    }
}
