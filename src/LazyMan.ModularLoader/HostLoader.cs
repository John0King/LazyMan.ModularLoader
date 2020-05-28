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
        public virtual void LoadPlugin(string pluginName, string pluginAssemblyPath)
        {
            //// 优先处理共享程序集
            //var sharedAssebmles = SharedAssemblySelector?.Invoke(pluginAssembly);
            //var sharedLoadContext = new AssemblyLoadContext($"{nameof(LazyMan)}:Shared:{pluginName}", true);


            //sharedLoadContext
            var pluginResolver = new PluginAssemblyDependencyResolver(pluginAssemblyPath, new PluginContext(this.HostLoadContext)
            {
                SharedAssemblyLoadContexts = SharedPlugins
            });
            var pluginContext = new PluginAssemblyLoadContext($"{nameof(LazyMan)}:Plugin:{pluginName}", pluginResolver);
            pluginContext.LoadFromAssemblyPath(pluginAssemblyPath);
            IsolatedPlugins.Add(pluginContext);
        }




        public virtual void LoadPlugin(PluginInfo info)
        {
            // 1. get dependency alc

            // 2. use the alc to create current alc

            // 3. add to alc list
        }
        public virtual void LoadPlugins()
        {
            // load as the right order
            var infos = this.HostContext.PluginInfos.GetOrderedPlugins();
            var alcDic = new Dictionary<string, AssemblyLoadContext>(StringComparer.OrdinalIgnoreCase);
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
                var pluginContext = new PluginAssemblyLoadContext(info, context);
                alcDic.Add(info.PluginName, pluginContext);
                pluginContext.LoadFromAssemblyPath(info.PluginDll);
            }



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
                var alc = AssemblyLoadContext.All
                    .Where(x => x.Name.TextEq($"{nameof(LazyMan)}:Plugin:{info.PluginName}"))
                    .FirstOrDefault();

                alc.Unloading += (a) =>
                {
                    cts.SetResult(true);
                };
                alc.Unload();
            }
            catch (Exception e)
            {
                cts.SetException(e);
            }

            return cts.Task;
        }
    }
}
