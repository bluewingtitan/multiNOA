using MultiNoa.Networking.PacketHandling;

namespace MultiNoa.Networking.Transport
{
    public interface IConnection
    {
        /// <summary>
        /// Sets the packet handler used for incoming traffic
        /// </summary>
        /// <param name="newHandler">New Handler to use</param>
        void SetPacketHandler(IPacketHandler newHandler);

        string GetEndpointIp();
        
        void SendData(Packet packet);
    }
}