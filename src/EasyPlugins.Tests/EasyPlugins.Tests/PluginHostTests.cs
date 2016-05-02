using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyPlugins.Fsi;
using NUnit.Framework.Internal;
using NUnit.Framework;

namespace EasyPlugins.Tests
{
    [TestFixture]
    public class PluginHostTests
    {
        static string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string pluginsDir = Path.Combine(currentDir, "..", "..", "TestPlugins");
        string binDir = Path.Combine(currentDir, "..", "..", "TestBin");

        [SetUp]
        public void SetUp()
        {
            PluginsManager.Initialize(new FileSystemPluginHostFactory(config =>
            {
                config.PluginsDirectoryPath = pluginsDir;
                config.ShadowCopyDirectoryPath = binDir;
            }));
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(binDir);
        }

        [Test, RunInApplicationDomain]
        public void Test123()
        {
            Assert.LessOrEqual(0, PluginsManager.Instance.InitializationDuration);

            var assemblies = PluginsManager.Instance.GetPluginAssemblies();
            Assert.That(0, Is.LessThan(assemblies.Length));

            var manifests = PluginsManager.Instance.GetPluginManifests();
            Assert.That(0, Is.LessThan(manifests.Length));
        }
    }
}
