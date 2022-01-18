using System;
using MultiNoa.Logging;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.PacketHandling;
using MultiNoa.Networking.Transport;

namespace ChatDemo
{
    [PacketHandler]
    public static class ChatHandlers
    {

        // Runs Server Side
        [HandlerMethod(ChatPackets.FromClient.MessageFromClient.PacketId)]
        public static void HandleMessageSend(ChatPackets.FromClient.MessageFromClient m, ConnectionBase c)
        {
            var client = (IServersideClient) c.GetClient();
            
            var answer = new ChatPackets.FromServer.MessageFromServer
            {
                Message = m.Message,
                Username = client.Username
            };
            
            MultiNoaLoggingManager.Logger.Information($"[{client.Room.GetRoomName()}] <{client.Username}> {m.Message}");
            
            client.Room.Broadcast(answer, client);
        }
        
        
        // Client Side
        [HandlerMethod(ChatPackets.FromServer.MessageFromServer.PacketId)]
        public static void HandleMessageReceived(ChatPackets.FromServer.MessageFromServer m)
        {
            MultiNoaLoggingManager.Logger.Information("HIIII");
            Console.WriteLine($"<{m.Username}> {m.Message}");
            Console.Out.Flush();
        }
    }
}