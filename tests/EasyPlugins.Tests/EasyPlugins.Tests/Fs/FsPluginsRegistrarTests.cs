using NUnit.Framework;
using EasyPlugins.Fs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EasyPlugins.Fs
{
    [TestFixture()]
    public class FsPluginsRegistrarTests
    {
        private FsPluginsRegistrar _registrar = null;

        private void ClearDirectories()
        {
            if (_registrar != null)
            {
                // in case something went wrong in a test
                if (File.Exists(_registrar.RegistryFile))
                    File.Delete(_registrar.RegistryFile);

                if (Directory.Exists(_registrar.RegistrarDirectory))
                    Directory.Delete(_registrar.RegistrarDirectory, true);
            }
        }

        [SetUp]
        public void SetUp()
        {
            _registrar = new FsPluginsRegistrar();
            ClearDirectories();
        }

        [TearDown]
        public void TearDown()
        {
            _registrar = null;
        }

        [Test()]
        public void FsPluginsRegistrar_PathsResolveCorrectly()
        {
            var pathTests = new List<string>
            {
                "~/plugins/test", "plugins\\test", "plugins//test",  Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins\\test")
            };

            var shouldAllResolveToDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins\\test");
            var shouldAllResolveToFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins\\test\\" + FsConstants.PluginsRegistryFileName);

            foreach (var path in pathTests)
            {
                var registrar = new FsPluginsRegistrar(path);

                // make sure default paths are as expected
                var directoryPath = registrar.RegistrarDirectory;
                var registryFilePath = registrar.RegistryFile;

                Assert.That(directoryPath, Is.SamePath(shouldAllResolveToDirectoryPath));
                Assert.That(registryFilePath, Is.SamePath(shouldAllResolveToFilePath));

                // directories don't get created until Initialize
                Assert.That(Directory.Exists(_registrar.RegistrarDirectory), Is.False);
                Assert.That(File.Exists(_registrar.RegistryFile), Is.False);
            }
        }

        [Test()]
        public void InitializeTest_WithEmptyPluginManifestList()
        {
            var pluginManifests = new List<PluginManifest>();

            _registrar.Initialize(pluginManifests);

            // directories should have been created
            Assert.That(Directory.Exists(_registrar.RegistrarDirectory), Is.True);
            Assert.That(File.Exists(_registrar.RegistryFile), Is.True);

            // the file should be empty, except for a root node
            var xe = XElement.Load(_registrar.RegistryFile);
            Assert.That(xe.Name.LocalName, Is.EqualTo("plugins"));
            Assert.That(xe.HasAttributes, Is.False);
            Assert.That(xe.HasElements, Is.False);
        }

        [Test()]
        public void InitializeTest_WithManyPluginManifests()
        {
            var pluginManifests = GetSampleListOfPluginManifestsForTest(20);
            _registrar.Initialize(pluginManifests);
            
            var xe = XElement.Load(_registrar.RegistryFile);
            Assert.That(xe.Name.LocalName, Is.EqualTo("plugins"));
            Assert.That(xe.HasAttributes, Is.False);
            Assert.That(xe.HasElements, Is.True);
            Assert.That(xe.Elements().Count(), Is.EqualTo(20));

            // make sure all plugin ids are there
            Assert.That(xe.Elements().Select(x => (string)x.Attribute("pluginId")).ToArray(), 
                Is.EquivalentTo(pluginManifests.Select(m => m.PluginId).ToArray()));

            // make sure there is a settings file for each plugin
            Assert.That(Directory.GetFiles(_registrar.RegistrarDirectory).Length, Is.EqualTo(21));
        }

        [Test()]
        public async Task SavePluginManifest_ShouldAddNewManifests()
        {
            var pluginManifests = GetSampleListOfPluginManifestsForTest(4);
            _registrar.Initialize(pluginManifests);

            // test that it adds a new manifest by adding a setting file
            var newManifest = GetSampleListOfPluginManifestsForTest(1, "additional").First();
            pluginManifests.Add(newManifest);
            await _registrar.SavePluginManifestAsync(newManifest).ConfigureAwait(false);

            Assert.That(Directory.GetFiles(_registrar.RegistrarDirectory).Length, Is.EqualTo(6));

            // test there is a new setting file named appropriately
            Assert.That(Directory.GetFiles(_registrar.RegistrarDirectory, string.Format(FsConstants.PluginsSettingsRegistryFileNameFormat, newManifest.PluginId)).Length, Is.EqualTo(1));
        }

        [Test()]
        public async Task SavePluginManifest_ShouldToggleActivations()
        {
            var pluginManifests = GetSampleListOfPluginManifestsForTest(10);
            _registrar.Initialize(pluginManifests);

            var activeManifest = pluginManifests.FirstOrDefault(x => x.RegistrationInfo.IsActivated);

            Assert.IsNotNull(activeManifest);

            // check that the registry file says it's active
            var registryItem = _registrar.GetRegistry().First(f => f.PluginId == activeManifest.PluginId);

            Assert.That(registryItem.IsActivated, Is.True);

            activeManifest.RegistrationInfo.IsActivated = false;

            await _registrar.SavePluginManifestAsync(activeManifest).ConfigureAwait(false);

            // check that the registry file says it's NOT active

            registryItem = _registrar.GetRegistry().First(f => f.PluginId == activeManifest.PluginId);

            Assert.That(registryItem.IsActivated, Is.False);
        }

        [Test()]
        public async Task SavePluginSettingsAsync_ShouldAccountForNewAndChangedSettings()
        {
            var pluginManifests = GetSampleListOfPluginManifestsForTest(10);
            _registrar.Initialize(pluginManifests);

            var activeManifestWithoutSettings = pluginManifests.FirstOrDefault(x => x.RegistrationInfo.PluginSettings.Count == 0);
            var activeManifestWithSettings = pluginManifests.FirstOrDefault(x => x.RegistrationInfo.PluginSettings.Count > 0);

            Assert.IsNotNull(activeManifestWithoutSettings);
            Assert.IsNotNull(activeManifestWithSettings);

            // add a setting to all the manifests
            activeManifestWithoutSettings.PluginDefaultSettings.Add("new-setting", "blah blah blah");
            activeManifestWithSettings.PluginDefaultSettings.Add("new-setting", "blah blah blah");

            // change a setting
            var keyToModify = activeManifestWithSettings.PluginDefaultSettings.First().Key;
            activeManifestWithSettings.PluginDefaultSettings[keyToModify] = "changed value";

            await _registrar.SavePluginManifestAsync(activeManifestWithoutSettings).ConfigureAwait(false);
            await _registrar.SavePluginManifestAsync(activeManifestWithSettings).ConfigureAwait(false);

            // check that the registry file says it's active
            var registrySettings1 = _registrar.GetRegistryItemSettings(activeManifestWithoutSettings.PluginId);
            var registrySettings2 = _registrar.GetRegistryItemSettings(activeManifestWithSettings.PluginId);

            Assert.That(registrySettings1.ContainsKey("new-setting"), Is.True);
            Assert.That(registrySettings2.ContainsKey("new-setting"), Is.True);
            Assert.That(registrySettings1["new-setting"], Is.EqualTo("blah blah blah"));
            Assert.That(registrySettings2["new-setting"], Is.EqualTo("blah blah blah")); ;
            Assert.That(registrySettings2[keyToModify], Is.EqualTo("changed value"));
        }

        [Test()]
        public async Task MarkAsActivatedAsyncTest()
        {
            var pluginManifests = GetSampleListOfPluginManifestsForTest(10);
            _registrar.Initialize(pluginManifests);

            var manifest = pluginManifests.FirstOrDefault(x => !x.RegistrationInfo.IsActivated);

            Assert.IsNotNull(manifest);

            // plugin should be active to start the test
            Assert.That(pluginManifests.First(p => p.PluginId == manifest.PluginId).RegistrationInfo.IsActivated, Is.False);

            await _registrar.MarkAsActivatedAsync(manifest.PluginId).ConfigureAwait(false);

            _registrar.Initialize(pluginManifests);

            // plugin should be inactive to prove the test
            Assert.That(pluginManifests.First(p => p.PluginId == manifest.PluginId).RegistrationInfo.IsActivated, Is.True);
        }

        [Test()]
        public async Task MarkAsDeactivatedAsyncTest()
        {
            var pluginManifests = GetSampleListOfPluginManifestsForTest(10);
            _registrar.Initialize(pluginManifests);

            var manifest = pluginManifests.FirstOrDefault(x => x.RegistrationInfo.IsActivated);

            Assert.IsNotNull(manifest);

            // plugin should be active to start the test
            Assert.That(pluginManifests.First(p => p.PluginId == manifest.PluginId).RegistrationInfo.IsActivated, Is.True);

            await _registrar.MarkAsDeactivatedAsync(manifest.PluginId).ConfigureAwait(false);

            _registrar.Initialize(pluginManifests);

            // plugin should be inactive to prove the test
            Assert.That(pluginManifests.First(p => p.PluginId == manifest.PluginId).RegistrationInfo.IsActivated, Is.False);
        }

        [Test()]
        public async Task MarkAsUninstalledAsyncTest()
        {
            var pluginManifests = GetSampleListOfPluginManifestsForTest(10);
            _registrar.Initialize(pluginManifests);

            var manifest = pluginManifests[5];

            // test there is a setting file for this manifest
            Assert.That(Directory.GetFiles(_registrar.RegistrarDirectory, string.Format(FsConstants.PluginsSettingsRegistryFileNameFormat, manifest.PluginId)).Length, Is.EqualTo(1));

            await _registrar.MarkAsUninstalledAsync(manifest.PluginId).ConfigureAwait(false);

            // make sure the registry removed it
            Assert.That(_registrar.GetRegistry().Count, Is.EqualTo(pluginManifests.Count - 1));

            // make sure the settings file DID NOT GET DELETED, we want settings to persist between install and uninstalls like WordPress
            Assert.That(Directory.GetFiles(_registrar.RegistrarDirectory, string.Format(FsConstants.PluginsSettingsRegistryFileNameFormat, manifest.PluginId)).Length, Is.EqualTo(1));
        }

        [Test()]
        public async Task RegistrarHandlesMultipleParallelRequests_OnUninstall()
        {
            var pluginManifests = GetSampleListOfPluginManifestsForTest(10);
            _registrar.Initialize(pluginManifests);

            var tasks = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                tasks.Add(_registrar.MarkAsUninstalledAsync(pluginManifests[i].PluginId));
            }
            
            await Task.WhenAll(tasks).ConfigureAwait(false);

            Assert.That(_registrar.GetRegistry().Count, Is.EqualTo(0));
        }

        private List<PluginManifest> GetSampleListOfPluginManifestsForTest(int numberOfPlugins, string pluginIdPrefix = "plugin")
        {
            Random random = new Random();

            var list = new List<PluginManifest>();
            for (var i = 1; i <= numberOfPlugins; i++)
            {
                var plugin = new PluginManifest { PluginId = pluginIdPrefix + i };
                list.Add(plugin);
                if (i%2 == 0)
                {
                    for (var s = 1; s < 10; s++)
                    {
                        var randomNumber = random.Next(0, 1000);

                        var key = "setting-" + randomNumber;
                        if (plugin.PluginDefaultSettings.ContainsKey(key))
                            continue;

                        plugin.PluginDefaultSettings.Add(key, "Random setting for setting " + randomNumber);
                    }
                }
                if (i%3 == 0)
                {
                    plugin.RegistrationInfo.IsActivated = true;
                    plugin.RegistrationInfo.ActivatedOn = DateTime.Now;
                }
            }

            return list;
        }
    }
}