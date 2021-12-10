using MultiNoa.Networking.Rooms;
using MultiNoa.Networking.Server;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.Client
{
    /// <summary>
    /// Represents a client on the server side.
    /// </summary>
    public class NoaServersideRepresentation: ClientBase
    {
        protected readonly ServerBase _server;
        protected readonly ConnectionBase _connection;
        protected readonly ulong _clientId;
        protected Room CurrentRoom { get; private set; }
        
        public NoaServersideRepresentation(ServerBase server, ConnectionBase connection, ulong id) : base("noa-user")
        {
            _server = server;
            _connection = connection;
            _clientId = id;
        }

        public override void SendData(object data)
        {
            _connection.SendData(data);
        }

        public override ServerBase GetServer()
        {
            return _server;
        }

        public override ConnectionBase GetConnection()
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