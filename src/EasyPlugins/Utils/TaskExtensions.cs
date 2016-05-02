using System;
using System.Linq;
using System.Threading.Tasks;

namespace EasyPlugins.Utils
{
    public delegate Task AsyncEventHandler<TEventArgs>(
        object sender,
        TEventArgs e)
        where TEventArgs : EventArgs;

    public static class TaskExtensions
    {
        public static Task[] InvokeAll<TEventArgs>(
            this AsyncEventHandler<TEventArgs> handler,
            object sender,
            TEventArgs e)
            where TEventArgs : EventArgs
            => (
                from AsyncEventHandler<TEventArgs> h in handler.GetInvocationList()
                select h(sender, e)).ToArray();

        // Convenience
        public static Task WhenInvokeAll<TEventArgs>(
            this AsyncEventHandler<TEventArgs> handler,
            object sender,
            TEventArgs e)
            where TEventArgs : EventArgs
            => Task.WhenAll(handler.InvokeAll(sender, e));
    }
}
