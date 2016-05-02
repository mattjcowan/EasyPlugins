using EasyPlugins.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Web.Hosting;

namespace EasyPlugins
{
    /// <summary>
    /// NOT Thread Safe, make sure this is managed properly as part of the PluginsManager singleton
    /// </summary>
    public abstract class PluginsProvider
    {
        protected virtual PluginsRegistrar PluginsRegistrar { get; }

        protected PluginsProvider(PluginsRegistrar pluginsRegistrar)
        {
            PluginsRegistrar = pluginsRegistrar;
        }

        /// <summary>
        /// This method should populate the pluginManifests, including registration info
        /// and runtime info (with the exception of the actual runtime plugin objects)
        /// </summary>
        /// <param name="pluginManifests"></param>
        protected internal virtual void Initialize(List<PluginManifest> pluginManifests)
        {
            // Get the raw manifests from the underlying provider
            var manifests = LoadPluginManifests();

            // Sync the manifests down to the registrar (this will populate the Registration info)
            PluginsRegistrar.Initialize(manifests);

            // Validate if there are any dependency and/or activation exceptions for each plugin
            ValidateManifestsForRuntimeActivationAndDependencyExceptions(manifests);

            // Set the list of manifests, topologically sorted based on dependencies
            // so that downstream methods can reference the plugins in the proper order
            var sortedManifests = new Dictionary<string, PluginManifest>();
            SortManifests(manifests, sortedManifests, new Dictionary<string, int>());
            manifests = sortedManifests.Values.ToList();

            // Load assemblies for active manifests
            PopulatePluginManifestAssemblies(manifests.Where(m => m.RegistrationInfo.IsActivated).ToList());

            pluginManifests.AddRange(manifests);
        }

        private void SortManifests(List<PluginManifest> manifests, Dictionary<string, PluginManifest> sortedManifests, Dictionary<string, int> passesPerPluginId)
        {
            foreach (var manifest in manifests.Where(m => !sortedManifests.ContainsKey(m.PluginId)))
            {
                if (manifest.PluginDependencies.Count == 0)
                {
                    sortedManifests.Add(manifest.PluginId, manifest);
                    continue;
                }

                var hasUnmetDependencies = false;
                foreach (var dependency in manifest.PluginDependencies)
                {
                    if (!sortedManifests.ContainsKey(dependency.PluginId))
                    {
                        hasUnmetDependencies = true;
                        break;
                    }
                }

                if (hasUnmetDependencies)
                {
                    // if this is the second pass, we have a cyclic dependency, so we'll just accept the dependency after a couple tries or we'll be stuck here forever
                    if (passesPerPluginId.ContainsKey(manifest.PluginId) && passesPerPluginId[manifest.PluginId] > 2)
                        sortedManifests.Add(manifest.PluginId, manifest);
                    else
                        passesPerPluginId[manifest.PluginId] =
                            passesPerPluginId.GetValueOrDefault(manifest.PluginId, 0) + 1;
                }
                else
                {
                    sortedManifests.Add(manifest.PluginId, manifest);
                }
            }

            if (manifests.Count > sortedManifests.Count)
                SortManifests(manifests, sortedManifests, passesPerPluginId);
        }

        public virtual async Task RestartAppDomainAsync()
        {
            FsUtils.RestartHostedAppDomain();
            await Task.Delay(0);
        }

        /// <summary>
        /// This method should load plugin manifests from the system without
        /// setting the registration and runtime info
        /// </summary>
        /// <returns></returns>
        protected internal abstract List<PluginManifest> LoadPluginManifests();

        /// <summary>
        /// This method will load and populate the referenced assemblies for each manifest runtime
        /// and return an aggregate distinct list of all assemblies loaded
        /// </summary>
        protected internal abstract List<Assembly> LoadPluginAssemblies(List<PluginManifest> manifests);

        /// <summary>
        /// This method will load and populate the referenced assemblies for each manifest runtime
        /// and return an aggregate distinct list of all assemblies loaded
        /// </summary>
        protected void PopulatePluginManifestAssemblies(List<PluginManifest> activeManifests)
        {
            var assemblies = LoadPluginAssemblies(activeManifests);

            // add the assembly to the build manager so that it can be made available
            // to aspnet runtime build compilations (i.e.: Razor, etc...)
            if (HostingEnvironment.IsHosted)
            {
                foreach (var asm in assemblies)
                {
                    BuildManager.AddReferencedAssembly(asm);
                    BuildManager.AddCompilationDependency(asm.FullName);
                }
            }
        }

        /// <summary>
        /// This method will install a plugin archive file (*.zip) and return a manifest for the file
        /// </summary>
        /// <param name="pluginArchive"></param>
        /// <returns></returns>
        protected internal abstract Task<PluginManifest> ExtractPluginAsync(string pluginArchive);

        /// <summary>
        /// This method will install a plugin archive file (*.zip) and return a manifest for the file
        /// </summary>
        /// <param name="pluginArchive"></param>
        /// <returns></returns>
        public virtual async Task<PluginManifest> InstallPluginAsync(string pluginArchive)
        {
            var manifest = await ExtractPluginAsync(pluginArchive).ConfigureAwait(false);
            if (manifest != null)
            {
                await PluginsRegistrar.SavePluginManifestAsync(manifest).ConfigureAwait(false);
            }
            return manifest;
        }

