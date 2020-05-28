using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace LazyMan.ModularLoader
{
    public class PluginALC : AssemblyLoadContext
    {
        public PluginALC(string name, IDependencyResolver dependencyResolver)
            : base($"{nameof(LazyMan)}:{name}", true)
        {
            DependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver));
        }
        IDependencyResolver DependencyResolver { get; }
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var path = DependencyResolver.ResolveAssemblyToPath(assemblyName);
            if (path != null)
            {
                return LoadFromAssemblyPath(path);
            }
            return base.Load(assemblyName);
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var path = DependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (path != null)
            {
                return LoadUnmanagedDllFromPath(path);
            }
            return LoadUnmanagedDll(unmanagedDllName);
        }
    }
}
