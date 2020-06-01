using System;
using System.Collections.Generic;
using System.Text;

namespace LazyMan.ModularLoader.AspNetCore.Abstractions
{
    public class ModuleInstallOption
    {
        public string RemoteModuleLocation { get; set; }

        public RemoteModuleFileType RemoteModuleFileType { get; set; }

        public RemoteType RemoteType { get; set; } 
    }

    public enum RemoteModuleFileType
    {
        Zip,
        Folder
    }

    public enum RemoteType
    {
        HttpOrHttps,
        Local,
        //Ftp
    }
}
