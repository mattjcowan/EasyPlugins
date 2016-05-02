using EasyPlugins.Fs;
using EasyPlugins.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyPlugins
{
    public sealed class PluginsManager
    {
        #region Singleton implementation

        private static PluginsManager _instance;

        public static PluginsManager Instance
        {
            get
            {
                if (_instance == null)
                    throw new PluginsException(ErrorCode.NotInitialized);
                return _instance;
            }
        }

        /// <summary>
        /// Initialize the plugin manager to setup with EasyPlugins system.
        /// </summary>
        /// <param name="pluginProvider"></param>
        /// <returns></returns>
        public static PluginsManager Initialize(PluginsProvider pluginProvider = null)
        {
            return Initialize((Type)null, pluginProvider);
        }

        /// <summary>
        /// Initialize the plugin manager to setup with EasyPlugins system.
        /// </summary>
        /// <param name="pluginType"></param>
        /// <param name="pluginProvider"></param>
        /// <returns></returns>
        public static PluginsManager Initialize(Type pluginType, PluginsProvider pluginProvider = null)
        {
            return Initialize(pluginType == null ? null: DefaultPluginActivator(pluginType), pluginProvider);
        }

        /// <summary>
        /// Initialize the plugin manager to setup with EasyPlugins system.
        /// </summary>
        /// <param name="pluginActivator"></param>
        /// <param name="pluginProvider"></param>
        /// <returns></returns>
        public static PluginsManager Initialize(Func<PluginManifest, Assembly, object> pluginActivator, PluginsProvider pluginProvider = null)
        {
            if (_instance != null)
                throw new PluginsException(ErrorCode.PluginManagerAlreadyInitialized);

            var watch = new Stopwatch();
            watch.Start();

            _instance = new PluginsManager(pluginActivator, pluginProvider).Init();

            watch.Stop();
            _instance.InitializationDuration = watch.ElapsedMilliseconds;

            return _instance;
        }

        #endregion // Singleton implementation

        private readonly Func<PluginManifest, Assembly, object> _pluginActivator;
        private readonly List<PluginManifest> _pluginManifests;
        private readonly List<Assembly> _pluginAssemblies;
        private readonly List<Assembly> _pluginReferencedAssemblies;
        private readonly PluginsProvider _pluginsProvider;

        internal PluginsManager(Func<PluginManifest, Assembly, object> pluginActivator, PluginsProvider pluginProvider)
        {
            _pluginActivator = pluginActivator ?? DefaultPluginActivator(typeof(IEasyPlugin));
            _pluginsProvider = pluginProvider ?? new FsPluginsProvider();

            _pluginManifests = new List<PluginManifest>();
            _pluginAssemblies = new List<Assembly>();
            _pluginReferencedAssemblies = new List<Assembly>();
        }

        private PluginsManager Init()
        {
            // populate the plugin manifest and assembly lists
            _pluginsProvider.Initialize(_pluginManifests);

            // at this point, manifests and assemblies have been retrieved, but plugins
            // have not been activated and created
            var pluginReferenceMap = new Dictionary<string, Assembly>();
            foreach (
                var manifest in
                    _pluginManifests.Where(
                        m => m.RuntimeInfo.IsActivated && m.RuntimeInfo.ReferencedAssemblies.Any()))
            {   
                // if the plugin manifest specifies a plugin assembly, then use that, otherwise, 
                // iterate each referenced assembly to search for the plugin type
                if (!string.IsNullOrEmpty(manifest.PluginAssemblyFileName))
                {
                    var assembly = manifest.RuntimeInfo.ReferencedAssemblies.GetValueOrDefault(manifest.PluginAssemblyFileName, null);
                    if (assembly != null)
                    {
                        DiscoverRuntimePlugin(manifest, assembly);
                    }
                }
                else
                {
                    foreach (var assembly in manifest.RuntimeInfo.ReferencedAssemblies.Values)
                    {
                        if (DiscoverRuntimePlugin(manifest, assembly))
                        {
                            break;
                        }
                    }
                }

                // do not activate plugin if the plugin could not be created
                if (manifest.RuntimeInfo.Plugin == null)
                {
                    manifest.RuntimeInfo.IsActivated = false;
                    manifest.RuntimeInfo.ActivationExceptions.Add(new PluginsException(ErrorCode.MissingPluginException));
                }
                else
                {
                    pluginReferenceMap.Merge(manifest.RuntimeInfo.ReferencedAssemblies);
                }
            }

            if (pluginReferenceMap.Count > 0)
            {
                _pluginReferencedAssemblies.AddRange(pluginReferenceMap.Values);
            }

            return this;
        }

        private bool DiscoverRuntimePlugin(PluginManifest manifest, Assembly assembly)
        {
            var hasSpecificType = !string.IsNullOrEmpty(manifest.PluginTypeName);
            var pluginType = hasSpecificType
                ? assembly.GetType(manifest.PluginTypeName)
                : null;
            if (hasSpecificType && pluginType == null)
                return false;

            var activator = pluginType == null ? _pluginActivator : DefaultPluginActivator(pluginType);
            var plugin = activator.Invoke(manifest, assembly);
            if (plugin != null)
            {
                manifest.RuntimeInfo.Plugin = plugin;
                manifest.RuntimeInfo.PluginAssembly = assembly;
                manifest.RuntimeInfo.PluginType = plugin.GetType();

                if (!_pluginAssemblies.Contains(assembly))
                    _pluginAssemblies.Add(assembly);
            }
            return manifest.RuntimeInfo.Plugin != null;
        }

        public List<Assembly> GetPluginAssemblies(bool allReferencedAssemblies = false)
        {
            return allReferencedAssemblies ? _pluginReferencedAssemblies.ToList(): _pluginAssemblies;
        }

        public List<PluginManifest> GetPluginManifests(bool activeOnly = false)
        {
            return activeOnly ? _pluginManifests.Where(pm => pm.RuntimeInfo.IsActivated).ToList(): _pluginManifests;
        }

        public PluginManifest GetPluginManifest(string pluginId)
        {
            return
                _pluginManifests.FirstOrDefault(pm => pm.PluginId.Equals(pluginId, StringComparison.OrdinalIgnoreCase));
        }

        public List<TPlugin> GetPlugins<TPlugin>(Func<PluginManifest, bool> filter = null) where TPlugin : class
        {
            var plugins = new List<TPlugin>();
            foreach (var manifest in _pluginManifests)
            {
                if (manifest.RuntimeInfo != null && manifest.RuntimeInfo.IsActivated && manifest.RuntimeInfo.Plugin != null)
                {
                    if (filter == null || filter(manifest))
                    {
                        var typedPlugin = manifest.RuntimeInfo.Plugin as TPlugin;
                        if (typedPlugin != null)
                            plugins.Add(typedPlugin);
                    }
                }
            }
            return plugins;
        }

        public long InitializationDuration { get; private set; }

        private static readonly object Sync = new object(); // single lock for field updates
        private bool _restartPending;

        public bool RestartPending
        {
            get { lock (Sync) { return _restartPending; } }
        }

        public event AsyncEventHandler<EventArgs> OnBeforeInstallEventHandler;

        public event AsyncEventHandler<PluginEventArgs> OnPluginInstalledEventHandler;

        public event AsyncEventHandler<PluginEventArgs> OnPluginUninstalledEventHandler;

        public event AsyncEventHandler<PluginEventArgs> OnPluginActivatedEventHandler;

        public event AsyncEventHandler<PluginEventArgs> OnPluginDeactivatedEventHandler;

        public event AsyncEventHandler<PluginEventArgs> OnPluginSettingsChangedEventHandler;

        private void ModifyPluginManifests(Func<PluginManifest, bool> manifestAction)
        {
            lock (Sync)
            {
                foreach (var pluginManifest in _pluginManifests)
                {
                    if (manifestAction(pluginManifest))
                    {
                        _restartPending = true;
                        break;
                    }
                }
            }
        }

        public async Task RestartAppDomainAsync()
        {
            if (OnBeforeInstallEventHandler != null)
                await OnBeforeInstallEventHandler.WhenInvokeAll(this, null).ConfigureAwait(false);

            await _pluginsProvider.RestartAppDomainAsync().ConfigureAwait(false);
        }

        public async Task<PluginManifest> InstallPluginAsync(Uri pluginUri)
        {
            var tempFile = Path.GetTempPath();
            using (var client = new WebClient())
            {
                try
                {
                    await client.DownloadFileTaskAsync(pluginUri, tempFile).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new PluginsException(ErrorCode.PluginDownloadException, ex);
                }

                return await InstallPluginAsync(tempFile).ConfigureAwait(false);
            }
        }

        public async Task<PluginManifest> InstallPluginAsync(string pluginArchive)
        {
            var manifest = await _pluginsProvider.InstallPluginAsync(pluginArchive).ConfigureAwait(false);
            if (manifest != null)
            {
                lock (Sync)
                {
                    var pm = _pluginManifests.FirstOrDefault(m => m.PluginId.Equals(manifest.PluginId, StringComparison.OrdinalIgnoreCase));
                    if (pm != null)
                    {
                        pm.RegistrationInfo.InstalledOn = manifest.RegistrationInfo.InstalledOn;
                        pm.RegistrationInfo.IsActivated = manifest.RegistrationInfo.IsActivated;
                        pm.RegistrationInfo.ActivatedOn = manifest.RegistrationInfo.ActivatedOn;
                    }
                    else
                    {
                        _pluginManifests.Add(manifest);
                    }
                };
                if (OnPluginInstalledEventHandler != null)
                    await
                        OnPluginInstalledEventHandler.WhenInvokeAll(this,
                            new PluginEventArgs { PluginId = manifest.PluginId, PluginManifest = manifest }).ConfigureAwait(false);

                return GetPluginManifest(manifest.PluginId);
            }
            return null;
        }

        public async Task UninstallPluginAsync(string pluginId)
        {
            await _pluginsProvider.UninstallPluginAsync(pluginId).ConfigureAwait(false);
            ModifyPluginManifests(pm =>
            {
                if (!pm.PluginId.Equals(pluginId, StringComparison.OrdinalIgnoreCase))
                    return false;
                
                pm.RegistrationInfo.IsActivated = false;
                return true;
            });

            if (OnPluginUninstalledEventHandler != null)
                await
                    OnPluginUninstalledEventHandler.WhenInvokeAll(this,
                        new PluginEventArgs { PluginId = pluginId }).ConfigureAwait(false);
        }

        public async Task<PluginManifest> ActivatePluginAsync(string pluginId)
        {
            await _pluginsProvider.ActivatePluginAsync(pluginId).ConfigureAwait(false);

            ModifyPluginManifests(pm =>
            {
                if (!pm.PluginId.Equals(pluginId, StringComparison.OrdinalIgnoreCase))
                    return false;

                pm.RegistrationInfo.IsActivated = true;
                return true;
            });

            var manifest = GetPluginManifest(pluginId);
            if (manifest != null)
            {
                if (OnPluginActivatedEventHandler != null)
                    await OnPluginActivatedEventHandler.WhenInvokeAll(this,
                            new PluginEventArgs { PluginId = pluginId, PluginManifest = manifest }).ConfigureAwait(false);
            }
            return manifest;
        }

        public async Task<PluginManifest> DeactivatePluginAsync(string pluginId)
        {
            await _pluginsProvider.ActivatePluginAsync(pluginId).ConfigureAwait(false);
            ModifyPluginManifests(pm =>
            {
                if (!pm.PluginId.Equals(pluginId, StringComparison.OrdinalIgnoreCase))
                    return false;

                pm.RegistrationInfo.IsActivated = false;
                pm.RegistrationInfo.ActivatedOn = null;
                return true;
            });

            var manifest = GetPluginManifest(pluginId);
            if (manifest != null)
            {
                if (OnPluginDeactivatedEventHandler != null)
                    await OnPluginDeactivatedEventHandler.WhenInvokeAll(this,
                            new PluginEventArgs { PluginId = pluginId, PluginManifest = manifest }).ConfigureAwait(false);
            }
            return manifest;
        }

        public async Task<PluginManifest> ResetPluginSettingsAsync(string pluginId)
        {
            return await UpdatePluginSettingsAsync(pluginId, new Dictionary<string, string>()).ConfigureAwait(false);
        }

        public async Task<PluginManifest> UpdatePluginSettingsAsync(string pluginId, Dictionary<string, string> pluginSettings)
        {
            await _pluginsProvider.SavePluginSettingsAsync(pluginId, pluginSettings).ConfigureAwait(false);
            ModifyPluginManifests(pm =>
            {
                if (!pm.PluginId.Equals(pluginId, StringComparison.OrdinalIgnoreCase))
                    return false;

                pm.RegistrationInfo.PluginSettings = pluginSettings;
                return true;
            });

            var manifest = GetPluginManifest(pluginId);
            if (manifest != null)
            {
                if (OnPluginSettingsChangedEventHandler != null)
                    await OnPluginSettingsChangedEventHandler.WhenInvokeAll(this,
                            new PluginEventArgs { PluginId = pluginId, PluginManifest = manifest }).ConfigureAwait(false);
            }
            return manifest;
        }

        #region Private methods
        private static Func<PluginManifest, Assembly, object> DefaultPluginActivator(Type pluginType)
        {
            var pluginActivationType = pluginType;
            Func<PluginManifest, Assembly, object> pluginActivator = (manifest, assembly) =>
            {
                var instanceType = assembly.GetTypes().FirstOrDefault(
                    t => !t.IsAbstract && !t.IsInterface && t.IsClass &&
                         pluginActivationType.IsAssignableFrom(t));

                return instanceType == null ? null : Activator.CreateInstance(instanceType);
            };
            return pluginActivator;
        }
        #endregion
    }
}