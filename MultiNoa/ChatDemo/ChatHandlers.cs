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
        public static void HandleMessageSend(ChatPackets.FromClient.MessageFromClient m, IConnection c)
        {
            MultiNoaLoggingManager.Logger.Information("Received Message:\n" + m.Message);
            
            var answer = new ChatPackets.FromServer.MessageFromServer
            {
                Message = "Hello Client,\nhow are you doing?\nIs the weather as sunny as it is here?\nAre those weird symbols transfered correctly?\n¸´ÜÈ^°\\\"@\n´o`"
            };
            c.SendData(PacketConverter.ObjectToByte(answer));
        }
        
        
        // Client Side
        [HandlerMethod(1)]
        public static void HandleMessageReceived(ChatPackets.FromServer.MessageFromServer m)
        {
            MultiNoaLoggingManager.Logger.Information("Received Message:\n" + m.Message);
        }
    }
}