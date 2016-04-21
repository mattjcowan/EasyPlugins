using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlugins
{
    public interface IPluginHostFactory
    {
        IPluginHost Create();
    }

    /// <summary>
    /// The plugin host is responsible for hosting plugins, and informing the registrar of which plugins it contains and the status of these plugins
    /// </summary>
    public interface IPluginHost
    {
        void Initialize();

        Assembly[] LoadAssemblies();
        IPluginManifest[] LoadManifests();

        Task InstallPlugin(string pluginArchive);
        Task UninstallPlugin(string pluginId);
        Task ActivatePlugin(string pluginId);
        Task DeactivatePlugin(string pluginId);
    }
}
