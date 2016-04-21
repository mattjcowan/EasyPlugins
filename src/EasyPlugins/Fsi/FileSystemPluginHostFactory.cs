using System;

namespace EasyPlugins.Fsi
{
    public class FileSystemPluginHostFactory: IPluginHostFactory
    {
        private readonly Action<FileSystemPluginHostConfig> _configurer;

        public FileSystemPluginHostFactory(Action<FileSystemPluginHostConfig> configurer = null)
        {
            _configurer = configurer;
        }

        public IPluginHost Create()
        {
            var config = new FileSystemPluginHostConfig();
            _configurer?.Invoke(config);
            return new FileSystemPluginHost(config);
        }
    }
}