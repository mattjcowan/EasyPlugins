using NUnit.Framework;
using EasyPlugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyPlugins.Fs;
using EasyPlugins.Utils;

namespace EasyPlugins
{
    [TestFixture()]
    public class PluginsManagerTests
    {
        private static readonly string baseDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
        private static readonly string pluginsDir = Path.Combine(baseDir, "_test_pluginsdir");
        private static readonly string shadowCopyDir = Path.Combine(baseDir, "_test_shadowcopydir");
        private static readonly string registryDir = Path.Combine(baseDir, "_test_registrydir");
        private static readonly string registryFile = Path.Combine(registryDir, FsConstants.PluginsRegistryFileName);

        [SetUp]
        public void SetUp()
        {
            FsUtils.DeleteDirectory(shadowCopyDir, true);
            FsUtils.DeleteDirectory(registryDir, true);
        }

        [TearDown]
        public void TearDown()
        {
            FsUtils.DeleteDirectory(shadowCopyDir, true);
            FsUtils.DeleteDirectory(registryDir, true);
        }

        [Test, RunInApplicationDomain]
        public void Initializing_without_options_should_never_fail()
        {
            PluginsManager.Initialize();
            Assert.IsNotNull(PluginsManager.Instance);
            Assert.IsNotNull(PluginsManager.Instance.GetPluginManifests());
            Assert.IsNotNull(PluginsManager.Instance.GetPluginAssemblies());
        }

        [Test, RunInApplicationDomain]
        public void PluginManagerSuccessfullyLoadsPlugins_VarietyOfTests()
        {
            Directory.CreateDirectory(registryDir);

            PluginsManager.Initialize(new FsPluginsProvider(pluginsDir, shadowCopyDir, registryDir));
            Assert.IsNotNull(PluginsManager.Instance);

            var manifests = PluginsManager.Instance.GetPluginManifests();
            var assemblies = PluginsManager.Instance.GetPluginAssemblies();

            Assert.IsNotNull(PluginsManager.Instance.GetPluginManifests());
            Assert.IsNotNull(PluginsManager.Instance.GetPluginAssemblies());

            // there should be 4 plugin manifests
            Assert.That(manifests.Count, Is.EqualTo(4));

            // the plugin manifests should all be inactive
            var activeManifests = PluginsManager.Instance.GetPluginManifests(true);
            Assert.That(activeManifests.Count, Is.EqualTo(0));

            // there should be no assemblies loaded from plugins as none are active
            Assert.That(assemblies.Count, Is.EqualTo(0));
        }

