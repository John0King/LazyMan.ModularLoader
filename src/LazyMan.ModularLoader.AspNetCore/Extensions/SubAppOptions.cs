using System;
using System.Collections.Generic;
using System.Text;

namespace LazyMan.ModularLoader.AspNetCore.Extensions
{
    public class SubAppOptions
    {
        public string SubAppPrefix { get; set; } = null;

        public ModuleManifest ModuleManifest { get; set; } = null;
    }
}
