using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using EasyPlugins.Utils;

namespace EasyPlugins.Fs
{
    public class FsPluginsProvider : PluginsProvider
    {
        #region Constructors
        public FsPluginsProvider() : this((string) null, (string) null, (string) null)
        {
        }

        public FsPluginsProvider(
            string pluginsFolderVirtualPath = FsConstants.DefaultAppPluginsVirtualPath,
            string shadowCopyDirectoryVirtualPath = FsConstants.DefaultAppPluginsShadowCopyVirtualPath,
            string pluginsRegistrarFolderVirtualPath = FsConstants.DefaultAppPluginsRegistrarVirtualPath) : this(
                pluginsFolderVirtualPath,
                shadowCopyDirectoryVirtualPath,
                new FsPluginsRegistrar(pluginsRegistrarFolderVirtualPath))
        {
        }

        public FsPluginsProvider(
            string pluginsFolderVirtualPath = FsConstants.DefaultAppPluginsVirtualPath,
            string shadowCopyDirectoryVirtualPath = FsConstants.DefaultAppPluginsShadowCopyVirtualPath,
            PluginsRegistrar pluginsRegistrar = null) : base(pluginsRegistrar)
        {
            if (string.IsNullOrEmpty(pluginsFolderVirtualPath))
                pluginsFolderVirtualPath = FsConstants.DefaultAppPluginsVirtualPath;

            if (string.IsNullOrEmpty(shadowCopyDirectoryVirtualPath))
                shadowCopyDirectoryVirtualPath = FsConstants.DefaultAppPluginsShadowCopyVirtualPath;

            if (pluginsRegistrar == null)
                pluginsRegistrar = new FsPluginsRegistrar(FsConstants.DefaultAppPluginsRegistrarVirtualPath);

            PluginsDirectoryPath = new DirectoryInfo(pluginsFolderVirtualPath.StartsWith("~")
                ? FsUtils.MapPath(pluginsFolderVirtualPath)
                : pluginsFolderVirtualPath).FullName;
            ShadowCopyDirectoryPath = new DirectoryInfo(shadowCopyDirectoryVirtualPath.StartsWith("~")
                ? FsUtils.MapPath(shadowCopyDirectoryVirtualPath)
                : shadowCopyDirectoryVirtualPath).FullName;
        }
        #endregion

        public string ShadowCopyDirectoryPath { get; private set; }
        public string PluginsDirectoryPath { get; private set; }

        // Config options

        public List<string> AssemblyFileNamesToIgnore { get; set; }
        public Func<FileInfo, bool> AssemblyFilesToIgnoreExpression { get; set; }
        public string AssemblyFileNamesToIgnoreRegexPattern { get; set; }
        public Func<DirectoryInfo, FileInfo> PluginManifestDiscoverer { get; set; }
        public Func<FileInfo, PluginManifest> PluginManifestDeserializer { get; set; }
        public Func<DirectoryInfo, FileInfo[]> PluginAssemblyDiscoverer { get; set; }

        #region Initialization
        protected static bool IsInitialized;
        protected static readonly object InitializationLock = new object();
        protected internal override void Initialize(List<PluginManifest> pluginManifests)
        {
            if (!IsInitialized)
            {
                lock (InitializationLock)
                {
                    if (!IsInitialized)
                    {
                        try
                        {
                            if (AssemblyFileNamesToIgnore == null)
                                AssemblyFileNamesToIgnore = new List<string>();

                            PopulateAssemblyFileNamesToIgnoreFromBinDirectories(AssemblyFileNamesToIgnore);

                            if (!Directory.Exists(PluginsDirectoryPath))
                            {
                                Directory.CreateDirectory(PluginsDirectoryPath);
                            }

                            // clear the shadow copy directory
                            try
                            {
                                if (Directory.Exists(ShadowCopyDirectoryPath))
                                {
                                    FsUtils.DeleteDirectory(ShadowCopyDirectoryPath, true);
                                }
                            }
                            catch (Exception)
                            {
                                // if IIS is holding on to the assemblies in the shadow copy directory for some reason, we may need to 
                                // reload the app domain to force the unlocking of the assemblies
                                FsUtils.RestartHostedAppDomain();
                                return;
                            }

                            if (!Directory.Exists(ShadowCopyDirectoryPath))
                            {
                                Directory.CreateDirectory(ShadowCopyDirectoryPath);
                            }

                            IsInitialized = true;
                        }
                        catch (Exception ex)
                        {
                            throw new PluginsException(ErrorCode.InitializationException, ex.Message, ex);
                        }
                    }
                }
            }
            else
            {
                throw new PluginsException(ErrorCode.PluginProviderAlreadyInitialized);
            }

            base.Initialize(pluginManifests);
        }
        #endregion

