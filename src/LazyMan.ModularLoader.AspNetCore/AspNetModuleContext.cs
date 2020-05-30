using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyMan.ModularLoader.AspNetCore
{
    public class AspNetModuleContext
    {
        public IServiceProvider HostProvider { get; set; }

        public IApplicationBuilder ApplicationBuilder { get; set; }
    }
}
