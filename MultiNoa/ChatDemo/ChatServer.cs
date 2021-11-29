using MultiNoa.Networking.Server;

namespace ChatDemo
{
    public class ChatServer: NoaTcpServer
    {
        public ChatServer(ushort port) : base(port, "demo", 5, "Chat Server")
        {
        }
    }
}