        /// <summary>
        /// This method will uninstall a plugin from the underlying provider and registrar
        /// </summary>
        /// <param name="pluginId"></param>
        /// <returns></returns>
        public virtual async Task UninstallPluginAsync(string pluginId)
        {
            await PluginsRegistrar.MarkAsUninstalledAsync(pluginId).ConfigureAwait(false);
        }

        public virtual async Task ActivatePluginAsync(string pluginId)
        {
            await PluginsRegistrar.MarkAsActivatedAsync(pluginId).ConfigureAwait(false);
        }

        public virtual async Task DeactivatePluginAsync(string pluginId)
        {
            await PluginsRegistrar.MarkAsDeactivatedAsync(pluginId).ConfigureAwait(false);
        }

        public virtual async Task SavePluginSettingsAsync(string pluginId, Dictionary<string, string> pluginSettings)
        {
            await PluginsRegistrar.SavePluginSettingsAsync(pluginId, pluginSettings).ConfigureAwait(false);
        }

        #region Private methods

        private static void ValidateManifestsForRuntimeActivationAndDependencyExceptions(List<PluginManifest> manifests)
        {
            foreach (var manifest in manifests.Where(m => m.RegistrationInfo.IsActivated))
            {
                manifest.RuntimeInfo.IsActivated = true;

                if (manifest.PluginDependencies == null || !manifest.PluginDependencies.Any())
                {
                    manifest.RuntimeInfo.IsActivated = true;
                    continue;
                }

                var nonOptionalErrorsExist = false;
                var pluginDependenciesWithErrors = new Dictionary<PluginDependency, string[]>();
                foreach (var dependency in manifest.PluginDependencies)
                {
                    var dependencyManifest = manifests.SingleOrDefault(
                        m => m.PluginId.Equals(dependency.PluginId, StringComparison.OrdinalIgnoreCase));

                    var errorMessages = new List<string>();
                    if (dependencyManifest == null)
                    {
                        errorMessages.Add(string.Format(ErrorMessages.MissingPluginDependencyFormat, dependency.PluginId));
                    }
                    else if (!dependencyManifest.RegistrationInfo.IsActivated)
                    {
                        errorMessages.Add(
                            string.Format(ErrorMessages.PluginDependencyInactiveConflictFormat,
                                dependency.PluginId));
                    }
                    else
                    {
                        ValidateVersionCompatibility(dependency, dependencyManifest, errorMessages);
                    }

                    if (errorMessages.Any())
                    {
                        pluginDependenciesWithErrors.Add(dependency, errorMessages.ToArray());
                    }
                    if (errorMessages.Any() && !dependency.IsOptional)
                    {
                        nonOptionalErrorsExist = true;
                    }
                }

                // make sure the plugin is NOT active if there are any plugin dependencies without optional errors
                if (pluginDependenciesWithErrors.Any())
                {
                    foreach (var error in pluginDependenciesWithErrors)
                    {
                        manifest.RuntimeInfo.DependencyExceptionMessages.Add(error.Key, error.Value);
                    }

                    if (nonOptionalErrorsExist)
                    {
                        manifest.RuntimeInfo.IsActivated = false;
                        manifest.RuntimeInfo.ActivationExceptions.Add(
                            new PluginsException(ErrorCode.NonOptionalPluginDependencyExceptions));
                    }
                }
            }
        }

        private static void ValidateVersionCompatibility(PluginDependency dependency, PluginManifest dependencyManifest,
            List<string> errorMessages)
        {
            ValidateMinVersionCompatibility(dependency, dependencyManifest, errorMessages);
            ValidateMaxVersionCompatibility(dependency, dependencyManifest, errorMessages);
        }

        private static void ValidateMaxVersionCompatibility(PluginDependency dependency, PluginManifest dependencyManifest,
            List<string> errorMessages)
        {
            if (dependency.MaxVersion != null && dependency.MaxVersion != default(Version) &&
                (dependencyManifest.PluginVersion == null ||
                 dependency.MaxVersion < dependencyManifest.PluginVersion))
            {
                errorMessages.Add(
                    string.Format(ErrorMessages.PluginDependencyMaxVersionConflictFormat,
                        dependency.PluginId,
                        dependencyManifest.PluginVersion == null
                            ? ""
                            : dependencyManifest.PluginVersion.ToString(3),
                        dependency.MaxVersion.ToString(3)));
            }
        }

        private static void ValidateMinVersionCompatibility(PluginDependency dependency, PluginManifest dependencyManifest,
            List<string> errorMessages)
        {
            if (dependency.MinVersion != null && dependency.MinVersion != default(Version) &&
                (dependencyManifest.PluginVersion == null ||
                 dependency.MinVersion > dependencyManifest.PluginVersion))
            {
                errorMessages.Add(
                    string.Format(ErrorMessages.PluginDependencyMinVersionConflictFormat,
                        dependency.PluginId,
                        dependencyManifest.PluginVersion == null
                            ? ""
                            : dependencyManifest.PluginVersion.ToString(3),
                        dependency.MinVersion.ToString(3)));
            }
        }

        #endregion Private Methods
    }
}