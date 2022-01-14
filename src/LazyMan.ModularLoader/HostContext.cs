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
        public AssemblyLoadContext HostLoadContext { get; set; } = default!;
        public List<AssemblyLoadContext> DependencyContexts { get; set; } = new List<AssemblyLoadContext>();

        public Dictionary<string, PluginAssemblyLoadContext> Plugins { get; } = new Dictionary<string, PluginAssemblyLoadContext>(StringComparer.OrdinalIgnoreCase);

        public PluginInfoCollection PluginInfos { get; } = new PluginInfoCollection();

        /// <summary>
        /// 一些公用程序集，使其在子ALC里面不加载
        /// </summary>
        public List<Assembly> SharedAssemblies { get; } = new List<Assembly>();

        /// <summary>
        /// 一些条件，控制程序集是否被子Alc加载
        /// </summary>
        public List<Func<AssemblyName, bool>> Conditions { get; } = new List<Func<AssemblyName, bool>>();
    }
}
