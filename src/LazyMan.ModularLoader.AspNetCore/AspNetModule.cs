using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LazyMan.ModularLoader.AspNetCore
{
    public abstract class AspNetModule
    {
        public abstract Task Load(AspNetModuleContext moduleContext);

        public abstract Task UnLoad();
    }
}
