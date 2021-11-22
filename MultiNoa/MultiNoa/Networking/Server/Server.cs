using MultiNoa.GameSimulation;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.Rooms;
// ReSharper disable VirtualMemberCallInConstructor

namespace MultiNoa.Networking.Server
{
    /// <summary>
    /// Base class for servers.
    /// Contains basic management-functions for clients.
    /// Does not manage connections, use TcpServer for a basic setup with TcpListener.
    /// </summary>
    public abstract class Server: IServer, IUpdatable
    {
        private readonly IRoom baseRoom;
        
        protected IDynamicThread Thread { get; }
        
        protected Server(ushort port, string protocolVersion, int tps = 60, string name = "New Server")
        {
            Thread = ConstructDynamicThread(tps, name);
        }
        
        protected virtual IDynamicThread ConstructDynamicThread(int tps, string name) => new DynamicThread(tps, name);

        public bool TryGetClient(ulong id, out IClient client) => baseRoom.TryGetClient(id, out client);
        
        
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
            
        }

        protected abstract void OnUpdate();
        protected abstract void OnStop();

    }
}