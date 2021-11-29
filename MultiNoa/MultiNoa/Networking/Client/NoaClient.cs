using MultiNoa.Networking.Rooms;
using MultiNoa.Networking.Server;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.Client
{
    public class NoaClient: ClientBase
    {
        protected readonly ServerBase _server;
        protected readonly IConnection _connection;
        protected readonly ulong _clientId;
        protected Room CurrentRoom { get; private set; }
        
        public NoaClient(ServerBase server, IConnection connection, ulong id)
        {
            _server = server;
            _connection = connection;
            _clientId = id;
        }

        public override void SendData(byte[] data)
        {
            _connection.SendData(data);
        }

        public override ServerBase GetServer()
        {
            return _server;
        }

        public override IConnection GetConnection()
        {
            return _connection;
        }

        public override ulong GetId()
        {
            return _clientId;
        }

        public override void Disconnect()
        {
            _connection.Disconnect();
        }

        public override Room GetRoom()
        {
            return CurrentRoom;
        }

        protected override void OnMovedToRoom(Room room)
        {
            CurrentRoom = room;
        }
    }
}