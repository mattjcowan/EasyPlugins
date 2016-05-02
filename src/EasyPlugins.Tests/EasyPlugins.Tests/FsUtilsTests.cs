using System;
using System.IO;
using NUnit.Framework;

namespace EasyPlugins.Tests
{
    [TestFixture()]
    public class FsUtilsTests
    {
        [Test()]
        public void NormalizePathTest()
        {
            var cd = Environment.CurrentDirectory;

            var path1 = FsUtils.NormalizePath("");
            var path2 = FsUtils.NormalizePath("~");
            var path3 = FsUtils.NormalizePath("~/");
            var path4 = FsUtils.NormalizePath(@"~\");
            var path5 = FsUtils.NormalizePath(cd);
            var path6 = FsUtils.NormalizePath("~/test");
            var path7 = FsUtils.NormalizePath("~/test\\abc/ok/now\\this");

            Assert.That(path1, Is.SamePath(cd));
            Assert.That(path2, Is.SamePath(cd));
            Assert.That(path3, Is.SamePath(cd));
            Assert.That(path4, Is.SamePath(cd));
            Assert.That(path5, Is.SamePath(cd));
            Assert.That(path6, Is.SamePath(Path.Combine(cd, "test")));
            Assert.That(path6, Is.SamePath(Path.Combine(cd, "test\\abc\\ok\\now\\this")));
        }

        [Test()]
        public void AppDataFolderTests()
        {
            var path1 = FsUtils.GetEasyPluginsAppDataFolder("");
            var path2 = FsUtils.GetEasyPluginsAppDataFolder("bin");

            Assert.That(path1, Is.SamePath("c:\\programdata\\easyplugins\\"));
            Assert.That(path2, Is.SamePath("c:\\programdata\\easyplugins\\bin"));
        }
    }
}