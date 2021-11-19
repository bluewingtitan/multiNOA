using System.Collections.Generic;
using MultiNoa.GameSimulation;
using MultiNoa.Networking.Client;

namespace MultiNoa.Networking.Server
{
    /// <summary>
    /// Base class for servers.
    /// Contains basic management-functions for clients.
    /// Does not manage connections, use TcpServer for a basic setup with TcpListener.
    /// </summary>
    public abstract class Server: IServer, IUpdatable
    {
        private IDictionary<ulong, IClient> Clients { get; } = new Dictionary<ulong, IClient>();
        
        protected IDynamicThread Thread { get; }
        
        protected Server(ushort port, string protocolVersion, int tps = 60, string name = "New Server")
        {
            Thread = GetDynamicThread(tps, name);
        }
        
        protected virtual IDynamicThread GetDynamicThread(int tps, string name) => new DynamicThread(tps, name);
        
        public bool TryGetClient(ulong id, out IClient client) => Clients.TryGetValue(id, out client);
        
        
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

        protected abstract void OnUpdate();
        protected abstract void OnStop();

    }
}