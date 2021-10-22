using System.Net.Sockets;
using MultiNoa.Networking.PacketHandling;

namespace MultiNoa.Networking.Transport.Connection
{
    public class TcpConnection: IConnection
    {
        public void Connect(TcpClient client){
            throw new System.NotImplementedException();
        }

        public void Disconnect(){
            throw new System.NotImplementedException();
        }
        
        public void SetPacketHandler(IPacketHandler newHandler)
        {
            throw new System.NotImplementedException();
        }

        public string GetEndpointIp()
        {
            throw new System.NotImplementedException();
        }

        public void SendData(Packet packet)
        {
            throw new System.NotImplementedException();
        }
    }
}