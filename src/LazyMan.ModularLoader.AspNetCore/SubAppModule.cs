using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LazyMan.ModularLoader.AspNetCore
{
    public class SubAppModule : AspNetModule
    {

        public override Task Load(AspNetModuleContext moduleContext)
        {
            throw new NotImplementedException();
        }

        public override Task UnLoad()
        {
            throw new NotImplementedException();
        }
    }
}
