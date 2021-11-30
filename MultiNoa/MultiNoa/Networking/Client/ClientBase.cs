using System;
using MultiNoa.Networking.Rooms;
using MultiNoa.Networking.Server;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.Client
{
    public abstract class ClientBase
    {
        public delegate void ClientReadyDelegate(ClientBase client);
        internal event ClientReadyDelegate OnClientConnected;
        public event ClientReadyDelegate OnClientReady;
        public abstract void SendData(byte[] data);
        public abstract ServerBase GetServer();
        public abstract IConnection GetConnection();
        public abstract ulong GetId();
        public abstract void Disconnect();
        public abstract Room GetRoom();
        
        public void MoveToRoom(Room room)
        {
            room?.RemoveClient(this);
            GetConnection().ChangeThread(room.GetRoomThread());
            
            OnMovedToRoom(room);
        }
        
        internal void InvokeOnClientConnected() => OnClientConnected?.Invoke(this);
        internal void InvokeOnClientReady() => OnClientReady?.Invoke(this);

        protected abstract void OnMovedToRoom(Room room);
    }
}