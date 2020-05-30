using System;

namespace LazyMan.ModularLoader.AspNetCore
{
    public class ModuleManifest
    {
        public string ModuleName { get; set; }

        public string ModuleVersion { get; set; }

        public ModuleType ModuleType { get; set; }

        public string ModuleEntrypoint { get; set; }
    }

    public enum ModuleType
    {
        Module,
        FullApp
    }
}
