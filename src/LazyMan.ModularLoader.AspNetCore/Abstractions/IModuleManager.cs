using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LazyMan.ModularLoader.AspNetCore.Abstractions
{
    public interface IModuleManager
    {
        Assembly LoadModuler(ModuleManifest moduleManifest);


        IEnumerable<ModuleManifest> GetModules();
    }
}
