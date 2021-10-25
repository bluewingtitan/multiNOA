using System.Net;
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

        IPAddress GetEndpointIp();
        
        void SendData(byte[] data);

        /// <summary>
        /// Returns the client associated with this connection
        /// </summary>
        /// <returns></returns>
        IClient GetClient();
    }
}