using MultiNoa.GameSimulation;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.PacketHandling;

namespace MultiNoa.Networking.Transport
{
    public interface IConnection: IUpdatable
    {
        public const int DataBufferSize = 4096;
        /// <summary>
        /// Sets the packet handler used for incoming traffic
        /// </summary>
        /// <param name="newHandler">New Handler to use</param>
        void SetPacketHandler(IPacketHandler newHandler);

        string GetEndpointIp();
        
        void SendData(byte[] data);

        /// <returns>client associated with this connection</returns>
        IClient GetClient();
    }
}