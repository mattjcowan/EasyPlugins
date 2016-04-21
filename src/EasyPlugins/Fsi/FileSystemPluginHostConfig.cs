using System;
using System.IO;

namespace EasyPlugins.Fsi
{
    public class FileSystemPluginHostConfig
    {
        public Type PluginType { get; set; }
        public string PluginsDirectoryPath { get; set; }
        public string ShadowCopyDirectoryPath { get; set; }
        public string PluginAssemblyNamePatternExclusions { get; set; }
        public string PluginAssemblyFileNamePatternExclusions { get; set; }
    }
}