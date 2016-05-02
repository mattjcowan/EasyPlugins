using System;

namespace EasyPlugins
{
    public class PluginEventArgs : EventArgs
    {
        public string PluginId { get; set; }
        public PluginManifest PluginManifest { get; set; }
    }
}