using MultiNoa.GameSimulation;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.Rooms;
using MultiNoa.Networking.Transport;

// ReSharper disable VirtualMemberCallInConstructor

namespace MultiNoa.Networking.Server
{
    /// <summary>
    /// Base class for servers.
    /// Contains basic management-functions for clients.
    /// Does not manage connections, use TcpServer for a basic setup with TcpListener.
    /// </summary>
    public abstract class ServerBase: IUpdatable
    {
        private ulong _currentClientId = 0;
        private ulong GetNewClientId() => _currentClientId++;

        private readonly Room baseRoom;
        private readonly ConnectionListener _listener;

        public readonly string ProtocolVersion;
        
        public ushort Port { get; }

        protected IDynamicThread Thread { get; }
        
        protected ServerBase(ushort port, string protocolVersion, ConnectionListener listener, int tps = 5, string name = "New Server")
        {
            ProtocolVersion = protocolVersion;
            Port = port;
            Thread = ConstructDynamicThread(tps, name);
            Thread.AddUpdatable(this);
            _listener = listener;
            _listener.OnConnection += OnConnected;
            
            baseRoom = new Room(this, Thread, "Base Room", false);
        }
        
        protected virtual IDynamicThread ConstructDynamicThread(int tps, string name) => new DynamicThread(tps, name);

        protected virtual ClientBase ConstructClient(IConnection connection, ulong clientId) => new NoaClient(this, connection, clientId);

        public bool TryGetClient(ulong id, out ClientBase client) => baseRoom.TryGetClient(id, out client);

        private void OnConnected(IConnection connection)
        {
            Thread.AddOffsetTask(new OffsetAction(() =>
            {
                var client = ConstructClient(connection, GetNewClientId());
                if (baseRoom.TryAddClient(client))
                {
                    return;
                }
            
                // => base room filled or closed => don't let new clients connect!
                // TODO: Send Control Packet containing Message "Server full or closed"
                client.Disconnect();
            }, 0));
        }
        
        
        public void Stop()
        {
            Thread.Stop();
            OnStop();
        }
        
        public IDynamicThread GetServerThread()
        {
            return Thread;
        }

        public void Update()
        {
            OnUpdate();
        }

        public void PerSecondUpdate()
        {
            OnPerSecondUpdate();
        }

        protected abstract void OnUpdate();
        protected abstract void OnPerSecondUpdate();
        protected abstract void OnStop();

    }
}