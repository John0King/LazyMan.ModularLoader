using LazyMan.ModularLoader.AspNetCore.Abstractions;
using LazyMan.ModularLoader.AspNetCore.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LazyMan.ModularLoader.AspNetCore.Infrastructure
{
    public class SubAppMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SubAppOptions _option;
        private readonly ILogger<SubAppMiddleware> _logger;


        public SubAppMiddleware(RequestDelegate next, SubAppOptions option, IModuleManager moduleManager, ILogger<SubAppMiddleware> logger)
        {
            _next = next;
            _option = option;
            ModuleManager = moduleManager;
            _logger = logger;
        }

        public IModuleManager ModuleManager { get; }

        public async Task InvokeAsync(HttpContext context)
        {
            if (_option.ModuleManifest == null)
            {
                _logger.LogDebug($"{nameof(ModuleManifest)} is null and skip");
                return;
            }
            IServiceProvider ModuleDefaultService = null;
            var assembly = ModuleManager.LoadModuler(_option.ModuleManifest);


            var entrypointType = AssemblyHelper.FindEntrypointType(assembly);
            if (entrypointType != null)
            {
                _logger.LogDebug($"find entrypoint type: {entrypointType.Name} from {assembly.FullName}");

                if (ModuleManager is FolderBaseModuleManager m)
                {
                    var p = m._hostLoader.HostContext.Plugins[_option.ModuleManifest.ModuleName];
                    using (p.EnterContextualReflection())
                    {
                        var main = entrypointType.GetMethod("Main");
                        Directory.SetCurrentDirectory(Path.GetDirectoryName(_option.ModuleManifest.ModuleEntrypoint));
                        _ = Task.Run(() =>
                        {
                            main.Invoke(null, new[]
                        {
                            new string[]
                            { "--urls", "http://localhost:8090" }
                        });
                        });
                        
                    }
                }

                return;
                var methodKey = "CreateHostBuilder";

                var method = entrypointType.GetMethod(methodKey, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null)
                {
                    _logger.LogDebug($"can not find CreateHostBuilder method from {entrypointType.FullName}");
                    return;
                }
                var hostBuilder = method.Invoke(null, new[] { new string[0] }) as IHostBuilder;
                if (hostBuilder != null)
                {
                    hostBuilder.ConfigureWebHost(wb =>
                    {
                        wb.UseContentRoot(Path.GetDirectoryName(_option.ModuleManifest.ModuleEntrypoint));
                    });
                    //hostBuilder.Properties
                    var host = hostBuilder.Build();
                    ModuleDefaultService = host.Services;
                    foreach (var item in hostBuilder.Properties)
                    {
                        await context.Response.WriteAsync(item.Key.ToString() + ":" + item.Value.ToString());
                    }
                    //await context.Response.WriteAsync(JsonSerializer.Serialize(hostBuilder.Properties));
                }


                //await RunApp();
                return;
            }

            _logger.LogDebug($"can not find entrypoint type from {assembly.FullName}");

            var startupType = AssemblyHelper.FindStartupType(assembly);
            if (startupType != null)
            {
                _logger.LogDebug($"find startup type: {startupType.Name} from {assembly.FullName}");
                await RunApp();
            }
            _logger.LogDebug($"can not find startup type from {assembly.FullName}");

        }

        private Task RunApp()
        {
            throw new NotImplementedException();
        }
    }
}
