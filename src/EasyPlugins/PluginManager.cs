using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EasyPlugins.Fsi;

namespace EasyPlugins
{
    public sealed class PluginManager: IPluginManager
    {
        #region Singleton implementation
        private static PluginManager _instance;

        public static IPluginManager Instance
        {
            get
            {
                if (_instance == null)
                    throw new EasyPluginException(ErrorCode.MissingInitialization);
                return _instance;
            }
        }
        #endregion

        public static IPluginManager Initialize()
        {
            return PluginManager.Initialize(new FileSystemPluginHostFactory(config =>
            {
                var baseDirectory = Path.Combine(Environment.CurrentDirectory, "App_Plugins");
                config.PluginsDirectoryPath = Path.Combine(baseDirectory, "plugins");
                config.ShadowCopyDirectoryPath = Path.Combine(baseDirectory, "bin");
                config.PluginAssemblyNamePatternExclusions = "^System|^Microsoft";
                config.PluginAssemblyFileNamePatternExclusions = "^System|^mscorlib|^Microsoft|^AjaxControlToolkit|^Antlr3|^Autofac|^AutoMapper|^Castle|^ComponentArt|^CppCodeProvider|^DotNetOpenAuth|^EntityFramework|^EPPlus|^FluentValidation|^ImageResizer|^itextsharp|^log4net|^MaxMind|^MbUnit|^MiniProfiler|^Mono.Math|^MvcContrib|^Newtonsoft|^NHibernate|^nunit|^Org.Mentalis|^PerlRegex|^QuickGraph|^Recaptcha|^Remotion|^RestSharp|^Rhino|^Telerik|^Iesi|^TestDriven|^TestFu|^UserAgentStringLibrary|^VJSharpCodeProvider|^WebActivator|^WebDev|^WebGrease";
            }));
        }

        public static IPluginManager Initialize(IPluginHostFactory pluginHostFactory)
        {
            if (_instance != null)
                throw new EasyPluginException(ErrorCode.AlreadyInitialized);

            var pluginManager = new PluginManager(pluginHostFactory);
            pluginManager.InitializeHost();
            _instance = pluginManager;
            return _instance;
        }

        private PluginManager(IPluginHostFactory hostFactory)
        {
            _pluginHost = hostFactory.Create();
        }

        public long InitializationDuration { get; private set; }

        private Assembly[] _pluginAssemblies;
        private IPluginManifest[] _pluginManifests;
        private readonly IPluginHost _pluginHost;

        public IPluginHost GetPluginHost() { return _pluginHost; }

        public Assembly[] GetPluginAssemblies()
        {
            return _pluginAssemblies;
        }

        public IPluginManifest[] GetPluginManifests()
        {
            return _pluginManifests;
        }

        public async Task InstallPlugin(string pluginArchive)
        {
            await _pluginHost.InstallPlugin(pluginArchive);
        }

        public async Task UninstallPlugin(string pluginId)
        {
            await _pluginHost.UninstallPlugin(pluginId);
        }

        public async Task ActivatePlugin(string pluginId)
        {
            await _pluginHost.ActivatePlugin(pluginId);
        }

        public async Task DeactivatePlugin(string pluginId)
        {
            await _pluginHost.DeactivatePlugin(pluginId);
        }

        #region Private Methods
        private void InitializeHost()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            _pluginHost.Initialize();
            _pluginAssemblies = _pluginHost.LoadAssemblies();
            _pluginManifests = _pluginHost.LoadManifests();

            watch.Stop();
            InitializationDuration = watch.ElapsedMilliseconds;
        }
        #endregion
    }
}