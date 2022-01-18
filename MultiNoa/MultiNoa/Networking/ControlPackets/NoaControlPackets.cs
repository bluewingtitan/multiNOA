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
                
                [NetworkProperty]
                private int _reason { get; set; }
                [NetworkProperty]
                public string Details { get; set; }
            }
            
            
            [MultiNoaInternal]
            [PacketStruct(NoaControlPacketIds.FromServer.WelcomePacket)]
            public struct WelcomePacket
            {
                /// <summary>
                /// If the protocol-version does not fit the one used on the receiving end,
                /// the receiver (client) will disconnect immediately.
                ///
                /// This version is set by the developer themselves when constructing the server/connection.
                /// It's recommended to change it via a configuration file.
                /// </summary>
                [NetworkProperty]
                public string ProtocolVersion { get; set; }
                
                /// <summary>
                /// Not used for now, besides for logging a warning that two different versions are used.
                /// Acts as a warning-system for developers.
                /// </summary>
                [NetworkProperty]
                public string RunningNoaVersion { get; set; }
                
                
                
                /// <summary>
                /// Not used for now, besides for logging a warning that two different versions are used.
                /// Acts as a warning-system for developers.
                /// </summary>
                [NetworkProperty]
                public ulong ClientId { get; set; }
            }
            
            [MultiNoaInternal]
            [PacketStruct(NoaControlPacketIds.FromServer.ConnectionEstablishedPacket)]
            public struct ConnectionEstablished
            {
            }
                        
            [MultiNoaInternal]
            [PacketStruct(NoaControlPacketIds.FromServer.SyncUsername)]
            public struct SyncUsername
            {
                [NetworkProperty]
                public string NewUsername { get; set; }
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
                [NetworkProperty]
                public string RunningNoaVersion { get; set; }
                
                [NetworkProperty] public string Username { get; set; }
            }

            [MultiNoaInternal]
            [PacketStruct(NoaControlPacketIds.FromClient.SyncUsername)]
            public struct SyncUsername
            {
                [NetworkProperty]
                public string NewUsername { get; set; }
            }
            
        }
    }
}