using System;
using System.Reflection;
using MultiNoa;
using MultiNoa.Logging;
using MultiNoa.Networking.Server;
using MultiNoa.Networking.Transport.Middleware;
using MultiNoa.Networking.Transport.Middleware.Fragmentation;

namespace ChatDemo
{
    /// <summary>
    /// A simple demo, where a server and a client are started, and are interchanging basic messsages.
    /// With the built API in here, one could build an entire basic chatroom without writing any code towards networking other than the one allready there.
    ///
    /// It's a demonstration of the simplicity multiNoa allows.
    ///
    ///
    /// It's running at 5 ticks per second (more is definitely not needed for a chat app...),
    /// so it might appear slower, as it's waiting for the next tick on the server, then for the next tick on the client too.
    /// </summary>
    internal static class ChatApp
    {
        public const ushort ServerPort = 25511;
        public const string ProtocolVersion = "demoV1";
        
        private static NoaTcpServer _server;

        private static void Main(string[] args)
        {
            MultiNoaSetup.CustomSetup(
                new MultiNoaConfig
                {
                    MainAssembly = typeof(ChatClient).Assembly,
                    ExtraAssemblies = new Assembly[0],
                    Middlewares = new INoaMiddleware[] {new NoaNetworkLoggingMiddleware(), new NoaRsaMiddleware(), new NoaFragmentationMiddleware(),  }
                });

            if (args.Length > 0)
            {
                // => Start Server
                MultiNoaLoggingManager.Logger.Information("Starting Server...");
                _server = new NoaTcpServer(ServerPort, ProtocolVersion, 5, "Chat Server");

            }
            else
            {
                // No debug messages!
                //MultiNoaLoggingManager.Logger = new MultiNoaLoggingManager.NoaNoLogger();

                Console.Write("Username: ");
                var username = Console.ReadLine();
                var client = new ChatClient(username, "127.0.0.1");
            }
        }
    }


}