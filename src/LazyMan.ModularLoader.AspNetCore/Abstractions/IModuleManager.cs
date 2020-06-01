using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LazyMan.ModularLoader.AspNetCore.Abstractions
{
    public interface IModuleManager
    {
        void Initialize();
        Assembly LoadModuler(ModuleManifest moduleManifest);


        IEnumerable<ModuleManifest> GetModules();

        Task EnableModuleAsync(string moduleName);

        Task DisableModuleAsync(string moduleName);

        Task InstallModuleAsync(ModuleInstallOption moduleInstallOption);

        Task UninstallModuleAsync(string moduleName);
    }
}
