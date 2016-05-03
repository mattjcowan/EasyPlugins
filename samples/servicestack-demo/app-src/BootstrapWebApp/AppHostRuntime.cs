using System;
using System.Reflection;
using Funq;
using ServiceStack;
using ServiceStack.Text;

namespace BootstrapWebApp
{
    public class AppHostRuntime
    {
        private static readonly Lazy<AppHostRuntime> Runtime = new Lazy<AppHostRuntime>(() => new AppHostRuntime());
        public static readonly Guid RuntimeGuid = Guid.NewGuid();

        public AppHostRuntime()
        {
            Warmup();
        }

        private void Warmup()
        {
            //Set JSON web services to return idiomatic JSON camelCase properties, and more
            JsConfig.DateHandler = DateHandler.ISO8601;
            JsConfig.TimeSpanHandler = TimeSpanHandler.DurationFormat;
            JsConfig.EmitCamelCaseNames = true;
            JsConfig.IncludeTypeInfo = true;
            JsConfig<Version>.SerializeFn = version => version.ToString();
            JsConfig<Version>.DeSerializeFn = Version.Parse;

            //todo: do anything here that needs to happen before the HttpApplication is initialized
        }

        public static AppHostRuntime Instance => Runtime.Value;

        public string GetApplicationName()
        {
            return "My Web App";
        }

        public int GetApplicationPort()
        {
            return 8098;
        }

        public Assembly[] GetAssembliesWithServices()
        {
            var thisAssembly = typeof(AppHostRuntime).Assembly;
            return new Assembly[] {thisAssembly};
        }

        public void ConfigureAppHost(ServiceStackHost appHost, Container container, HostConfig config)
        {
            config.EmbeddedResourceSources.Add(typeof(AppHostRuntime).Assembly);
            config.AllowFileExtensions.Add("tag"); // for RiotJS
            appHost.SetConfig(config);

            //Add some default plugins
            appHost.Plugins.Add(new CorsFeature());
            appHost.Plugins.Add(new PostmanFeature());
        }
    }
}