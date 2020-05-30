using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LazyMan.ModularLoader.AspNetCore.Extensions
{
    public class AssemblyHelper
    {
        public static Type FindEntrypointType(Assembly assembly)
        {
            return assembly.DefinedTypes
                .Where(t => t.GetMethod("Main", 0, 
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, Type.DefaultBinder, 
                    new[] { typeof(string[]) }, null)
                        != null)
                .FirstOrDefault();
        }

        public static Type FindStartupType(Assembly assembly)
        {
            return assembly.DefinedTypes
                .Where(t => t.Name == "Startup")
                .FirstOrDefault();
        }
    }
}