        [Test, RunInApplicationDomain]
        public void PluginManagerSuccessfullyLoadsPlugins_AndMergesWithExistingRegistry()
        {
            // setup existing registry
            Directory.CreateDirectory(registryDir);
            File.WriteAllText(registryFile, @"
<?xml version=""1.0"" encoding=""utf-8""?>
<plugins>
  <plugin pluginId=""MyCompany.MyApp.AwesomePlugin2"" isActivated=""false"" installedOn=""2016-05-01T20:12:04.2112638-05:00"" />
  <plugin pluginId=""MyCompany.MyApp.AwesomePlugin3"" isActivated=""false"" installedOn=""2016-05-01T20:12:04.2112638-05:00"" />
  <plugin pluginId=""MyCompany.MyApp.AwesomePluginBOGUS"" isActivated=""false"" installedOn=""2016-05-01T20:12:04.2112638-05:00"" />
</plugins>
".Trim());

            PluginsManager.Initialize(new FsPluginsProvider(pluginsDir, shadowCopyDir, registryDir));
            Assert.IsNotNull(PluginsManager.Instance);

            var newRegistryFileContents = File.ReadAllText(registryFile);

            // the BOGUS registry item should be gone
            Assert.That(newRegistryFileContents.Contains("AwesomePluginBOGUS"), Is.False);

            // the AwesomePlugin1 item should have been added
            Assert.That(newRegistryFileContents.Contains("AwesomePlugin1"), Is.True);
        }

        [Test, RunInApplicationDomain]
        public void PluginManagerSuccessfullyLoadsPlugins_HasActivationExceptionForUnmetNonOptionalDependencies()
        {
            // setup existing registry
            Directory.CreateDirectory(registryDir);
            File.WriteAllText(registryFile, @"
<?xml version=""1.0"" encoding=""utf-8""?>
<plugins>
  <plugin pluginId=""MyCompany.MyApp.AwesomePlugin1"" isActivated=""true"" installedOn=""2016-05-01T20:12:04.2112638-05:00"" />
  <plugin pluginId=""MyCompany.MyApp.AwesomePlugin2"" isActivated=""true"" installedOn=""2016-05-01T20:12:04.2112638-05:00"" />
  <plugin pluginId=""MyCompany.MyApp.AwesomePlugin3"" isActivated=""false"" installedOn=""2016-05-01T20:12:04.2112638-05:00"" />
  <plugin pluginId=""MyCompany.MyApp.AwesomePluginBOGUS"" isActivated=""false"" installedOn=""2016-05-01T20:12:04.2112638-05:00"" />
</plugins>
".Trim());

            PluginsManager.Initialize(new FsPluginsProvider(pluginsDir, shadowCopyDir, registryDir));
            Assert.IsNotNull(PluginsManager.Instance);

            var allManifests = PluginsManager.Instance.GetPluginManifests();
            Assert.That(allManifests.Count, Is.EqualTo(4));

            // 1 plugin manifests should be active
            var activeManifests = PluginsManager.Instance.GetPluginManifests(true);
            Assert.That(activeManifests.Count, Is.EqualTo(1));

            // Another should have activation exceptions
            var plugin3 = allManifests.Single(m => m.PluginId == "MyCompany.MyApp.AwesomePlugin2");
            Assert.That(plugin3.RuntimeInfo.IsActivated, Is.False);
            Assert.That(plugin3.RuntimeInfo.ActivationExceptions.Count, Is.EqualTo(1));
            Assert.That(plugin3.RuntimeInfo.DependencyExceptionMessages.Count, Is.EqualTo(1));
        }

        [Test, RunInApplicationDomain]
        public void PluginManagerSuccessfullyLoadsPlugins_SuccessfullyLoadsOnlyAssembliesForActiveManifests()
        {
            // setup existing registry
            Directory.CreateDirectory(registryDir);
            File.WriteAllText(registryFile, @"
<?xml version=""1.0"" encoding=""utf-8""?>
<plugins>
  <plugin pluginId=""MyCompany.MyApp.AwesomePlugin1"" isActivated=""true"" installedOn=""2016-05-01T20:12:04.2112638-05:00"" />
  <plugin pluginId=""MyCompany.MyApp.AwesomePlugin2"" isActivated=""true"" installedOn=""2016-05-01T20:12:04.2112638-05:00"" />
  <plugin pluginId=""MyCompany.MyApp.AwesomePlugin3"" isActivated=""true"" installedOn=""2016-05-01T20:12:04.2112638-05:00"" />
  <plugin pluginId=""MyCompany.MyApp.AwesomePluginBOGUS"" isActivated=""false"" installedOn=""2016-05-01T20:12:04.2112638-05:00"" />
</plugins>
".Trim());

            PluginsManager.Initialize(new FsPluginsProvider(pluginsDir, shadowCopyDir, registryDir));
            Assert.IsNotNull(PluginsManager.Instance);

            // 3 plugin manifests should be active
            var allManifests = PluginsManager.Instance.GetPluginManifests();
            Assert.That(allManifests.Count, Is.EqualTo(4));
            var activeManifests = PluginsManager.Instance.GetPluginManifests(true);
            Assert.That(activeManifests.Count, Is.EqualTo(3));
            
            //TOPOLOGICAL SORT TEST
            // because AwesomePlugin2 depends on AwesomePlugin3, AwesomePlugin3 should be before in the order
            Assert.That(activeManifests.IndexOf(m => m.PluginId == "MyCompany.MyApp.AwesomePlugin3"), 
                Is.LessThan(activeManifests.IndexOf(m => m.PluginId == "MyCompany.MyApp.AwesomePlugin2")));

            // there should be 8 plugin assemblies: 
            /*
             *   MyCompany.MyApp.AwesomePlugin1, 
             *   MyCompany.MyApp.AwesomePlugin2, 
             *   MyCompany.MyApp.AwesomePlugin3, 
             *   Dapper, 
             *   Newtonsoft.Json, 
             *   ServiceStack.Text
             *   
             *   NOT log4net (that one is part of AwesomePlugin4)
             */
            var assemblies = PluginsManager.Instance.GetPluginAssemblies(true);
            var pluginAssemblies = PluginsManager.Instance.GetPluginAssemblies(false);
            Assert.That(assemblies.Count, Is.EqualTo(6));
            Assert.That(pluginAssemblies.Count, Is.EqualTo(3));

            // there should be NO log4net in the mix of assemblies
            Assert.That(assemblies.Any(a => a.FullName.IndexOf("log4net", StringComparison.OrdinalIgnoreCase) >= 0), Is.False);

            // finally, make sure the plugin types are correct
            foreach (var m in activeManifests)
            {
                Assert.NotNull(m.RuntimeInfo.Plugin);

                if (m.PluginId.EndsWith("1"))
                    Assert.That(m.RuntimeInfo.PluginType.Name, Is.EqualTo("NonEasyPlugin"));
                if (m.PluginId.EndsWith("2"))
                    Assert.That(m.RuntimeInfo.PluginType.Name, Is.EqualTo("NonEasyPlugin"));
                if (m.PluginId.EndsWith("3"))
                    Assert.That(m.RuntimeInfo.PluginType.Name, Is.EqualTo("EasyPlugin"));
            }
        }
    }
}