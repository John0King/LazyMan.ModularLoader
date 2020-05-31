using LazyMan.ModularLoader.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace LazyMan.ModularLoader.Internal
{
    public class PluginAssemblyLoadContext : AssemblyLoadContext
    {
        public PluginAssemblyLoadContext(PluginInfo info, PluginContext alcContext)
            : base($"{nameof(LazyMan)}:Plugin:{info.PluginName}", true)
        {
            PluginInfo = info ?? throw new ArgumentNullException(nameof(info));
            ALCContext = alcContext ?? throw new ArgumentNullException(nameof(alcContext));
            Resolver = new AssemblyDependencyResolver(info.PluginDll);
        }

        public PluginInfo PluginInfo { get; }
        PluginContext ALCContext { get; }
        AssemblyDependencyResolver Resolver { get; }
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            Assembly? a = null;
            //a = ALCContext.HostLoadContext.LoadFromAssemblyName(assemblyName);
            a = ALCContext.SharedAssemblies.Where(x=>AssemblyName.ReferenceMatchesDefinition(x.GetName(),assemblyName)).FirstOrDefault();
            if (a != null)
            {
                return a;
            }
            foreach (var alc in ALCContext.Plugins)
            {
                a = alc.Value.LoadFromAssemblyName(assemblyName);
                if (a != null)
                {
                    return a;
                }
            }

            var path = Resolver.ResolveAssemblyToPath(assemblyName);
            if (path != null)
            {
                return LoadFromAssemblyPath(path);
            }

            a =  ALCContext.HostLoadContext.LoadFromAssemblyName(assemblyName);
            if(a != null)
            {
                return a;
            }
           

            // razor issue

            System.Diagnostics.Debug.WriteLine(assemblyName);

            var file = PluginInfo.PluginFolder + assemblyName.Name + ".view.dll";
            if (File.Exists(file))
            {
                return LoadFromAssemblyPath(file);
            }

            return base.Load(assemblyName);
        }


        // native ?  do we need any other handle method ?
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var path = Resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (path != null)
            {
                return LoadUnmanagedDllFromPath(path);
            }
            return LoadUnmanagedDll(unmanagedDllName);
        }
    }
}
