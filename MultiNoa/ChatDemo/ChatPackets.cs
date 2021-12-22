using MultiNoa.Networking.PacketHandling;

namespace ChatDemo
{
    public static class ChatPackets
    {
        public static class FromClient
        {
            
            [PacketStruct(PacketId)]
            public struct MessageFromClient
            {
                public const int PacketId = 0;
                
                [NetworkProperty(0)]
                public string Message { get; set; }
            }
        }
        
        public static class FromServer
        {
            [PacketStruct(PacketId)]
            public struct MessageFromServer
            {
                public const int PacketId = 1;
                [NetworkProperty(0)]
                public string Message { get; set; }
                
                [NetworkProperty(1)]
                public string Username { get; set; }
            }
        }
    }
}