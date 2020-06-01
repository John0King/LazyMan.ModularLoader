using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LazyMan.ModularLoader
{
    public class PluginInfo
    {
        public string PluginName { get; set; } = "";

        public string? PluginFolder => Path.GetDirectoryName(PluginDll);

        public string PluginDll { get; set; } = "";

        /// <summary>
        /// 依赖的插件
        /// </summary>
        public IEnumerable<string> DependedPlugins { get; set; } = Enumerable.Empty<string>();
    }
}
