using MultiNoa.Networking.PacketHandling;
using MultiNoa.Networking.Server;

namespace MultiNoa.Networking.ControlPackets
{
    /// <summary>
    /// A collection of packets used internally to send data between instances of network nodes running multiNoa-driven Network Code.
    /// </summary>
    public static class NoaControlPackets
    {
        public static class FromServer
        {
            [MultiNoaInternal]
            [PacketStruct(NoaControlPacketIds.FromServer.ConnectionRefusedPacket)]
            public struct ConnectionRefusedPacket
            {
                public ConnectionRefusedReason Reason
                {
                    get => (ConnectionRefusedReason) _reason;
                    set => _reason = (int) value;
                }
                // TODO: Implement dynamic enums and use this here.
                
                [NetworkProperty(0)]
                private int _reason { get; set; }
                [NetworkProperty(1)]
                public string Details { get; set; }
            }
            
            
            [MultiNoaInternal]
            [PacketStruct(NoaControlPacketIds.FromServer.WelcomePacket)]
            public struct WelcomePacket
            {
                /// <summary>
                /// If the protocol-version does not fit the one used on the receiving end,
                /// the receiver (client) will disconnect immideatly.
                ///
                /// This version is set by the developer themselves when constructing the server/connection.
                /// It's recommended to change it via a configuration file.
                /// </summary>
                [NetworkProperty(0)]
                public string ProtocolVersion { get; set; }
                
                /// <summary>
                /// Not used for now, besides for logging a warning that two different versions are used.
                /// Acts as a warning-system for developers.
                /// </summary>
                [NetworkProperty(1)]
                public string RunningNoaVersion { get; set; }
            }
            
        }

        public static class FromClient
        {
            [MultiNoaInternal]
            [PacketStruct(NoaControlPacketIds.FromClient.WelcomeReceived)]
            public struct WelcomeReceived
            {
                /// <summary>
                /// Not used for now, besides for logging a warning that two different versions are used.
                /// Acts as a warning-system for developers.
                /// </summary>
                [NetworkProperty(0)]
                public string RunningNoaVersion { get; set; }
                
                [NetworkProperty(0)] public string Username { get; set; }
            }

            [MultiNoaInternal]
            [PacketStruct(NoaControlPacketIds.FromClient.SyncUsername)]
            public struct SyncUsername
            {
                [NetworkProperty(0)]
                public string NewUsername { get; set; }
            }
            
        }
    }
}