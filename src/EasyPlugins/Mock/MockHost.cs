using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlugins.Mock
{
    public class MockHost: IPluginHost
    {
        public void Initialize()
        {
            
        }

        public Assembly[] LoadAssemblies()
        {
            return new Assembly[0];
        }

        public IPluginManifest[] LoadManifests()
        {
            return new IPluginManifest[0];
        }

        public Task InstallPlugin(string pluginArchive)
        {
            return Task.Delay(0);
        }

        public Task UninstallPlugin(string pluginId)
        {
            return Task.Delay(0);
        }

        public Task ActivatePlugin(string pluginId)
        {
            return Task.Delay(0);
        }

        public Task DeactivatePlugin(string pluginId)
        {
            return Task.Delay(0);
        }
    }

    public class MockHostFactory : IPluginHostFactory
    {
        public IPluginHost Create()
        {
            return new MockHost();
        }
    }
}
