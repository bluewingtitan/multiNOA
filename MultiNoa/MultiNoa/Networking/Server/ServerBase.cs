using MultiNoa.GameSimulation;
using MultiNoa.Logging;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.ControlPackets;
using MultiNoa.Networking.Rooms;
using MultiNoa.Networking.Transport;
using MultiNoa.Networking.Transport.Middleware;

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

        private readonly Room _pendingRoom;
        private readonly Room _baseRoom;
        private readonly ConnectionListener _listener;

        public readonly string ProtocolVersion;

        public ushort Port { get; }

        protected IDynamicThread Thread { get; }



        #region Events
        public delegate void ClientEventDelegate(IClient c);
        public event ClientEventDelegate OnClientFinishConnecting;
        public event ClientEventDelegate OnClientDisconnected;

        #endregion
        
        
        protected ServerBase(ushort port, string protocolVersion, ConnectionListener listener, int tps = 5, string name = "New Server")
        {
            ProtocolVersion = protocolVersion;
            Port = port;
            Thread = ConstructDynamicThread(tps, name);
            Thread.AddUpdatable(this);
            _listener = listener;
            _listener.OnConnection += OnConnected;
            
            _baseRoom = new Room(this, Thread, "Base Room", false);
            _pendingRoom = new Room(this, Thread, "Pending Room", false);
        }
        
        protected virtual IDynamicThread ConstructDynamicThread(int tps, string name) => new DynamicThread(tps, name);

        protected virtual IServersideClient ConstructClient(ConnectionBase connection, ulong clientId) => new NoaServersideClient(this, connection, clientId);

        public bool TryGetClient(ulong id, out IServersideClient client) => _baseRoom.TryGetClient(id, out client);

        private void OnConnected(ConnectionBase connection)
        {
            Thread.AddOffsetTask(new OffsetAction(() =>
            {
                var client = ConstructClient(connection, GetNewClientId());
                connection.SetClient(client);
                if (_pendingRoom.TryAddClient(client))
                {
                    // Send Welcome Packet
                    var packet = new NoaControlPackets.FromServer.WelcomePacket
                    {
                        ProtocolVersion = ProtocolVersion,
                        RunningNoaVersion = MultiNoaSetup.VersionCode
                    };

                    client.OnClientConnected += OnClientWelcomed;

                    client.GetConnection().OnDisconnected += con => InvokeClientDisconnected(client);

                    connection.SendData(packet);

                    return;
                }
            
                // => base room filled or closed => don't let new clients connect!
                // TODO: Send Control Packet containing Message "Server full or closed"
                client.Disconnect();
            }, 0));
        }

        /// <summary>
        /// Event invoked once client sends "WelcomeReceived"-Packet, stating the wish of being fully connected.
        /// </summary>
        /// <param name="client"></param>
        private void OnClientWelcomed(IClient client)
        {
            Thread.AddOffsetTask(new OffsetAction(() => 
                {
                    MultiNoaLoggingManager.Logger.Verbose($"Client {client.GetConnection().GetEndpointIp()} was fully connected");
                    NoaMiddlewareManager.OnConnectedServerside(client.GetConnection());
                    client.OnClientConnected -= OnClientWelcomed;
                    _baseRoom.TryAddClient((IServersideClient) client);
                    client.InvokeOnClientReady();
                    OnClientFinishConnecting?.Invoke(client);

                    // invoke OnClientReady on user side
                    client.GetConnection().SendData(new NoaControlPackets.FromServer.ConnectionEstablished(), excludes: new []
                        {
                            MiddlewareTarget.Encrypting
                        });
                }, 0));
        }
        
        public void Stop()
        {
            OnStop();
            _listener.StopListening();
            Thread.Stop();
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


        private void InvokeClientDisconnected(IClient client)
        {
            OnClientDisconnected?.Invoke(client);
        }
        
    }
}