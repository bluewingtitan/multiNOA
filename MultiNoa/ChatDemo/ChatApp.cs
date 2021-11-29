using System;
using System.Net;
using System.Threading;
using MultiNoa;
using MultiNoa.GameSimulation;
using MultiNoa.Logging;
using MultiNoa.Networking.PacketHandling;
using MultiNoa.Networking.Server;
using MultiNoa.Networking.Transport;
using MultiNoa.Networking.Transport.Connection;

namespace ChatDemo
{
    /// <summary>
    /// A simple demo, where a server and a client are started, and are interchanging basic messsages.
    /// With the built API in here, one could build an entire basic chatroom without writing any code towards networking other than the one allready there.
    ///
    /// It's a demonstration of the simplicity multiNoa allows.
    /// </summary>
    internal static class ChatApp
    {
        private const ushort ServerPort = 25511;
        private static NoaTcpServer _server;
        private static TcpConnection connection;
        private static DynamicThread _thread = new DynamicThread(5, "Client");
        
        private static void Main(string[] args)
        {
            MultiNoaSetup.DefaultSetup(typeof(ChatApp).Assembly);
            
            // => Start Server
            MultiNoaLoggingManager.Logger.Information("Starting Server...");
            _server = new NoaTcpServer(ServerPort, "demo", 5, "Chat Server");
            
            // Make sure the server is fully up before trying to connect.
            Thread.Sleep(500);
            
            // => Start Client
            MultiNoaLoggingManager.Logger.Information("Starting Client...");
            connection = new TcpConnection(() => MultiNoaLoggingManager.Logger.Error("Connection to server was closed."));
            connection.Connect("127.0.0.1", ServerPort);
            connection.ChangeThread(_thread);

            Thread.Sleep(500);
            var message = new ChatPackets.FromClient.MessageFromClient
            {
                Message = "Hello Server!"
            };
            connection.SendData(PacketConverter.ObjectToByte(message));
        }
    }
}