using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyPlugins
{
    public abstract class PluginsRegistrar
    {
        /// <summary>
        /// Provides a mechanism for the underlying registrar to 'sync' the list of installed plugins
        /// on a particular machine with the registry. This happens early on at startup, when
        /// the plugin manager is first being initialized.
        /// </summary>
        /// <param name="pluginManifests">
        ///     Full list of all the manifests that are installed on the given machine. 
        ///     It's expected that this is the entire list. Any plugins NOT included in the list should be considered 'uninstalled' 
        ///     for the given machineName.
        /// </param>
        public abstract void Initialize(List<PluginManifest> pluginManifests);

        public abstract Task SavePluginManifestAsync(PluginManifest manifest);

        public abstract Task SavePluginSettingsAsync(string pluginId, Dictionary<string, string> pluginSettings);

        public abstract Task MarkAsActivatedAsync(string pluginId);

        public abstract Task MarkAsDeactivatedAsync(string pluginId);

        public abstract Task MarkAsUninstalledAsync(string pluginId);
    }
}