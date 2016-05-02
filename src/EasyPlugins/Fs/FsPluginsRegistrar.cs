using EasyPlugins.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EasyPlugins.Fs
{
    public class FsPluginsRegistrar : PluginsRegistrar
    {
        public FsPluginsRegistrar(string pluginsRegistrarFolderVirtualPath = FsConstants.DefaultAppPluginsRegistrarVirtualPath)
        {
            if (string.IsNullOrEmpty(pluginsRegistrarFolderVirtualPath))
                pluginsRegistrarFolderVirtualPath = FsConstants.DefaultAppPluginsRegistrarVirtualPath;

            RegistrarDirectory = new DirectoryInfo(FsUtils.MapPath(pluginsRegistrarFolderVirtualPath)).FullName;
            RegistryFile = new FileInfo(Path.Combine(RegistrarDirectory, FsConstants.PluginsRegistryFileName)).FullName;
        }

        public string RegistrarDirectory { get; private set; }
        public string RegistryFile { get; private set; }

        /// <summary>
        /// Synchronize the underlying registry (on the file system) with the manifests
        /// </summary>
        /// <param name="pluginManifests">A list of all the manifests installed on the system</param>
        public override void Initialize(List<PluginManifest> pluginManifests)
        {
            Directory.CreateDirectory(RegistrarDirectory);

            // load existing registry
            var previousRegistry = GetRegistry();

            // sync manifests with existing registry
            foreach (var pm in pluginManifests)
            {
                var ri = previousRegistry.FirstOrDefault(m => m.PluginId.Equals(pm.PluginId, StringComparison.OrdinalIgnoreCase));
                if (ri != null)
                {
                    pm.RegistrationInfo.IsActivated = ri.IsActivated;
                    pm.RegistrationInfo.ActivatedOn = ri.ActivatedOn;
                    pm.RegistrationInfo.InstalledOn = ri.InstalledOn;
                }
            }

            // update registry (this will also get rid of any plugin references that were manually removed or otherwise deleted)
            var registry = new FsRegistry(pluginManifests.Select(m => MapToFsRegistryItem(m)));
            SaveRegistry(registry);

            // load and update plugin registry settings
            foreach (var manifest in pluginManifests)
            {
                manifest.RegistrationInfo.PluginSettings = manifest.PluginDefaultSettings;

                // merge saved settings with the plugin settings
                var settings = GetRegistryItemSettings(manifest.PluginId);
                foreach (var setting in settings)
                {
                    manifest.RegistrationInfo.PluginSettings[setting.Key] = setting.Value;
                }
                SaveRegistryItemSettings(manifest.PluginId, manifest.RegistrationInfo.PluginSettings);
            }
        }

        public override async Task SavePluginManifestAsync(PluginManifest manifest)
        {
            await Task.Delay(0);

            // load existing registry
            var registry = GetRegistry();

            var registryItem = registry.FirstOrDefault(
                r => r.PluginId.Equals(manifest.PluginId, StringComparison.OrdinalIgnoreCase));
            var updatedRegistryItem = MapToFsRegistryItem(manifest, registryItem);

            if (registryItem == null)
                registry.Add(updatedRegistryItem);

            // save registry
            SaveRegistry(registry);

            // save settings
            if (manifest.RegistrationInfo.PluginSettings == null || manifest.RegistrationInfo.PluginSettings.Count == 0)
                manifest.RegistrationInfo.PluginSettings = manifest.PluginDefaultSettings;

            SaveRegistryItemSettings(manifest.PluginId, manifest.RegistrationInfo.PluginSettings);
        }

        public override async Task SavePluginSettingsAsync(string pluginId, Dictionary<string, string> pluginSettings)
        {
            await Task.Delay(0);
            SaveRegistryItemSettings(pluginId, pluginSettings);
        }

        public override async Task MarkAsActivatedAsync(string pluginId)
        {
            await Task.Delay(0);

            // load existing registry
            var registry = GetRegistry();

            var registryItem = registry.FirstOrDefault(
                r => r.PluginId.Equals(pluginId, StringComparison.OrdinalIgnoreCase));

            if (registryItem != null)
            {
                registryItem.IsActivated = true;
                registryItem.ActivatedOn = DateTime.Now;

                // save registry
                SaveRegistry(registry);
            }
        }

        public override async Task MarkAsDeactivatedAsync(string pluginId)
        {
            await Task.Delay(0);

            // load existing registry
            var registry = GetRegistry();

            var registryItem = registry.FirstOrDefault(
                r => r.PluginId.Equals(pluginId, StringComparison.OrdinalIgnoreCase));

            if (registryItem != null)
            {
                registryItem.IsActivated = false;
                registryItem.ActivatedOn = null;

                // save registry
                SaveRegistry(registry);
            }
        }

        public override async Task MarkAsUninstalledAsync(string pluginId)
        {
            await Task.Delay(0);

            // update registry without a reference to the plugin
            var registry = GetRegistry();
            if (0 < registry.RemoveAll(r => r.PluginId.Equals(pluginId, StringComparison.OrdinalIgnoreCase)))
            {
                SaveRegistry(registry);
            }
        }

        #region Private methods

        private static readonly object RegistryLock = new object();

        #region Mappers
        private static FsRegistryItem MapToFsRegistryItem(PluginManifest m, FsRegistryItem useExistingRegistryItem = null)
        {
            var ri = useExistingRegistryItem ?? new FsRegistryItem();
            ri.PluginId = m.PluginId;
            ri.IsActivated = m.RegistrationInfo.IsActivated;
            ri.ActivatedOn = m.RegistrationInfo.ActivatedOn;
            ri.InstalledOn = m.RegistrationInfo.InstalledOn.GetValueOrDefault(DateTime.Now);
            return ri;
        }

        private static FsRegistryItem MapToFsRegistryItem(XElement x)
        {
            return new FsRegistryItem
            {
                PluginId = (string)x.Attribute("pluginId"),
                IsActivated = ((bool?)x.Attribute("isActivated")).GetValueOrDefault(false),
                ActivatedOn = (DateTime?)x.Attribute("activatedOn"),
                InstalledOn = ((DateTime?)x.Attribute("installedOn")).GetValueOrDefault(DateTime.Now)
            };
        }

        private static FsRegistry MapToFsRegistry(XElement xe)
        {
            return new FsRegistry(xe.Elements("plugin").Select(MapToFsRegistryItem));
        }

        private static XElement MapToXElement(FsRegistry registry)
        {
            var xe = new XElement("plugins");
            foreach (var registryItem in registry)
            {
                xe.Add(MapToXElement(registryItem));
            }
            return xe;
        }

        private static XElement MapToXElement(FsRegistryItem registryItem)
        {
            var xri = new XElement("plugin", new XAttribute("pluginId", registryItem.PluginId));
            xri.SetAttributeValue("isActivated", registryItem.IsActivated);
            xri.SetAttributeValue("activatedOn", registryItem.ActivatedOn);
            xri.SetAttributeValue("installedOn", registryItem.InstalledOn);
            return xri;
        }

        private static XElement MapToXElement(Dictionary<string, string> pluginSettings)
        {
            return new XElement("settings",
                (pluginSettings ?? new Dictionary<string, string>()).Select(s =>
                    new XElement("setting", new XAttribute("key", s.Key), s.Value)
                    ));
        }

        private static Dictionary<string, string> MapToDictionary(XElement xe)
        {
            return xe.Elements("setting")
                .ToDictionary(
                    k => (string)k.Attribute("key"),
                    v => v.Value.NormalizeCarriageReturns()
                );
        }
        #endregion // Mappers

        internal FsRegistry GetRegistry()
        {
            if (!File.Exists(RegistryFile))
                return new FsRegistry();

            // load from disk
            XElement xe;
            lock (RegistryLock)
            {
                xe = XElement.Load(RegistryFile, LoadOptions.PreserveWhitespace);
            }
            return MapToFsRegistry(xe);
        }

        private void SaveRegistry(FsRegistry registry)
        {
            var xe = MapToXElement(registry);

            // save to disk
            lock (RegistryLock)
            {
                xe.Save(RegistryFile, SaveOptions.None);
            }
        }

        internal Dictionary<string, string> GetRegistryItemSettings(string pluginId)
        {
            var pluginSettingsFile = Path.Combine(RegistrarDirectory,
                string.Format(FsConstants.PluginsSettingsRegistryFileNameFormat, pluginId));

            if (!File.Exists(pluginSettingsFile))
                return new Dictionary<string, string>();

            // load from disk
            XElement xe;
            lock (RegistryLock)
            {
                xe = XElement.Load(pluginSettingsFile, LoadOptions.PreserveWhitespace);
            }

            return MapToDictionary(xe);
        }

        private void SaveRegistryItemSettings(string pluginId, Dictionary<string, string> pluginSettings)
        {
            // delete the settings if applicable
            var xe = MapToXElement(pluginSettings);

            // save to disk
            var pluginSettingsFile = Path.Combine(RegistrarDirectory,
                string.Format(FsConstants.PluginsSettingsRegistryFileNameFormat, pluginId));

            lock (RegistryLock)
            {
                xe.Save(pluginSettingsFile, SaveOptions.None);
            }
        }

        #endregion Private methods
    }
}