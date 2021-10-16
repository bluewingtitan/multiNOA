using System.Net.Sockets;
using MultiNoa.Networking.PacketHandling;

namespace MultiNoa.Networking.Transport
{
    /// <summary>
    /// Represents a connection, offering functionality for sending data and for configuring packet handling.
    /// </summary>
    public interface ITcpConnetion : IConnection
    {
        void Connect(TcpClient client);

        void Disconnect();
    }
}