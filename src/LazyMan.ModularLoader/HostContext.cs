using LazyMan.ModularLoader.Graph;
using LazyMan.ModularLoader.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;
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


        public List<Assembly> SharedAssemblies { get; } = new List<Assembly>();

        public List<Func<AssemblyName, bool>> Conditions { get; } = new List<Func<AssemblyName, bool>>();
#nullable enable
    }
}
