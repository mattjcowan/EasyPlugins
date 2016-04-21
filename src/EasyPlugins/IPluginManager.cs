using System.Reflection;
using System.Threading.Tasks;

namespace EasyPlugins
{
    public interface IPluginManager
    {
        long InitializationDuration { get; }

        IPluginHost GetPluginHost();
        Assembly[] GetPluginAssemblies();
        IPluginManifest[] GetPluginManifests();

        Task InstallPlugin(string pluginArchive);
        Task UninstallPlugin(string pluginId);
        Task ActivatePlugin(string pluginId);
        Task DeactivatePlugin(string pluginId);
    }
}