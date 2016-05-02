namespace EasyPlugins
{
    public enum ErrorCode
    {
        NotSpecified,
        NotInitialized,
        PluginManagerAlreadyInitialized,
        InitializationException,
        PluginDownloadException,
        PluginProviderAlreadyInitialized,
        NonOptionalPluginDependencyExceptions,
        ActivePluginIsNotInstalled,
        MissingPluginException
    }
}