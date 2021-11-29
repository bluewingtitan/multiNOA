using MultiNoa.Networking.PacketHandling;

namespace ChatDemo
{
    public static class ChatPackets
    {
        public static class FromClient
        {
            [PacketStruct(0)]
            public struct MessageFromClient
            {
                [NetworkProperty(0)]
                public string Message { get; set; }
            }
        }
        
        public static class FromServer
        {
            [PacketStruct(1)]
            public struct MessageFromServer
            {
                [NetworkProperty(0)]
                public string Message { get; set; }
            }
        }
    }
}