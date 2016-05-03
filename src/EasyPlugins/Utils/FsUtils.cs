using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace EasyPlugins.Utils
{
    public static class FsUtils
    {
        public static void DeleteDirectory(string path, bool recurse = false)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recurse);
            }
            catch (IOException)
            {
                Thread.Sleep(100);
                Directory.Delete(path, recurse);
            }
            catch (UnauthorizedAccessException)
            {
                Thread.Sleep(100);
                try
                {
                    Directory.Delete(path, recurse);
                }
                catch
                {
                    // Delete all files in the directory
                    foreach (var file in Directory.GetFiles(path))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        public static bool AreIdenticalPaths(DirectoryInfo dir1, DirectoryInfo dir2)
        {
            return string.Compare(
                dir1.FullName.TrimEnd('/', '\\'),
                dir2.FullName.TrimEnd('/', '\\'),
                StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static string MapPath(string virtualPath)
        {
            if (Directory.Exists(virtualPath) || File.Exists(virtualPath))
                return virtualPath;

            if (HostingEnvironment.IsHosted && virtualPath.StartsWith("~"))
            {
                // always resolve hosted path mapping on directories with '/' path separators
                return HostingEnvironment.MapPath(virtualPath.Replace(Path.DirectorySeparatorChar, '/'));
            }

            virtualPath = NormalizeDirectorySeparatorChars(virtualPath);

            var appDomainDirectory = GetAppDomainDirectory();
            if(virtualPath.StartsWith(appDomainDirectory, StringComparison.OrdinalIgnoreCase))
                return virtualPath;

            return CombinePaths(GetAppDomainDirectory(), virtualPath);
        }

        /*
         * 
        var x = new List<string[]> {
		    new[] { "~/", "", "abc", @"\def", @"def\/", "def//\\/" },
		    new[] { "x", @"~\xy", @"\g/" },
		    new[] { "\\x", @"~\xy", @"\g" },
		    new[] { "//x", @"~\xy", @"\g/" },
		    new[] { @"\\documents\", @"~\xy", @"\\g/" },
            new[] { "~/" },
	    };
	    foreach (var y in x)
	    {
		    Console.WriteLine(CombinePaths(y));
	    }
        
        // produces
        \abc\def\def\def\
        x\xy\g\
        \x\xy\g
        \\x\xy\g\
        \\documents\xy\g\
        \

        */
        public static string CombinePaths(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return string.Empty;

            var first = paths[0].Replace("/", @"\").Trim();
            var last = paths[paths.Length - 1].Replace("/", @"\").Trim();

            var path = string.Join(@"\", paths.Select(p => p.Trim().Trim('~', '/', '\\')).ToArray());

            while (path.Contains(@"\\"))
                path = path.Replace(@"\\", @"\");

            while (first.StartsWith(@"\"))
            {
                first = first.Substring(1);
                path = @"\" + path;
            }

            if (last.EndsWith(@"\"))
                path += @"\";

            return NormalizeDirectorySeparatorChars(path);
        }

        public static string NormalizeDirectorySeparatorChars(string path)
        {
            return Path.DirectorySeparatorChar == '/' ? path.Replace('\\', '/') : path.Replace('/', '\\');
        }

        public static string GetMachineName()
        {
            string machineName;
            try
            {
                machineName = Environment.MachineName;
            }
            catch
            {
                try
                {
                    machineName = Dns.GetHostName();
                }
                catch
                {
                    throw new ApplicationException("Unable to retrieve the machine name");
                }
            }
            return machineName.ToAlphaNumeric('-', '-');
        }

        public static AppDomain GetAppDomain()
        {
            return AppDomain.CurrentDomain;
        }

        public static string GetAppDomainDirectory()
        {
            //return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)
            return GetAppDomain().BaseDirectory;
        }

        public static string GetAppVirtualPath()
        {
            return HostingEnvironment.IsHosted ? HostingEnvironment.ApplicationVirtualPath : "/";
        }

        public static string GetAppPhysicalPath()
        {
            return MapPath("~/");
        }

        public static string GetAppBinDirectory()
        {
            return HostingEnvironment.IsHosted ? HttpRuntime.BinDirectory :
                GetAppDomainDirectory();
        }

        public static void RestartHostedAppDomain()
        {
            if (GetTrustLevel() > AspNetHostingPermissionLevel.Medium)
            {
                //full trust
                HttpRuntime.UnloadAppDomain();
                TryWriteWebConfig();
                TryWriteGlobalAsax();
            }
            else
            {
                //medium trust
                bool success = TryWriteWebConfig();
                if (!success)
                {
                    throw new ApplicationException(
                        "The application needs needs to be restarted due to a configuration change, but was unable to do so." +
                        Environment.NewLine +
                        "To prevent this issue in the future, a change to the web server configuration is required:" +
                        Environment.NewLine +
                        "- run the application in a full trust environment, or" + Environment.NewLine +
                        "- give the application write access to the 'web.config' file.");
                }

                success = TryWriteGlobalAsax();
                if (!success)
                {
                    throw new ApplicationException(
                        "The application needs to be restarted due to a configuration change, but was unable to do so." +
                        Environment.NewLine +
                        "To prevent this issue in the future, a change to the web server configuration is required:" +
                        Environment.NewLine +
                        "- run the application in a full trust environment, or" + Environment.NewLine +
                        "- give the application write access to the 'Global.asax' file.");
                }
            }
        }

        #region Private methods

        private static AspNetHostingPermissionLevel? _trustLevel = null;

        /// <summary>
        /// Finds the trust level of the running application (http://blogs.msdn.com/dmitryr/archive/2007/01/23/finding-out-the-current-trust-level-in-asp-net.aspx)
        /// </summary>
        /// <returns>The current trust level.</returns>
        private static AspNetHostingPermissionLevel GetTrustLevel()
        {
            if (!_trustLevel.HasValue)
            {
                //set minimum
                _trustLevel = AspNetHostingPermissionLevel.None;

                //determine maximum
                foreach (var trustLevel in
                    new[]
                    {
                        AspNetHostingPermissionLevel.Unrestricted,
                        AspNetHostingPermissionLevel.High,
                        AspNetHostingPermissionLevel.Medium,
                        AspNetHostingPermissionLevel.Low,
                        AspNetHostingPermissionLevel.Minimal
                    })
                {
                    try
                    {
                        new AspNetHostingPermission(trustLevel).Demand();
                        _trustLevel = trustLevel;
                        break; //we've set the highest permission we can
                    }
                    catch (System.Security.SecurityException)
                    {
                        continue;
                    }
                }
            }
            return _trustLevel.Value;
        }

        private static bool TryWriteWebConfig()
        {
            try
            {
                //Do an explicit write
                var webConfigFile = MapPath("~/web.config");
                if (!File.Exists(webConfigFile))
                {
                    webConfigFile = MapPath("~/../web.config");
                    if (!File.Exists(webConfigFile))
                    {
                        webConfigFile = MapPath("~/../../web.config");
                        if (!File.Exists(webConfigFile))
                            return false;
                    }
                }
                var contents = File.ReadAllText(webConfigFile);
                contents = contents.Substring(0, contents.IndexOf("</configuration>", StringComparison.OrdinalIgnoreCase) + 16) + "\n<!-- restart at " + DateTime.Now.ToString("u") + " -->";
                File.WriteAllText(webConfigFile, contents);
                //File.SetLastWriteTimeUtc(webConfigFile, DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryWriteGlobalAsax()
        {
            try
            {
                var asaxFile = MapPath("~/global.asax");
                if (!File.Exists(asaxFile))
                {
                    asaxFile = MapPath("~/../global.asax");
                    if (!File.Exists(asaxFile))
                    {
                        asaxFile = MapPath("~/../../global.asax");
                        if (!File.Exists(asaxFile))
                            return false;
                    }
                }
                File.SetLastWriteTimeUtc(asaxFile, DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        public static void MoveDirectory(string sourceDirName, string destDirName, bool throwOnException = false, int attempts = 1)
        {
            try
            {
                Directory.Move(sourceDirName, destDirName);
            }
            catch
            {
                if (attempts > 1)
                {
                    Thread.Sleep(100);
                    MoveDirectory(sourceDirName, destDirName, throwOnException, attempts - 1);
                }
                else
                {
                    if (throwOnException)
                        throw;
                }
            }
        }
    }
}
