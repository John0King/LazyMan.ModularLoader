using System;
using System.Diagnostics.CodeAnalysis;

namespace LazyMan.ModularLoader.AspNetCore
{
    public class ModuleManifest: IEquatable<ModuleManifest>
    {
        public string ModuleName { get; set; }

        public string ModuleVersion { get; set; }

        public ModuleType ModuleType { get; set; }

        public string ModuleEntrypoint { get; set; }

        public bool Equals([AllowNull] ModuleManifest other)
        {
            if(other == null)
            {
                return false;
            }
            return string.Equals(ModuleName, other.ModuleName, StringComparison.OrdinalIgnoreCase);
        }
    }

    public enum ModuleType
    {
        Module,
        FullApp
    }
}
