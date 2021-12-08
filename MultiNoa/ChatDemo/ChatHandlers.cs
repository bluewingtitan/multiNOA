using System;
using MultiNoa.Logging;
using MultiNoa.Networking.PacketHandling;
using MultiNoa.Networking.Transport;

namespace ChatDemo
{
    [PacketHandler]
    public static class ChatHandlers
    {

        // Runs Server Side
        [HandlerMethod(0)]
        public static void HandleMessageSend(ChatPackets.FromClient.MessageFromClient m, ConnectionBase c)
        {
            var client = c.GetClient();
            
            var answer = new ChatPackets.FromServer.MessageFromServer
            {
                Message = m.Message,
                Username = client.Username
            };
            
            MultiNoaLoggingManager.Logger.Information($"[{client.GetRoom().GetRoomName()}] <{client.Username}> {m.Message}");
            
                
            client.GetRoom().Broadcast(answer, client);
        }
        
        
        // Client Side
        [HandlerMethod(1)]
        public static void HandleMessageReceived(ChatPackets.FromServer.MessageFromServer m)
        {
            Console.WriteLine($"<{m.Username}> {m.Message}");
            Console.Out.Flush();
        }
    }
}