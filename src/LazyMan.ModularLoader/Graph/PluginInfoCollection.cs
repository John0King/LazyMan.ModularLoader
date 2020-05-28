using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyMan.ModularLoader.Graph
{
    public class PluginInfoCollection 
    {
        protected Dictionary<string,PluginInfoWrap> Infos = new Dictionary<string, PluginInfoWrap>(StringComparer.OrdinalIgnoreCase);
        public void Add(PluginInfo info)
        {
            PluginInfoWrap? wrap = null;
            if (Infos.ContainsKey(info.PluginName))
            {
                wrap = Infos[info.PluginName];
                if (wrap.IsReal)
                {
                    return;
                }
                
            }
            if(wrap == null)
            {
                wrap = new PluginInfoWrap(ref this.Infos, info);
                Infos[info.PluginName] = wrap;
            }
            else
            {
                wrap.SetInfo(info);
            }
            wrap.AddCount();
        }

        public void AddRange(IEnumerable<PluginInfo> infos)
        {
            foreach(var info in infos)
            {
                Add(info);
            }
        }

        public IEnumerable<PluginInfo> GetOrderedPlugins()
        {
            return Infos.Values.Cast<PluginInfoWrap>().OrderByDescending(p => p.Count).Select(x => x.PluginInfo!);
        }
    }
}