        /// <summary>
        /// The first method that is called shortly after initialization during startup
        /// </summary>
        protected internal override List<PluginManifest> LoadPluginManifests()
        {
            if (!IsInitialized)
                throw new PluginsException(ErrorCode.NotInitialized);

            if (!Directory.Exists(PluginsDirectoryPath))
                return new List<PluginManifest>();

            var manifests = new List<PluginManifest>();
            foreach (var pluginDirectoryPath in Directory.GetDirectories(PluginsDirectoryPath))
            {
                var pluginDirectory = new DirectoryInfo(pluginDirectoryPath);
                var pluginManifest = LoadPluginManifest(pluginDirectory);
                manifests.Add(pluginManifest);
            }

            return manifests;
        }

        /// <summary>
        /// Loads the plugin assemblies for active manifests and populates the runtime info 
        /// for these plugins (i.e.: Plugin, PluginType, PluginAssembly, ReferencedAssemblies, and/or 
        /// ActivationExceptions)
        /// </summary>
        protected internal override List<Assembly> LoadPluginAssemblies(List<PluginManifest> activeManifests)
        {
            if (!IsInitialized)
                throw new PluginsException(ErrorCode.NotInitialized);

            if (!Directory.Exists(PluginsDirectoryPath) || activeManifests == null || !activeManifests.Any())
                return new List<Assembly>();

            var pluginToAssemblyMap = ShadowCopyAssemblyFiles(activeManifests);

            // Load the assemblies into the app domain (NO NEED to add them to the build manager, the base provider does that for you)
            var assemblies = new List<Assembly>();
            foreach (var assemblyFile in new DirectoryInfo(ShadowCopyDirectoryPath).GetFiles("*.dll", SearchOption.TopDirectoryOnly))
            {
                var asm = Assembly.LoadFrom(assemblyFile.FullName);
                assemblies.Add(asm);

                // Add the assembly as a reference to every plugin it is associated with
                foreach (var manifest in activeManifests)
                {
                    var manifestAssemblyFiles = pluginToAssemblyMap.GetValueOrDefault(manifest.PluginId, new List<FileInfo>());
                    if (manifestAssemblyFiles.Any(f => f.Name.Equals(assemblyFile.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        manifest.RuntimeInfo.ReferencedAssemblies.Add(assemblyFile.Name, asm);
                    }
                }
            }

            // Return an aggregate list of all shadow copied assemblies
            return assemblies;
        }

        protected internal override async Task<PluginManifest> ExtractPluginAsync(string pluginArchive)
        {
            await Task.Delay(0);

            if (!File.Exists(pluginArchive))
                return null;

            // first thing we want to see is if the plugin archive has a singular folder at the top of it's tree
            // or is already exploded. this will determine how and where we extract it to
            string rootDirectoryName;
            using (var archive = ZipFile.OpenRead(pluginArchive))
            {
                rootDirectoryName = archive.Entries.Count == 1 ? archive.Entries[0].Name: null;
            }

            var pluginArchiveName = new FileInfo(pluginArchive).Name;
            var pluginDirName = rootDirectoryName ?? pluginArchiveName;
            var pluginDirPath = Path.Combine(PluginsDirectoryPath, pluginDirName);
            var directoryToExtractTo = rootDirectoryName != null
                ? PluginsDirectoryPath
                : pluginDirPath;

            // rename the old plugin dir if it exists
            var moved = false;
            var previousPluginDirBackupPath = string.Empty;
            if (Directory.Exists(pluginDirPath))
            {
                var backupDirName = pluginDirName + "_" + DateTime.Now.ToString("yyyyMMdd_hhmmsst");
                previousPluginDirBackupPath = Path.Combine(PluginsDirectoryPath, backupDirName);
                FsUtils.DeleteDirectory(previousPluginDirBackupPath);

                FsUtils.MoveDirectory(pluginDirPath, previousPluginDirBackupPath, false, 3);
                moved = Directory.Exists(previousPluginDirBackupPath);
            }

            // make sure the root directory we are extracting to exists
            Directory.CreateDirectory(directoryToExtractTo);
            
            // extract
            try
            {
                ZipFile.ExtractToDirectory(pluginArchive, directoryToExtractTo);
                // delete the backed up directory after successful extraction
                FsUtils.DeleteDirectory(previousPluginDirBackupPath);
            }
            catch
            {
                if (!string.IsNullOrEmpty(previousPluginDirBackupPath))
                {
                    FsUtils.DeleteDirectory(pluginDirPath); // in case something got partially extracted
                    FsUtils.MoveDirectory(previousPluginDirBackupPath, pluginDirPath, false, 3);
                }
                throw;
            }

            // return the plugin manifest
            return LoadPluginManifest(new DirectoryInfo(pluginDirPath));
        }

        #region Protected methods

        protected virtual PluginManifest LoadPluginManifest(DirectoryInfo pluginDirectory)
        {
            var pluginManifestFile = PluginManifestDiscoverer != null ? 
                PluginManifestDiscoverer(pluginDirectory) : 
                pluginDirectory.GetFiles(FsConstants.DefaultPluginManifestFileName, SearchOption.TopDirectoryOnly).FirstOrDefault();

            PluginManifest manifest = null;

            if (pluginManifestFile != null && pluginManifestFile.Exists)
            {
                manifest = LoadPluginManifest(pluginManifestFile);
            }

            if(manifest == null)
            {
                manifest = new PluginManifest { PluginId = pluginDirectory.Name };
            }

            manifest.RuntimeInfo = new FsPluginRuntime
            {
                ManifestFile = pluginManifestFile,
                PluginDirectory = pluginDirectory
            };

            return manifest;
        }

        protected virtual PluginManifest LoadPluginManifest(FileInfo pluginManifestFile)
        {
            if (pluginManifestFile != null && pluginManifestFile.Exists)
            {
                if (PluginManifestDeserializer != null)
                {
                    return PluginManifestDeserializer(pluginManifestFile);
                }

                var m = new PluginManifest();
                var xe = XElement.Load(pluginManifestFile.FullName, LoadOptions.PreserveWhitespace);
                foreach (var xec in xe.Elements())
                {
                    FsManifestPropertyNames prop;
                    if (!Enum.TryParse(xec.Name.LocalName, true, out prop))
                    {
                        // if the element is not recognized, add it as a setting
                        if(!m.PluginDefaultSettings.ContainsKey(xec.Name.LocalName))
                            m.PluginDefaultSettings.Add(xec.Name.LocalName, xec.Value);
                        continue;
                    }

                    switch (prop)
                    {
                        case FsManifestPropertyNames.PluginId:
                            m.PluginId = xec.Value;
                            break;
                        case FsManifestPropertyNames.PluginTypeName:
                            m.PluginTypeName = xec.Value;
                            break;
                        case FsManifestPropertyNames.PluginAssemblyFileName:
                            m.PluginAssemblyFileName = xec.Value;
                            break;
                        case FsManifestPropertyNames.PluginTitle:
                            m.PluginTitle = xec.Value;
                            break; ;
                        case FsManifestPropertyNames.PluginDescription:
                            m.PluginDescription = xec.Value;
                            break; ;
                        case FsManifestPropertyNames.PluginUrl:
                            m.PluginUrl = xec.Value;
                            break; ;
                        case FsManifestPropertyNames.PluginVersion:
                            Version v;
                            if (Version.TryParse(xec.Value, out v))
                                m.PluginVersion = v;
                            break;
                        case FsManifestPropertyNames.PluginDefaultSettings:
                            foreach (var setting in xec.Elements())
                            {
                                if (setting.HasAttributes)
                                {
                                    var key = setting.Attributes().First().Value;
                                    if (!string.IsNullOrWhiteSpace(key))
                                        m.PluginDefaultSettings[key] = setting.Value;
                                }
                            }
                            break;
                        case FsManifestPropertyNames.PluginCategory:
                            m.PluginCategory = xec.Value;
                            break;
                        case FsManifestPropertyNames.PluginTags:
                            foreach (var tag in xec.Elements())
                            {
                                var t = tag.Value;
                                if (!string.IsNullOrWhiteSpace(t) && !m.PluginTags.Contains(t))
                                {
                                    m.PluginTags.Add(t);
                                }
                            }
                            break;
                        case FsManifestPropertyNames.Author:
                            m.Author = xec.Value;
                            break;
                        case FsManifestPropertyNames.AuthorUrl:
                            m.AuthorUrl = xec.Value;
                            break;
                        case FsManifestPropertyNames.License:
                            m.License = xec.Value;
                            break;
                        case FsManifestPropertyNames.LicenseUrl:
                            m.LicenseUrl = xec.Value;
                            break;
                        case FsManifestPropertyNames.PluginDependencies:
                            foreach (var dependency in xec.Elements())
                            {
                                if (dependency.HasAttributes)
                                {
                                    var attDict =
                                        dependency.Attributes()
                                            .ToDictionary(k => k.Name.LocalName.ToLowerInvariant(), dv => dv.Value);
                                    if (attDict.ContainsKey("pluginid"))
                                    {
                                        var d = new PluginDependency
                                        {
                                            PluginId = attDict["pluginid"],
                                            IsOptional = attDict.GetValueOrDefault("optional", bool.FalseString)
                                                .Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase)
                                        };
                                        Version mv;
                                        if (attDict.ContainsKey("minversion") &&
                                            Version.TryParse(attDict["minversion"], out mv))
                                            d.MinVersion = mv;
                                        Version xv;
                                        if (attDict.ContainsKey("maxversion") &&
                                            Version.TryParse(attDict["maxversion"], out xv))
                                            d.MaxVersion = xv;
                                        m.PluginDependencies.Add(d);
                                    }
                                }
                            }
                            break;
                    }
                }

                return m;
            }
            return null;
        }

        protected virtual Dictionary<string, List<FileInfo>> ShadowCopyAssemblyFiles(List<PluginManifest> activeManifests)
        {
            var pluginToAssemblyMap = new Dictionary<string, List<FileInfo>>();
            foreach (var pluginManifest in activeManifests)
            {
                var pluginId = pluginManifest.PluginId;
                var runtimeInfo = pluginManifest.RuntimeInfo as FsPluginRuntime;
                if (runtimeInfo == null)
                    continue;

                var assemblyFilesToShadowCopy = PluginAssemblyDiscoverer != null ?
                    PluginAssemblyDiscoverer(runtimeInfo.PluginDirectory):
                    runtimeInfo.PluginDirectory.GetFiles("*.dll", SearchOption.AllDirectories);

                foreach (var assemblyFile in assemblyFilesToShadowCopy)
                {
                    if (AssemblyFileNamesToIgnore != null && AssemblyFileNamesToIgnore.Contains(assemblyFile.Name, StringComparer.OrdinalIgnoreCase))
                        continue;

                    if (AssemblyFilesToIgnoreExpression != null && AssemblyFilesToIgnoreExpression(assemblyFile))
                        continue;

                    if (!string.IsNullOrWhiteSpace(AssemblyFileNamesToIgnoreRegexPattern) &&
                        Regex.IsMatch(assemblyFile.Name, AssemblyFileNamesToIgnoreRegexPattern))
                        continue;

                    var dest = Path.Combine(ShadowCopyDirectoryPath, assemblyFile.Name);

                    if (!File.Exists(dest))
                    {
                        assemblyFile.CopyTo(dest, true);
                    }

                    if (File.Exists(dest))
                    {
                        var destFile = new FileInfo(dest);
                        if (!pluginToAssemblyMap.ContainsKey(pluginId))
                            pluginToAssemblyMap.Add(pluginId, new List<FileInfo>());
                        pluginToAssemblyMap[pluginId].Add(destFile);
                    }
                }
            }
            return pluginToAssemblyMap;
        }
        
        #endregion

        #region Private methods
        private void PopulateAssemblyFileNamesToIgnoreFromBinDirectories(List<string> assemblyFileNamesToIgnore)
        {
            foreach (var file in new DirectoryInfo(FsUtils.GetAppBinDirectory()).GetFiles("*.dll", SearchOption.TopDirectoryOnly))
            {
                if (!assemblyFileNamesToIgnore.Contains(file.Name, StringComparer.OrdinalIgnoreCase))
                    assemblyFileNamesToIgnore.Add(file.Name);
            }
        }
        #endregion
    }
}
