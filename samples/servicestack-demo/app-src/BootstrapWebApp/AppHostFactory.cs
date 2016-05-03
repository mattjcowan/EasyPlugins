using System;
using System.Web.Hosting;
using BootstrapWebApp;
using Funq;
using ServiceStack;
using ServiceStack.Text;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(AppHostFactory), "Start", Order = 1)]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(AppHostFactory), "Stop", Order = 1)]

namespace BootstrapWebApp
{
    public static class AppHostFactory
    {
        private static readonly Lazy<ServiceStackHost> AppHost =
            new Lazy<ServiceStackHost>(() => HostingEnvironment.IsHosted
                ? new IisHostedAppHost(AppHostRuntime.Instance) as ServiceStackHost
                : new SelfHostedAppHost(AppHostRuntime.Instance) as ServiceStackHost);

        public static void Start()
        {
            var selfHost = AppHost.Value.Init() as AppSelfHostBase;
            if (selfHost == null) return;

            var port = AppHostRuntime.Instance.GetApplicationPort();
            selfHost.Start("http://*:{0}/".FormatWith(port));
            "ServiceStack SelfHost listening at http://localhost:{0}".FormatWith(port).Print();
        }

        public static void Stop()
        {
            var appHost = AppHost.Value;
            (appHost as AppSelfHostBase)?.Stop();
            AppHost.Value.Dispose();
        }
    }

    public class IisHostedAppHost : AppHostBase
    {
        private readonly AppHostRuntime _instance;

        public IisHostedAppHost(AppHostRuntime instance) : base(instance.GetApplicationName(), instance.GetAssembliesWithServices())
        {
            _instance = instance;
        }

        public override void Configure(Container container)
        {
            _instance.ConfigureAppHost(this, container, new HostConfig());
        }
    }

    public class SelfHostedAppHost : AppSelfHostBase
    {
        private readonly AppHostRuntime _instance;

        public SelfHostedAppHost(AppHostRuntime instance) : base(instance.GetApplicationName(), instance.GetAssembliesWithServices())
        {
            _instance = instance;
        }

        public override void Configure(Container container)
        {
            //Uncomment to change the default ServiceStack configuration
            var config = new HostConfig
            {
                WebHostUrl = "/",
                WebHostPhysicalPath = "~/..".MapServerPath() // running out of app\bin directory (go UP one to parent dir)
            };
            _instance.ConfigureAppHost(this, container, config);
        }
    }
}
