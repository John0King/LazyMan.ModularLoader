using System;
using System.Collections.Generic;
using System.Text;

namespace LazyMan.ModularLoader.Graph
{
    public class PluginInfoWrap
    {
        private readonly Dictionary<string, PluginInfoWrap> _collection;

        public PluginInfoWrap(ref Dictionary<string,PluginInfoWrap> collection,PluginInfo pluginInfo)
        {
            _collection = collection;
            if(pluginInfo != null)
            {
                PluginInfo = pluginInfo;
                IsReal = true;
            }
            
        }
        public PluginInfo? PluginInfo { get; private set; }

        public int Count { get; private set; }

        public bool IsReal { get; set; } = false;

        public void SetInfo(PluginInfo info)
        {
            this.PluginInfo = info;
            this.IsReal = true;
        }

        public void AddCount()
        {
            this.Count++;

            foreach(var name in this.PluginInfo.DependedPlugins)
            {
                if (_collection.ContainsKey(name) && !name.TextEq(this.PluginInfo.PluginName))
                {
                    var info = _collection[name];
                    info.AddCount();
                }
            }
        }

        public bool Equals(PluginInfoWrap wrap)
        {
            return this.PluginInfo.PluginName.TextEq(wrap.PluginInfo.PluginName);
        }

    }
}
