using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.Client
{
    public class NoaUserSideClient: ClientBase, IUserSideClient
    {
        private readonly ConnectionBase _connection;
        
        public NoaUserSideClient(string username, ConnectionBase connection) : base(username)
        {
            connection.SetClient(this);
            _connection = connection;
        }

        public override void SendData(object data) => _connection.SendData(data);

        public override ConnectionBase GetConnection() => _connection;
        
        public override void Disconnect() => _connection.Disconnect();
    }
}