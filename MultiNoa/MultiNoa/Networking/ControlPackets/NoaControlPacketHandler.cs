using MultiNoa.Logging;
using MultiNoa.Networking.PacketHandling;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.ControlPackets
{
    public static class NoaControlPacketHandler
    {
        [MultiNoaInternal]
        [PacketHandler]
        // Client side
        public static class FromServer
        {
            [HandlerMethod(NoaControlPacketIds.FromServer.WelcomePacket)]
            public static void WelcomePacket(NoaControlPackets.FromServer.WelcomePacket welcomePacket, ConnectionBase c)
            {
                if (!welcomePacket.RunningNoaVersion.Equals(MultiNoaSetup.VersionCode))
                {
                    MultiNoaLoggingManager.Logger.Warning($"Different multiNoa-Versions used: server uses '{welcomePacket.RunningNoaVersion}', this client uses '{MultiNoaSetup.VersionCode}'");
                }
                
                if (!c.GetProtocolVersion().Equals(welcomePacket.ProtocolVersion))
                {
                    MultiNoaLoggingManager.Logger.Error($"Distant server and this client are running two different protocol-versions! ({welcomePacket.ProtocolVersion} vs {c.GetProtocolVersion()} -> Disconnecting)");
                    c.Disconnect();
                    return;
                }
                
                MultiNoaLoggingManager.Logger.Verbose($"Server sent Welcome Packet with fitting protocol version");
                
                var wReceived = new NoaControlPackets.FromClient.WelcomeReceived()
                {
                    RunningNoaVersion = MultiNoaSetup.VersionCode,
                    Username = "Dummy" // TODO: Implement support layer for user-names (including name change events)
                };
                c.SendData(PacketConverter.ObjectToByte(wReceived)); // TODO: Shorthand/Abstraction for sending objects instead of bytes.
                
                c.GetClient()?.InvokeOnClientReady();
                c.InvokeOnConnected();
            }
        }
        
        [MultiNoaInternal]
        [PacketHandler]
        // Server side
        public static class FromClient
        {
            [HandlerMethod(NoaControlPacketIds.FromClient.WelcomeReceived)]
            public static void WelcomeReceivedPacket(NoaControlPackets.FromClient.WelcomeReceived welcomeReceived, ConnectionBase c)
            {
                if (!welcomeReceived.RunningNoaVersion.Equals(MultiNoaSetup.VersionCode))
                {
                    MultiNoaLoggingManager.Logger.Warning($"Different multiNoa-Versions used: client uses '{welcomeReceived.RunningNoaVersion}', this server uses '{MultiNoaSetup.VersionCode}'");
                }
                
                c.GetClient()?.InvokeOnClientConnected();
                c.InvokeOnConnected();
            }
        }
    }
}