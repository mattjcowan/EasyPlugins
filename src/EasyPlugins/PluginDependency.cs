using System;

namespace EasyPlugins
{
    public class PluginDependency
    {
        public virtual string PluginId { get; set; }
        public virtual Version MinVersion { get; set; }
        public virtual Version MaxVersion { get; set; }
        public virtual bool IsOptional { get; set; }

        public override string ToString()
        {
            return
                string.Format("{0},{1},{2}", PluginId, MinVersion != null ? MinVersion.ToString(3) : "",
                    MaxVersion != null ? MaxVersion.ToString(3) : "").TrimEnd(',');
        }
    }
}