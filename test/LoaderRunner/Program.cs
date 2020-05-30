using LazyMan.ModularLoader;
using LazyMan.ModularLoader.Graph;
using LazyMan.ModularLoader.Internal;
using ModuleShared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace LoaderRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(typeof(Program).FullName);
            Console.WriteLine(typeof(AutoMapper.Mapper).Assembly.FullName);
            Console.WriteLine("--------开始加载插件------");
            var plugins = new List<PluginInfo>
            {
                new PluginInfo
                {
                    PluginName = "Module1",
                    PluginDll = Path.Combine(AppContext.BaseDirectory,"modules","Module1", "Module1.dll"),
                },
                new PluginInfo
                {
                    PluginName = "Module2",
                    PluginDll = Path.Combine(AppContext.BaseDirectory,"modules","Module2", "Module2.dll"),
                },
            };

            var loader = new HostLoader(plugins);
            loader.AddSharedAssembly(typeof(IPlugIn).Assembly);

            var assemblies = loader.LoadPlugins();
            var pluginType = typeof(IPlugIn);
            var pTypes =  assemblies.SelectMany(x => x.DefinedTypes)
                .Where(t => pluginType.IsAssignableFrom(t))
                .ToList();

            foreach(var t in pTypes)
            {
                var p = Activator.CreateInstance(t) as IPlugIn;
                var pl = AssemblyLoadContext.GetLoadContext(t.Assembly) as PluginAssemblyLoadContext;
                Console.WriteLine();
                Console.WriteLine($"-----------enter {pl.PluginInfo.PluginName}---------------");
                p.WriteOutPut();
                Console.WriteLine($"-----------leave {pl.PluginInfo.PluginName}---------------");
                Console.WriteLine();
            }

            Console.WriteLine("-------回到核心程序------");

            Console.WriteLine(typeof(Program).FullName);
            Console.WriteLine(typeof(AutoMapper.Mapper).Assembly.FullName);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("-------插件卸载中，开始等待------");
            Task.Run(() =>
            {
                foreach(var info in plugins)
                {
                    var sp = new Stopwatch();
                    sp.Start();
                    loader.Unload(info)
                    .ContinueWith((t)=>
                    {
                        sp.Stop();
                        Console.WriteLine($"{info.PluginName} 卸载完成 , 用时：{ sp.Elapsed.TotalMilliseconds } 毫秒");
                    });
                }
            });

            Console.ReadLine();

        }
    }
}
