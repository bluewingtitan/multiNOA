using MultiNoa.GameSimulation;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.PacketHandling;

namespace MultiNoa.Networking.Transport
{
    public abstract class ConnectionBase: IUpdatable
    {

        protected ConnectionBase()
        {
            // Prepare for horror.
            OnDisconnected += c => c.GetClient()?.GetRoom().RemoveClient(c.GetClient());
            // Recover from horror.
        }
        
        protected const int DataBufferSize = 4096;

        public delegate void ConnectionEventDelegate(ConnectionBase connection);

        public event ConnectionEventDelegate OnConnected;
        public event ConnectionEventDelegate OnDisconnected;

        public abstract void Update();
        public abstract void PerSecondUpdate();
        public abstract void SetPacketHandler(IPacketHandler newHandler);
        public abstract string GetEndpointIp();
        public abstract void SendData(byte[] data);
        public abstract ClientBase GetClient();
        public abstract void SetClient(ClientBase client);

        internal void InvokeOnConnected()
        {
            OnConnected?.Invoke(this);
        }
        
        public void Disconnect()
        {
            OnDisconnected?.Invoke(this);
            OnDisconnect();
        }
        
        protected abstract void OnDisconnect();
        public abstract void ChangeThread(IDynamicThread newThread);
        public abstract string GetProtocolVersion();
    }
}