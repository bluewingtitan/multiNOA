using System.Net.Sockets;
using MultiNOA.Networking.PacketHandling;

namespace MultiNOA.Networking.Common
{
    /// <summary>
    /// Represents a connection, offering functionality for sending data and for configuring packet handling.
    /// </summary>
    public interface IConnetion
    {
        /// <summary>
        /// Sets the packet handler used for incoming traffic
        /// </summary>
        /// <param name="newHandler">New Handler to use</param>
        void SetPacketHandler(IPacketHandler newHandler);

        string GetEndpointIp();

        void Connect(TcpClient client);

        void Disconnect();

        void SendData(Packet packet);

    }
}