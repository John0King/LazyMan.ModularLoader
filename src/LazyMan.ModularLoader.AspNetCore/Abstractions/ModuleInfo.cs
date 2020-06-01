using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace LazyMan.ModularLoader.AspNetCore.Abstractions
{
    public class ModuleInfo
    {
        public Assembly ModuleAssembly { get; set; }

        public AssemblyLoadContext Alc { get; set; }

        public ModuleManifest Manifest { get; set; }
    }
}
