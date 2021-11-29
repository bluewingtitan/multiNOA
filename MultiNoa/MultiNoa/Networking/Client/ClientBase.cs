using System;
using MultiNoa.Networking.Rooms;
using MultiNoa.Networking.Server;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.Client
{
    public abstract class ClientBase
    {
        public event EventHandler OnClientReady;
        public abstract void SendData(byte[] data);
        public abstract ServerBase GetServer();
        public abstract IConnection GetConnection();
        public abstract ulong GetId();
        public abstract void Disconnect();
        public abstract Room GetRoom();
        
        public void MoveToRoom(Room room)
        {
            GetConnection().ChangeThread(room.GetRoomThread());
            
            OnMovedToRoom(room);
        }

        protected abstract void OnMovedToRoom(Room room);
    }
}