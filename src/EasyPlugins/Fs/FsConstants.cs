using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlugins.Fs
{
    public static class FsConstants
    {
        public const string DefaultPluginManifestFileName = "plugin.xml";
        public const string DefaultAppPluginsVirtualPath = "~/App_Plugins/plugins";
        public const string DefaultAppPluginsShadowCopyVirtualPath = "~/App_Plugins/bin";
        public const string DefaultAppPluginsRegistrarVirtualPath = "~/App_Data/plugins";

        public const string PluginsRegistryFileName = "plugins.xml";
        public const string PluginsSettingsRegistryFileNameFormat = "settings.{0}.xml";
    }
}
