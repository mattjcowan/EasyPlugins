using System;
using System.Collections.Generic;
using System.IO;

namespace EasyPlugins.Fs
{
    public class FsPluginRuntime: PluginRuntime
    {
        public virtual FileInfo ManifestFile { get; set; }
        public virtual DirectoryInfo PluginDirectory { get; set; }
    }

    internal class FsRegistry : List<FsRegistryItem>
    {
        public FsRegistry() { }

        public FsRegistry(IEnumerable<FsRegistryItem> collection) : base(collection)
        {
        }
    }

    internal class FsRegistryItem
    {
        public string PluginId { get; set; }
        public bool IsActivated { get; set; }
        public DateTime? ActivatedOn { get; set; }
        public DateTime InstalledOn { get; set; }
    }

    internal enum FsManifestPropertyNames
    {
        PluginId,
        PluginTypeName,
        PluginAssemblyFileName,
        PluginTitle,
        PluginDescription,
        PluginUrl,
        PluginVersion,
        PluginDefaultSettings,
        PluginCategory,
        PluginTags,
        PluginDependencies,
        Author,
        AuthorUrl,
        License,
        LicenseUrl
    }
}