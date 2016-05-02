using System;
using System.Collections.Generic;

namespace EasyPlugins
{
    public sealed class PluginManifest
    {
        public PluginManifest()
        {
            RegistrationInfo = new PluginRegistration();
            PluginDefaultSettings = new Dictionary<string, string>();
            PluginTags = new List<string>();
            PluginDependencies = new List<PluginDependency>();
        }

        /// <summary>
        /// (Required) This is the Id (aka: unique name) of the plugin.
        /// The id of the plugin is the only required field
        /// for the plugin.
        /// </summary>
        public string PluginId { get; set; }

        /// <summary>
        /// (Recommended) The C# class name for the plugin
        /// </summary>
        public string PluginTypeName { get; set; }

        /// <summary>
        /// (Recommended) The C# assembly name which contains the plugin assembly
        /// </summary>
        public string PluginAssemblyFileName { get; set; }

        /// <summary>
        /// (Recommended) A user friendly name for the plugin.
        /// </summary>
        public string PluginTitle { get; set; }

        /// <summary>
        /// (Recommended) A description for your plugin.
        /// </summary>
        public string PluginDescription { get; set; }

        /// <summary>
        /// (Recommended) The version of the plugin (eg: 1.5, or 1.5.2)
        /// </summary>
        public Version PluginVersion { get; set; }

        /// <summary>
        /// (Optional) A URL to a page that further describes the plugin and how it can be used
        /// </summary>
        public string PluginUrl { get; set; }

        /// <summary>
        /// (Optional) A URL to an icon for the plugin (ideally: 256x256)
        /// </summary>
        public string PluginIconUrl { get; set; }

        /// <summary>
        /// (Optional) A general category for the plugin
        /// </summary>
        public string PluginCategory { get; set; }

        /// <summary>
        /// (Optional) A set of tags for the plugin
        /// </summary>
        public List<string> PluginTags { get; }

        /// <summary>
        /// (Optional) The author of the plugin
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// (Optional) A URL to the author page
        /// </summary>
        public string AuthorUrl { get; set; }

        /// <summary>
        /// (Optional) The license for the plugin
        /// </summary>
        public string License { get; set; }

        /// <summary>
        /// (Optional) A URL to the license page
        /// </summary>
        public string LicenseUrl { get; set; }

        /// <summary>
        /// (Optional) The default plugin settings for the plugin, these are settings that the plugin
        /// typically exposes to users of the system to configure and edit.
        /// </summary>
        public Dictionary<string, string> PluginDefaultSettings { get; }

        /// <summary>
        /// (Optional) A list of plugin dependencies on which this plugin depends
        /// </summary>
        public List<PluginDependency> PluginDependencies { get; }

        // The following properties are set at runtime during Initialization

        public PluginRegistration RegistrationInfo { get; internal set; }

        public PluginRuntime RuntimeInfo { get; internal set; }
    }
}