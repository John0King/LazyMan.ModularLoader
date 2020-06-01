using LazyMan.ModularLoader.AspNetCore.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LazyMan.ModularLoader.AspNetCore.Infrastructure
{
    public class FolderBaseModuleManager : IModuleManager
    {
        private readonly ModuleOptions _options;
        private readonly IWebHostEnvironment _hostEnv;

        internal readonly HostLoader HostLoader = new HostLoader();

        public FolderBaseModuleManager(IOptions<ModuleOptions> options, IWebHostEnvironment hostEnv)
        {
            _options = options.Value;
            _hostEnv = hostEnv;
        }

        public Task DisableModuleAsync(string moduleName)
        {
            throw new NotImplementedException();
        }

        public Task EnableModuleAsync(string moduleName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ModuleManifest> GetModules()
        {
            var folder = Path.Combine(_hostEnv.ContentRootPath, _options.AppModuleFolder);

            foreach(var dir in Directory.EnumerateDirectories(folder))
            {
                var file  = Path.Combine(dir, "Module.json");
                if (!File.Exists(file))
                {
                    continue;
                }
                var manifest = JsonSerializer.Deserialize<ModuleManifest>(File.ReadAllText(file));
                if(manifest.ModuleEntrypoint == null)
                {
                    manifest.ModuleEntrypoint = new DirectoryInfo(Path.GetDirectoryName(file)).Name + ".dll";
                }
                if(manifest.ModuleEntrypoint.StartsWith("/") || manifest.ModuleEntrypoint.StartsWith("\\"))
                {
                    manifest.ModuleEntrypoint = manifest.ModuleEntrypoint.Substring(1);
                }
                manifest.ModuleEntrypoint = Path.Combine(Path.GetDirectoryName(file), manifest.ModuleEntrypoint);
                yield return manifest;
            }
        }

        public Task InstallModuleAsync(ModuleInstallOption moduleInstallOption)
        {
            throw new NotImplementedException();
        }

        public Assembly LoadModuler(ModuleManifest moduleManifest)
        {
            if(_hostLoader == null)
            {
                var modules = this.GetModules().Select(m => new PluginInfo
                {
                    PluginName = m.ModuleName,
                    PluginDll = m.ModuleEntrypoint
                });
                _hostLoader = new HostLoader(modules);
                //_hostLoader.AddSharedAssembly(typeof(IWebHost).Assembly,
                //    typeof(Microsoft.Extensions.Hosting.IHostBuilder).Assembly,
                //    typeof(IHost).Assembly);
            }

            return _hostLoader.LoadPlugin(new PluginInfo
            {
                PluginName = moduleManifest.ModuleName,
                PluginDll = moduleManifest.ModuleEntrypoint
            });
        }

        public Task UninstallModuleAsync(string moduleName)
        {
            throw new NotImplementedException();
        }
    }
}
