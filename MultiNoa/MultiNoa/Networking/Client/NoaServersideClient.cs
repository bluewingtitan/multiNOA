using MultiNoa.Networking.Server;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.Client
{
    /// <summary>
    /// Represents a client on the server side.
    /// </summary>
    public class NoaServersideClient: ClientBase, IServersideClient
    {
        protected readonly ServerBase Server;
        protected readonly ConnectionBase Connection;
        
        public NoaServersideClient(ServerBase server, ConnectionBase connection, ulong id) : base("noa-user")
        {
            Server = server;
            Connection = connection;
            IdOnServer = id;
        }

        public override void SendData(object data)
        {
            Connection.SendData(data);
        }

        public ServerBase GetServer()
        {
            return Server;
        }

        public override ConnectionBase GetConnection()
        {
            return Connection;
        }

        public override void Disconnect()
        {
            Connection.Disconnect();
        }
    }
}