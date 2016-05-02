using System;
using System.IO;
using System.Reflection;
using EasyPlugins.Fsi;
using EasyPlugins.Mock;
using NUnit.Framework;

namespace EasyPlugins.Tests
{
    [TestFixture]
    public class PluginManagerTests
    {
        public static string TestBaseDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "test123");

        [TearDown]
        public void TearDownTest()
        {
            if (Directory.Exists(TestBaseDir))
            {
                Directory.Delete(TestBaseDir, true);
            }
        }

        [Test, RunInApplicationDomain]
        public void PluginManagerAccessedBeforeInitializationShouldFail()
        {
            try
            {
                var instance = PluginsManager.Instance;
                Assert.Fail(
                    "Accessing the plugin manager instance before it is initialized should generate an exception");
            }
            catch (Exception ex)
            {
                var epx = ex as EPException;
                Assert.IsNotNull(epx, "Wrong exception type: " + ex.GetType());
                Assert.AreEqual(ErrorCode.NotInitialized, epx.ErrorCode);
            }
        }

        [Test, RunInApplicationDomain]
        public void PluginManagerInitializationAfterAlreadyInitializedShouldFail()
        {
            try
            {
                PluginsManager.Initialize(new MockHostFactory());
                PluginsManager.Initialize(new MockHostFactory());
                Assert.Fail("Calling the plugin manager initialization twice should generate an exception");
            }
            catch (Exception ex)
            {
                var epx = ex as EPException;
                Assert.IsNotNull(epx, "Wrong exception type: " + ex.GetType());
                Assert.AreEqual(ErrorCode.AlreadyInitialized, epx.ErrorCode);
            }
        }
        
        [Test, RunInApplicationDomain]
        public void InitializingThePluginHostWillCreateSomeDefaultPluginDirectories()
        {
            PluginsManager.Initialize(new FileSystemPluginHostFactory(config =>
            {
                config.PluginsDirectoryPath = Path.Combine(TestBaseDir, "plugins");
                config.ShadowCopyDirectoryPath = Path.Combine(TestBaseDir, "bin");
            }));

            Assert.IsTrue(Directory.Exists(TestBaseDir));
            Assert.IsTrue(Directory.Exists(Path.Combine(TestBaseDir, "plugins")));
            Assert.IsTrue(Directory.Exists(Path.Combine(TestBaseDir, "bin")));

        }

    }
}
