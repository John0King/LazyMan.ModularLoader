using LazyMan.ModularLoader;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace LoaderRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("before");
            var loader = new HostLoader();
            var path = Path.Combine(AppContext.BaseDirectory, Assembly.GetEntryAssembly().GetName().Name) + ".dll";
            var resolver = new AssemblyDependencyResolver(path);
            resolver.

            var a = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());

            foreach(var assembly in a.Assemblies)
            {
                Console.WriteLine(assembly.FullName);
            }
        }
    }
}
