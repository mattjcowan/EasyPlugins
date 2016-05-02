using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyPlugins
{
    public class PluginRuntime
    {
        public PluginRuntime()
        {
            ActivationExceptions = new List<Exception>();
            DependencyExceptionMessages = new Dictionary<PluginDependency, string[]>();
            ReferencedAssemblies = new Dictionary<string, Assembly>();
        }
        
        public virtual object Plugin { get; set; }

        public virtual Type PluginType { get; set; }
        
        public virtual Assembly PluginAssembly { get; set; }

        /// <summary>
        /// A dictionary of assembly file names to loaded assemblies
        /// </summary>
        public Dictionary<string, Assembly> ReferencedAssemblies { get; }

        /// <summary>
        /// If IsActivated is 'True', then Plugin, PluginType, PluginAssembly will never be null. There may still be
        /// some broken dependencies listed in the DependencyExceptionMessages for optional dependencies. 
        /// If IsActivated is 'False', then Plugin, PluginType, PluginAssembly will always be null, and there will
        /// be one or more exceptions listed in the ActivationExceptions list. 
        /// </summary>
        public virtual bool IsActivated { get; set; }

        public virtual List<Exception> ActivationExceptions { get; }

        public virtual Dictionary<PluginDependency, string[]> DependencyExceptionMessages { get; }
    }
}