using MultiNoa.Networking.Rooms;
using MultiNoa.Networking.Server;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.Client
{
    /// <summary>
    /// Represents a client on the server side.
    /// </summary>
    public class NoaServersideClient: ClientBase, IServersideClient
    {
        protected readonly ServerBase _server;
        protected readonly ConnectionBase _connection;
        protected readonly ulong _clientId;
        
        public NoaServersideClient(ServerBase server, ConnectionBase connection, ulong id) : base("noa-user")
        {
            _server = server;
            _connection = connection;
            _clientId = id;
        }

        public override void SendData(object data)
        {
            _connection.SendData(data);
        }

        public ServerBase GetServer()
        {
            return _server;
        }

        public override ConnectionBase GetConnection()
        {
            return _connection;
        }

        public ulong GetId()
        {
            return _clientId;
        }

        public override void Disconnect()
        {
            _connection.Disconnect();
        }
    }
}