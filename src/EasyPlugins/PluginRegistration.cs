using System;
using System.Collections.Generic;

namespace EasyPlugins
{
    public sealed class PluginRegistration
    {
        public PluginRegistration()
        {
            PluginSettings = new Dictionary<string, string>();
        }

        /// <summary>
        /// The saved settings for the plugin
        /// </summary>
        public Dictionary<string, string> PluginSettings { get; set; }

        /// <summary>
        /// An indicator of whether the plugin is active or not
        /// </summary>
        public bool IsActivated { get; set; }

        /// <summary>
        /// An indicator of when the plugin was activated
        /// </summary>
        public DateTime? ActivatedOn { get; set; }

        /// <summary>
        /// An indicator of when the plugin was activated
        /// </summary>
        public DateTime? InstalledOn { get; set; }
    }
}