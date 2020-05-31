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
        /// <summary>
        /// the <see cref="PluginInfo"/>
        /// </summary>
        public PluginInfo PluginInfo { get; }

        /// <summary>
        /// the context for this plugin
        /// </summary>
        PluginContext ALCContext { get; }

        /// <summary>
        /// the dependencyReolver
        /// </summary>
        AssemblyDependencyResolver Resolver { get; }

        /// <inheritdoc />
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            Assembly? a = null;
            // if the assembly is in SharedAssembies then we use that assembly first
            a = ALCContext.SharedAssemblies
                .Where(x=>AssemblyName.ReferenceMatchesDefinition(x.GetName(),assemblyName))
                .FirstOrDefault();
            if (a != null)
            {
                return a;
            }

            // if the assembly is in dependented plugins 
            // then we use the assembly in the assembly in dependented plugin
            // ========= [mark]  =============================
            // should we only use the plugin's entryAssemlby? 
            // or we add [ExportedAssemlby] in PluginInfo
            // or we use some attribute like [ExportAttribute] to detect all it's dependented assembly
            // ===============================================
            foreach (var alc in ALCContext.Plugins)
            {
                a = alc.Value.LoadFromAssemblyName(assemblyName);
                if (a != null)
                {
                    return a;
                }
            }


            // load the dependency from our only
            var path = Resolver.ResolveAssemblyToPath(assemblyName);
            if (path != null)
            {
                return LoadFromAssemblyPath(path);
            }


            // fallback to local folder first

            System.Diagnostics.Debug.WriteLine(assemblyName);

            var file = PluginInfo.PluginFolder + assemblyName.Name + ".dll";
            if (File.Exists(file))
            {
                return LoadFromAssemblyPath(file);
            }

            // fallback to Host AssemlbyLoadContext
            a =  ALCContext.HostLoadContext.LoadFromAssemblyName(assemblyName);
            if(a != null)
            {
                return a;
            }
           

            // fallback to AssemblyLoadContext.Default

            return base.Load(assemblyName);
        }


        /// <inheritdoc/>
        /// <remarks> native ?  do we need any other handle method ?</remarks>
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
