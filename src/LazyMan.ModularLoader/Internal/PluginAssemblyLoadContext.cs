using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace LazyMan.ModularLoader.Internal
{
    /// <summary>
    /// 插件ALC
    /// </summary>
    public class PluginAssemblyLoadContext : AssemblyLoadContext
    {
        public PluginAssemblyLoadContext(PluginInfo info, PluginContext alcContext)
            : base($"{nameof(LazyMan)}:Plugin:{info.PluginName}", true)
        {
            PluginInfo = info ?? throw new ArgumentNullException(nameof(info));
            ALCContext = alcContext ?? throw new ArgumentNullException(nameof(alcContext));
            try
            {
                // resolver have bug for fileName(seems fixed for windows)
                // the problem is:  AssemblyName and FileName missmatch eg. AssemblyName[Abc] with the FileName[abc.dll]
                Resolver = new AssemblyDependencyResolver(info.PluginDll);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            //using (this.EnterContextualReflection())
            //{
                // if this load fail, then throw
                EntryAssemlby = this.LoadFromAssemblyPath(info.PluginDll);
                //foreach (var asln in EntryAssemlby.GetReferencedAssemblies())
                //{
                //    Console.WriteLine("#########################################");
                //    try
                //    {
                //        Console.WriteLine(asln.Name);
                //        this.LoadFromAssemblyName(asln);
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine("can not load " + asln.Name);
                //        Console.WriteLine(e);
                //    }
                //    Console.WriteLine("#########################################");
                //}
            //}
                
                
        }

        public Assembly EntryAssemlby { get; }

        /// <summary>
        /// the <see cref="PluginInfo"/>
        /// </summary>
        public PluginInfo PluginInfo { get; }

        /// <summary>
        /// the context for this plugin
        /// </summary>
        internal PluginContext ALCContext { get; }

        /// <summary>
        /// the dependencyReolver
        /// </summary>
        private AssemblyDependencyResolver? Resolver { get; }

        /// <inheritdoc />
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"=== [{this.Name}] loading [{ assemblyName.FullName }]====");
            Assembly? a = null;
            // if the assembly is in SharedAssembies then we use that assembly first
            a = ALCContext.SharedAssemblies
                .Where(x => AssemblyName.ReferenceMatchesDefinition(x.GetName(), assemblyName))
                .FirstOrDefault();
            if (a != null)
            {
                Console.WriteLine($"load {assemblyName.Name} from shared");
                Console.ResetColor();
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
                    Console.WriteLine($"load {assemblyName.Name} from alc:{alc.Value.Name}");
                    Console.ResetColor();
                    return a;
                }
            }


            // load the dependency from our only
            var path = Resolver?.ResolveAssemblyToPath(assemblyName);
            if (path != null)
            {
                foreach (var condition in this.ALCContext.Conditions)
                {
                    if (condition(assemblyName))
                    {
                        Console.WriteLine($"load {assemblyName.Name} from hostcontext [{this.ALCContext.HostLoadContext.Name}] by condition \n from {path}");
                        /*return*/ this.ALCContext.HostLoadContext.LoadFromAssemblyPath(path);
                        Console.ResetColor();
                        return null;
                    }
                }
                Console.WriteLine($"load {assemblyName.Name} from locale: {path}");
                return LoadFromAssemblyPath(path);
            }


            // fallback to local folder first

            System.Diagnostics.Debug.WriteLine(assemblyName);

            var file = PluginInfo.PluginFolder + assemblyName.Name + ".dll";
            if (File.Exists(file))
            {
                Console.WriteLine($"load {assemblyName.Name} from local folder:{file}");
                Console.ResetColor();
                return LoadFromAssemblyPath(file);
            }

            // fallback to Host AssemlbyLoadContext
            a = ALCContext.HostLoadContext.LoadFromAssemblyName(assemblyName);
            if (a != null)
            {
                Console.WriteLine($"load {assemblyName.Name} from hostContext[{this.ALCContext.HostLoadContext.Name}]");
                Console.ResetColor();
                return a;
            }


            // fallback to AssemblyLoadContext.Default
            Console.WriteLine($"load {assemblyName.Name} from default");
            Console.ResetColor();
            return base.Load(assemblyName);
        }


        /// <inheritdoc/>
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var path = Resolver?.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (path != null)
            {
                return LoadUnmanagedDllFromPath(path);
            }

            // problem: we  can not access to AssemblyLoadContext.LoadUnManagedDll() method,
            //         we can only access our own , may be the base.LoadUnmanagedDll should 
            //         do this for us.
            if (this.ALCContext.HostLoadContext is PluginAssemblyLoadContext alc)
            {
                var ptr = alc.LoadUnmanagedDll(unmanagedDllName);
                if (ptr != IntPtr.Zero)
                {
                    return ptr;
                }
            }
            return base.LoadUnmanagedDll(unmanagedDllName);
        }
    }
}
