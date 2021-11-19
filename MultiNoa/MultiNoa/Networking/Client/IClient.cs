using MultiNoa.Networking.Server;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.Client
{
    public interface IClient
    {
        IServer GetServer();
        IConnection GetConnection();
        ulong GetId();
        void Disconnect();
    }
}