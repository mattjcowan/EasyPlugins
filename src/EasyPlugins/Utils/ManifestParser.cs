using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace EasyPlugins.Utils
{


    /*
    SAMPLE JSON
    {
        "PluginId": "MyCompany.MyApp.AwesomePlugin1",
        "PluginTypeName": "MyCompany.MyApp.AwesomePlugin1, MyCompany.MyApp",
        "PluginAssemblyName": "MyCompany.MyApp.dll",
        "PluginTitle": "My awesome plugin 1",
        "PluginDescription": "An awesome plugin for my awesome app",
        "PluginUrl": "http://github.com/mycompany/MyApp.AwesomePlugin1",
        "PluginVersion": "1.0.5",
        "PluginDefaultSettings": [
            { "Key": "ABC", "Value": "DEF1" },
            { "Key": "ABCD", "Value": "DEF2" },
            { "Key": "ABCDE", "Value": "DEF3" },
            { "Key": "ABCDEF", "Value": "DEF4" }
        ],
        "PluginCategory": "MyApp",
        "PluginTags": ["MyCompany", "MyApp", "AwesomePlugin1" ],
        "PluginDependencies": [
            { "PluginId": "MyCompany.MyApp.AnotherPlugin1", "MinVersion": "1.0.0", "MaxVersion": "5.0.0", "Optional": true },
            { "PluginId": "MyCompany.MyApp.AnotherPlugin2", "MinVersion": "1.0.0", "Optional": true },
            { "PluginId": "MyCompany.MyApp.AnotherPlugin3", "MaxVersion": "5.0.0", "Optional": false },
            { "PluginId": "MyCompany.MyApp.AnotherPlugin4", "MinVersion": "1.0.0", "MaxVersion": "5.0.0" },
            { "PluginId": "MyCompany.MyApp.AnotherPlugin5" }
        ],
        "Author": "@mattjcowan",
        "AuthorUrl": "http://www.mattjcowan.com",
        "License": "MIT",
        "LicenseUrl": "http://path_to_license_url"
    }
    */
    internal class JsonManifestParser
    {
        public static PluginManifest Parse(string manifestJson)
        {
            if (string.IsNullOrEmpty(manifestJson))
                return null;

            // Make a stream to read from.
            JsonManifest jm = null;
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(manifestJson);
                    writer.Flush();
                    stream.Position = 0;
                    // Deserialize from the stream.
                    var serializer = new DataContractJsonSerializer(typeof(JsonManifest));
                    jm = (JsonManifest)serializer.ReadObject(stream);
                }
            }

            var pluginManifest = new PluginManifest();
            Populate(pluginManifest, jm);
            return pluginManifest;
        }

        private static void Populate(PluginManifest pm, JsonManifest jm)
        {
            pm.PluginId = jm.PluginId;
            pm.PluginTypeName = jm.PluginTypeName;
            pm.PluginTitle = jm.PluginTitle;
            pm.PluginDescription = jm.PluginDescription;
            pm.PluginUrl = jm.PluginUrl;
            pm.PluginVersion = ParseToVersion(jm.PluginVersion);
            pm.PluginCategory = jm.PluginCategory;
            pm.Author = jm.Author;
            pm.AuthorUrl = jm.AuthorUrl;
            pm.License = jm.License;
            pm.LicenseUrl = jm.LicenseUrl;

            var tags = (jm.PluginTags ?? new string[0]).ToList();
            if (tags.Count > 0)
                pm.PluginTags.AddRange(tags);

            var settings = ParseToDictionary(jm.PluginDefaultSettings);
            if (settings.Count > 0)
                pm.PluginDefaultSettings.AddRange(settings);

            var dependencies = ParseToDependencies(jm.PluginDependencies);
            if (dependencies.Count > 0)
                pm.PluginDependencies.AddRange(dependencies);
        }

        private static List<PluginDependency> ParseToDependencies(JsonDependency[] pluginDependencies)
        {
            if (pluginDependencies == null)
                return new List<PluginDependency>();

            return pluginDependencies.Select(d => new PluginDependency
            {
                PluginId = d.PluginId,
                MinVersion = ParseToVersion(d.MinVersion),
                MaxVersion = ParseToVersion(d.MaxVersion),
                IsOptional = d.Optional
            }).ToList();
        }

        private static Dictionary<string, string> ParseToDictionary(JsonDefaultSetting[] pluginDefaultSettings)
        {
            if(pluginDefaultSettings == null)
                return new Dictionary<string, string>();

            return pluginDefaultSettings.ToDictionary(k => k.Key, v => v.Value);
        }

        private static Version ParseToVersion(string v)
        {
            Version v1;
            if (Version.TryParse(v, out v1))
                return v1;
            return null;
        }

        public class JsonManifest
        {
            public string PluginId { get; set; }
            public string PluginTypeName { get; set; }
            public string PluginTitle { get; set; }
            public string PluginDescription { get; set; }
            public string PluginUrl { get; set; }
            public string PluginVersion { get; set; }
            public JsonDefaultSetting[] PluginDefaultSettings { get; set; }
            public string PluginCategory { get; set; }
            public string[] PluginTags { get; set; }
            public JsonDependency[] PluginDependencies { get; set; }
            public string Author { get; set; }
            public string AuthorUrl { get; set; }
            public string License { get; set; }
            public string LicenseUrl { get; set; }
        }

        public class JsonDefaultSetting
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public class JsonDependency
        {
            public string PluginId { get; set; }
            public string MinVersion { get; set; }
            public string MaxVersion { get; set; }
            public bool Optional { get; set; }
        }
    }
}
