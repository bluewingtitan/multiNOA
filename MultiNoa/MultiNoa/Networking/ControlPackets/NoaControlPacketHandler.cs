using MultiNoa.Logging;
using MultiNoa.Networking.Client;
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
                    Username = c.GetClient().GetUsername()
                };
                c.SendData(wReceived);
                
                c.GetClient()?.InvokeOnClientReady();
                c.InvokeOnConnected();
            }


            [HandlerMethod(NoaControlPacketIds.FromServer.ConnectionEstablishedPacket)]
            public static void ConnectionEstablished(ConnectionBase c)
            {
                c.InvokeOnConnected();
            }
            

            [HandlerMethod(NoaControlPacketIds.FromServer.SyncUsername)]
            public static void SyncUsername(NoaControlPackets.FromServer.SyncUsername s, ConnectionBase c)
            {
                var client = c.GetClient();
                if (client == null) return;
                
                MultiNoaLoggingManager.Logger.Verbose($"Changed Username of {c.GetEndpointIp()} from '{client.GetUsername()}' to '{s.NewUsername}'");
                c.GetClient()?.SetUsername(s.NewUsername, false);
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
                (c.GetClient() as IServersideClient)?.SetUsername(welcomeReceived.Username, false);
            }

            [HandlerMethod(NoaControlPacketIds.FromClient.SyncUsername)]
            public static void SyncUsername(NoaControlPackets.FromClient.SyncUsername username, ConnectionBase c)
            {
                var client = c.GetClient();
                if (client == null) return;
                
                MultiNoaLoggingManager.Logger.Verbose($"Changed Username of {c.GetEndpointIp()} from '{client.GetUsername()}' to '{username.NewUsername}'");
                c.GetClient()?.SetUsername(username.NewUsername, true);
            }
        }
    }
}