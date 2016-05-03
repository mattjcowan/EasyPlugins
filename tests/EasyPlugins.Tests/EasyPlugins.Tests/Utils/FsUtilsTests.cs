using NUnit.Framework;
using EasyPlugins.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlugins.Utils
{
    [TestFixture()]
    public class FsUtilsTests
    {
        [Test()]
        public void MapPath_VarietyOfTests()
        {
            var path = FsUtils.GetAppDomainDirectory();

            Assert.That(path, Is.SamePath(AppDomain.CurrentDomain.BaseDirectory));

            var pathTests = new List<string>
            {
                "~/plugins/test",
                "plugins\\test",
                "plugins//test",
                "\\plugins//test",
                "plugins/test\\",
                "~\\plugins/test\\",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins\\test")
            };

            var shouldAllResolveToDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins\\test");

            foreach (var pathTest in pathTests)
            {
                Assert.That(FsUtils.MapPath(pathTest), Is.SamePath(shouldAllResolveToDirectoryPath),
                    "Failed with " + pathTest);
            }
        }

        [Test()]
        public void RestartHostedAppDomain_ShouldModifyWebConfig_Up3LevelsFromBaseDirectoryUntilFound()
        {
            var webConfigFile = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "web.config");

            var modTime = File.GetLastWriteTime(webConfigFile);
            FsUtils.RestartHostedAppDomain();
            var modTime2 = File.GetLastWriteTime(webConfigFile);

            Assert.That(modTime2, Is.GreaterThan(modTime));
        }
    }
}