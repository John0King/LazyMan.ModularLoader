using LazyMan.ModularLoader.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LazyMan.ModularLoader.AspNetCore
{
    public class AspNetLoader
    {
        public HostLoader HostLoader { get; private set; }

        public async Task LoadModules()
        {
            HostLoader = new HostLoader(new PluginInfo[0]);
            HostLoader.AddSharedAssembly(this.GetType().Assembly);
        }
    }
}
