using ExampleProject.Packets;
using MultiNoa.Logging;
using MultiNoa.Networking.PacketHandling;
using MultiNoa.Networking.Transport;

namespace ExampleProject
{
    [PacketHandler]
    static class Handler
    {
        [HandlerMethod(PacketId.Message)]
        public static void HandleMessage(Message m, ConnectionBase connection)
        {
            MultiNoaLoggingManager.Logger.Information("Handled Message.");
            MultiNoaLoggingManager.Logger.Information("Message Information: " + m.ToString());
        }
    }
}