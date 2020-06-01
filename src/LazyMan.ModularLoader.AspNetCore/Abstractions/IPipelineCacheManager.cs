using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LazyMan.ModularLoader.AspNetCore.Abstractions
{
    public interface IPipelineCacheManager
    {
        IModuleManager ModuleManager { get; }
        (RequestDelegate,IServiceProvider) GetOrCache(string moduleName);

        void RemoveCache(string moduleName);
    }
}
