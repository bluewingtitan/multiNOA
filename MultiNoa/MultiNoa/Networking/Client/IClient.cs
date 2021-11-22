using MultiNoa.Networking.Rooms;
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

        IRoom GetRoom();
        void MoveToRoom(IRoom room);
    }
}