using LazyMan.ModularLoader.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace LazyMan.ModularLoader
{
    public class PluginContext
    {
        public PluginContext(HostContext context)
        {
            HostContext = context;
        }

        public HostContext HostContext { get; }
        public IEnumerable<PluginInfo> PluginInfos => HostContext.PluginInfos.GetOrderedPlugins();
        public AssemblyLoadContext HostLoadContext => HostContext.HostLoadContext;

        public IEnumerable<Assembly> SharedAssemblies => HostContext.SharedAssemblies;

        public IEnumerable<Func<AssemblyName, bool>> Conditions => HostContext.Conditions;

        public Dictionary<string, PluginAssemblyLoadContext> Plugins { get; } = new Dictionary<string, PluginAssemblyLoadContext>(StringComparer.OrdinalIgnoreCase);
    }
}
