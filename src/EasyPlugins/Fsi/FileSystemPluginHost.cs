using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyPlugins.Fsi
{
    public class FileSystemPluginHost : IPluginHost
    {
        private static Assembly[] _assemblies = null;
        private static readonly object AssemblyListLocker = new object();

        private static PluginManifest[] _manifests = null;
        private static readonly object ManifestListLocker = new object();

        public FileSystemPluginHostConfig PluginHostConfig { get; set; }
        public DirectoryInfo PluginsDirectory { get; }
        public DirectoryInfo ShadowCopyDirectory { get; }

        public FileSystemPluginHost(FileSystemPluginHostConfig pluginHostConfig)
        {
            PluginHostConfig = pluginHostConfig;
            PluginsDirectory = Directory.CreateDirectory(PluginHostConfig.PluginsDirectoryPath);
            ShadowCopyDirectory = Directory.CreateDirectory(PluginHostConfig.ShadowCopyDirectoryPath);
        }

        public void Initialize()
        {
            
        }

        public Assembly[] LoadAssemblies()
        {
            return _assemblies;
        }

        public IPluginManifest[] LoadManifests()
        {
            return _manifests;
        }

        public async Task InstallPlugin(string pluginArchive)
        {
            await Task.Delay(0);
        }

        public async Task UninstallPlugin(string pluginId)
        {
            await Task.Delay(0);
        }

        public async Task ActivatePlugin(string pluginId)
        {
            await Task.Delay(0);
        }

        public async Task DeactivatePlugin(string pluginId)
        {
            await Task.Delay(0);
        }
    }
}