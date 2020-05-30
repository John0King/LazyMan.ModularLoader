using LazyMan.ModularLoader.Graph;
using LazyMan.ModularLoader.Internal;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace LazyMan.ModularLoader
{
    public class HostLoader
    {
        public HostLoader(IEnumerable<PluginInfo> pluginInfos)
        {
            this.HostContext.PluginInfos.AddRange(pluginInfos);
        }

        public HostContext HostContext { get; } = new HostContext
        {
            HostLoadContext = AssemblyLoadContext.CurrentContextualReflectionContext ?? AssemblyLoadContext.Default,
        };

        public void AddSharedAssembly(IEnumerable<Assembly> sharedAssemblies)
        {
            HostContext.SharedAssemblies.AddRange(sharedAssemblies);
        }

        public void AddSharedAssembly(Assembly sharedAssembly)
        {
            HostContext.SharedAssemblies.Add(sharedAssembly);
        }

        public void AddSharedAssembly(params Assembly[] assemblies)
        {
            this.AddSharedAssembly(assemblies as IEnumerable<Assembly>);
        }

        /// <summary>
        /// 加载新的插件
        /// </summary>
        /// <param name="info"></param>
        public virtual Assembly LoadPlugin(PluginInfo info)
        {
            
            this.HostContext.PluginInfos.Add(info);
            var context = new PluginContext(this.HostContext);
            foreach (var d in info.DependedPlugins)
            {
                if (this.HostContext.Plugins.TryGetValue(d, out var alc))
                {
                    context.Plugins[d] = alc;
                }
            }
            var pluginAlc = new PluginAssemblyLoadContext(info, context);
            this.HostContext.Plugins[info.PluginName] = pluginAlc;
            return pluginAlc.LoadFromAssemblyPath(info.PluginDll);

        }

        /// <summary>
        /// 从已有的列表中加载插件
        /// </summary>
        public virtual IEnumerable<Assembly> LoadPlugins()
        {
            var result = new List<Assembly>();
            // load as the right order
            var infos = this.HostContext.PluginInfos.GetOrderedPlugins();
            foreach (var info in infos)
            {
                var context = new PluginContext(this.HostContext);
                foreach(var d in info.DependedPlugins)
                {
                    if(this.HostContext.Plugins.TryGetValue(d, out var alc))
                    {
                        context.Plugins[d] = alc;
                    }
                }
                var pluginAlc = new PluginAssemblyLoadContext(info, context);
                this.HostContext.Plugins[info.PluginName] = pluginAlc;
                var assembly =  pluginAlc.LoadFromAssemblyPath(info.PluginDll);
                result.Add(assembly);
            }

            return result;

        }

        /// <summary>
        /// 从内存卸载插件
        /// </summary>
        /// <param name="info">插件信息</param>
        /// <returns>等待卸载完成</returns>
        public Task Unload(PluginInfo info)
        {
            var cts = new TaskCompletionSource<bool>();
            try
            {
                if(this.HostContext.Plugins.TryGetValue(info.PluginName, out var alc))
                {
                    alc.Unloading += (a) =>
                    {
                        cts.SetResult(true);
                    };
                    alc.Unload();
                }
            }
            catch (Exception e)
            {
                cts.SetException(e);
            }

            return cts.Task;
        }
    }
}
