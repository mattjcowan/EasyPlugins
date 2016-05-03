using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyPlugins.Utils;
using ServiceStack;
using ServiceStack.Messaging.Rcon;

namespace BootstrapWebApp
{
    /// <summary>
    /// A Ping service, cause you always have to have a ping service, right?
    /// </summary>
    public class PingService: Service
    {
        public object Get(Ping ping)
        {
            return Request.ToOptimizedResult(new PingResponse
            {
                User = base.GetSession().IsAuthenticated ? base.GetSession().UserName: null,
                Server = FsUtils.GetMachineName(),
                ServerTime = DateTime.Now,
                ServerTimeUtc = DateTime.UtcNow
            });
        }
    }

    [Route("/ping")]
    public class Ping: IReturn<PingResponse>
    {
    }

    public class PingResponse
    {
        public Guid RuntimeGuid => AppHostRuntime.RuntimeGuid;

        public string User { get; set; }
        public string Server { get; set; }
        public DateTime ServerTime { get; set; }
        public DateTime ServerTimeUtc { get; set; }
    }
}
