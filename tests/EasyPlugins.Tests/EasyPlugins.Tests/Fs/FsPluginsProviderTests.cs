using NUnit.Framework;
using EasyPlugins.Fs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyPlugins.Utils;

namespace EasyPlugins.Fs
{
    [TestFixture()]
    public class FsPluginsProviderTests
    {
        private FsPluginsProvider _provider = null;

        private void ClearDirectories()
        {
            FsUtils.DeleteDirectory(FsUtils.MapPath(FsConstants.DefaultAppPluginsVirtualPath), true);
            FsUtils.DeleteDirectory(FsUtils.MapPath(FsConstants.DefaultAppPluginsRegistrarVirtualPath), true);
            FsUtils.DeleteDirectory(FsUtils.MapPath(FsConstants.DefaultAppPluginsShadowCopyVirtualPath), true);
        }

        [SetUp]
        public void SetUp()
        {
            ClearDirectories();
            _provider = new FsPluginsProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider = null;
            ClearDirectories();
        }

        [Test()]
        public void FsPluginsProvider_ConstructorDoesNotCreateDirectories()
        {
            // Just creating the plugins provider does not create the directories
            Assert.That(Directory.Exists(_provider.PluginsDirectoryPath), Is.False);
            Assert.That(Directory.Exists(_provider.ShadowCopyDirectoryPath), Is.False);
        }

        [Test()]
        public void FsPluginsProvider_InitializeCreatesDirectories()
        {
            _provider.Initialize(new List<PluginManifest>());
            
            // Just creating the plugins provider does not create the directories
            Assert.That(Directory.Exists(_provider.PluginsDirectoryPath), Is.True);
            Assert.That(Directory.Exists(_provider.ShadowCopyDirectoryPath), Is.True);
        }

        //[Test()]
        //public void FsPluginsProvider_ExtractionOfPluginWorksForBothArchiveWithRootDir()
        //{
        //    _provider.Initialize(new List<PluginManifest>());
            
        //    Assert.That(Directory.Exists(_provider.PluginsDirectoryPath), Is.True);
        //    Assert.That(Directory.Exists(_provider.ShadowCopyDirectoryPath), Is.True);
        //}
    }
}