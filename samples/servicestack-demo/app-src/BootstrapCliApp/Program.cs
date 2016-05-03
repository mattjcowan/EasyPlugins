using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BootstrapCliApp
{
    class Program
    {
        private static FileSystemWatcher _fs = null;
        private static bool _quit = false;

        static void Main(string[] args)
        {
            // add a file system watcher to detect changes to the web.config file
            var directoryToWatch =
                new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.FullName;
            _fs = new FileSystemWatcher
            {
                Path = directoryToWatch,
                //Filter = "*.config",
                IncludeSubdirectories = false
            };
            _fs.Created += ChangesDetected;
            _fs.Deleted += ChangesDetected;
            _fs.Changed += ChangesDetected;
            _fs.Renamed += ChangesDetected;
            _fs.EnableRaisingEvents = true;

            Console.WriteLine("Watching directory '" + directoryToWatch + "' for changes");

            // start the application (in order not to be redundant, we'll re-use the same application host
            // boostrapping mechanism as the web version)
            WebActivatorEx.ActivationManager.RunPreStartMethods();

            while (true)
            {
                if (_quit) break;
                Thread.Sleep(250);
            }

            WebActivatorEx.ActivationManager.RunShutdownMethods();
        }

        private static void ChangesDetected(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Detected change: " + e.Name + " (" + e.ChangeType + ")");
            if (e.Name.Equals("web.config", StringComparison.OrdinalIgnoreCase))
            {
                _fs.EnableRaisingEvents = false;
                WebActivatorEx.ActivationManager.RunShutdownMethods();
                _quit = true;
            }
        }
    }
}
