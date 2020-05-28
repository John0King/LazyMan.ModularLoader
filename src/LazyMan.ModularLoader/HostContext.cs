using LazyMan.ModularLoader.Graph;
using LazyMan.ModularLoader.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Text;

namespace LazyMan.ModularLoader
{
    public class HostContext
    {
#nullable disable
        public AssemblyLoadContext HostLoadContext { get; set; }
        public List<AssemblyLoadContext> DependencyContexts { get; set; } = new List<AssemblyLoadContext>();

        public Dictionary<string, PluginAssemblyLoadContext> Plugins { get; } = new Dictionary<string, PluginAssemblyLoadContext>(StringComparer.OrdinalIgnoreCase);

        public PluginInfoCollection PluginInfos { get; } = new PluginInfoCollection();
#nullable enable
    }
}
