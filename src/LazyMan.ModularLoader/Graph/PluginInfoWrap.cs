using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyMan.ModularLoader.Graph
{
    public class PluginInfoWrap
    {
        private readonly Dictionary<string, PluginInfoWrap> _collection;

        public PluginInfoWrap(ref Dictionary<string,PluginInfoWrap> collection,PluginInfo pluginInfo)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            PluginInfo = pluginInfo ?? throw new ArgumentNullException(nameof(pluginInfo));
            PluginName = pluginInfo.PluginName;
            IsReal = true;
            
        }

        public PluginInfoWrap(ref Dictionary<string, PluginInfoWrap> collection, string pluginName)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            PluginName = pluginName ?? throw new ArgumentNullException(nameof(pluginName));
            PluginInfo = null;
            IsReal = false;
        }

        public string PluginName { get; set; }
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

            foreach(var name in this.PluginInfo?.DependedPlugins ?? Enumerable.Empty<string>())
            {
                if (_collection.ContainsKey(name) && !name.TextEq(this.PluginName))
                {
                    var info = _collection[name];
                    info.AddCount();
                }
            }
        }

        public void MinusCount()
        {
            this.Count--;
            foreach (var name in this.PluginInfo?.DependedPlugins ?? Enumerable.Empty<string>())
            {
                if (_collection.ContainsKey(name) && !name.TextEq(this.PluginName))
                {
                    var info = _collection[name];
                    info.MinusCount();
                }
            }
        }

        public bool Equals(PluginInfoWrap wrap)
        {
            return this.PluginName.TextEq(wrap.PluginName);
        }

    }
}
