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
        public HostLoader()
        {

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

        public void ForceHostShared(Func<AssemblyName, bool> condition)
        {
            this.HostContext.Conditions.Add(condition);
        }

        /// <summary>
        /// 加载新的插件
        /// </summary>
        /// <param name="info"></param>
        public virtual (PluginInfo info, PluginAssemblyLoadContext? alc) LoadPlugin(PluginInfo info)
        {

            if (this.HostContext.Plugins.ContainsKey(info.PluginName))
            {
                var plugin = this.HostContext.Plugins[info.PluginName];
                return (info, plugin);
            }
            this.HostContext.PluginInfos.Add(info);
            var context = new PluginContext(this.HostContext);
            foreach (var d in info.DependedPlugins)
            {
                if (this.HostContext.Plugins.TryGetValue(d, out var alc))
                {
                    context.Plugins[d] = alc;
                }
            }
            try
            {
                var pluginAlc = new PluginAssemblyLoadContext(info, context);
                this.HostContext.Plugins[info.PluginName] = pluginAlc;
                return (info, pluginAlc);
            }
            catch
            {
                return (info, null);
            }
            

        }

        /// <summary>
        /// 从已有的列表中加载插件
        /// </summary>
        public virtual IEnumerable<(PluginInfo info, PluginAssemblyLoadContext? alc)> LoadPlugins(IEnumerable<PluginInfo> pluginInfos)
        {
            this.HostContext.PluginInfos.AddRange(pluginInfos);
            var result = new List<(PluginInfo,PluginAssemblyLoadContext?)>();
            // load as the right order
            var infos = this.HostContext.PluginInfos.GetOrderedPlugins();
            foreach (var info in infos)
            {
                var r = LoadPlugin(info);
                result.Add(r);
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
            var tasks = new List<Task>();
            var infos = GetUnloadEffected(info);
            infos.Reverse(); // 以相反的方向加载,  第一个是依赖此插件的最严重的, 最后一个是当前插件
            foreach (var i in infos)
            {
                this.HostContext.PluginInfos.Remove(info);
                tasks.Add(this.InternalUnloadOnly(info));
            }

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// 获取受影响的信息, 注意 此处第一个是当前插件信息,下面是依赖它的插件信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public IEnumerable<PluginInfo> GetUnloadEffected(PluginInfo info)
        {
            return this.HostContext.PluginInfos.GetRemoveEffected(info);
        }

        /// <summary>
        /// 仅卸载不管其他事情
        /// </summary>
        /// <param name="info">插件信息</param>
        /// <returns></returns>
        private Task InternalUnloadOnly(PluginInfo info)
        {
            var cts = new TaskCompletionSource<bool>();
            try
            {
                if (this.HostContext.Plugins.TryGetValue(info.PluginName, out var alc))
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
