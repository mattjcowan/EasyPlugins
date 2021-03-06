﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EasyPlugins {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ErrorMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ErrorMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("EasyPlugins.ErrorMessages", typeof(ErrorMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This plugin is marked as activated, but is not installed. Install the plugin to continue ....
        /// </summary>
        internal static string ActivePluginIsNotInstalled {
            get {
                return ResourceManager.GetString("ActivePluginIsNotInstalled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An unexpected initialization error occurred..
        /// </summary>
        internal static string InitializationException {
            get {
                return ResourceManager.GetString("InitializationException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This plugin depends on another missing plugin: {0}.
        /// </summary>
        internal static string MissingPluginDependencyFormat {
            get {
                return ResourceManager.GetString("MissingPluginDependencyFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The manifest plugin was not found or could not be created from any of the provided reference assemblies..
        /// </summary>
        internal static string MissingPluginException {
            get {
                return ResourceManager.GetString("MissingPluginException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This plugin has some non-optional dependencies that have not been met..
        /// </summary>
        internal static string NonOptionalPluginDependencyExceptions {
            get {
                return ResourceManager.GetString("NonOptionalPluginDependencyExceptions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You must first initialize the plugin manager before attempting to use it..
        /// </summary>
        internal static string NotInitialized {
            get {
                return ResourceManager.GetString("NotInitialized", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An unspecified error has occurred inside the EasyPlugins library..
        /// </summary>
        internal static string NotSpecified {
            get {
                return ResourceManager.GetString("NotSpecified", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This plugin depends on another plugin &apos;{0}&apos; that has not been activated..
        /// </summary>
        internal static string PluginDependencyInactiveConflictFormat {
            get {
                return ResourceManager.GetString("PluginDependencyInactiveConflictFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The plugin {0} version {1} is greater than the maximum version {2} dependency requirement..
        /// </summary>
        internal static string PluginDependencyMaxVersionConflictFormat {
            get {
                return ResourceManager.GetString("PluginDependencyMaxVersionConflictFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The plugin {0} version {1} is less than the minimum version {2} dependency requirement..
        /// </summary>
        internal static string PluginDependencyMinVersionConflictFormat {
            get {
                return ResourceManager.GetString("PluginDependencyMinVersionConflictFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred while attempting to download the plugin..
        /// </summary>
        internal static string PluginDownloadException {
            get {
                return ResourceManager.GetString("PluginDownloadException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The plugin manager was already initialized. Please fix your code to ensure it does not get called twice..
        /// </summary>
        internal static string PluginManagerAlreadyInitialized {
            get {
                return ResourceManager.GetString("PluginManagerAlreadyInitialized", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The plugin provider was already initialized. Only one plugin provider of this type can exist in the system at one time..
        /// </summary>
        internal static string PluginProviderAlreadyInitialized {
            get {
                return ResourceManager.GetString("PluginProviderAlreadyInitialized", resourceCulture);
            }
        }
    }
}
