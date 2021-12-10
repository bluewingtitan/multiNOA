using System;
using MultiNoa.Networking.ControlPackets;
using MultiNoa.Networking.PacketHandling;
using MultiNoa.Networking.Rooms;
using MultiNoa.Networking.Server;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.Client
{
    /// <summary>
    /// The base implementation of IClient.
    /// </summary>
    public abstract class ClientBase
    {
        #region Synced Fields

        private string _username = "User";

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                // Sync new name
                var newName = new NoaControlPackets.FromClient.SyncUsername
                {
                    NewUsername = value
                };
                SendData(newName);
            }
        }

        /// <summary>
        /// Sets the username without syncing back.
        /// </summary>
        /// <param name="newName"></param>
        internal void SetUsernameUnsynced(string newName)
        {
            _username = newName;
        }

        #endregion


        public ClientBase(string username)
        {
            _username = username;
        }
        
        public delegate void ClientReadyDelegate(ClientBase client);
        internal event ClientReadyDelegate OnClientConnected;
        public event ClientReadyDelegate OnClientReady;
        public abstract void SendData(object data);
        public abstract ServerBase GetServer();
        public abstract ConnectionBase GetConnection();
        public abstract ulong GetId();
        public abstract void Disconnect();
        public abstract Room GetRoom();

        public void MoveToRoom(Room room)
        {
            GetRoom()?.RemoveClient(this);
            
            if (GetRoom()?.GetRoomThread() != room.GetRoomThread())
            {
                GetConnection().ChangeThread(room.GetRoomThread());
            }
            
            
            OnMovedToRoom(room);
        }
        
        internal void InvokeOnClientConnected() => OnClientConnected?.Invoke(this);
        internal void InvokeOnClientReady() => OnClientReady?.Invoke(this);

        protected abstract void OnMovedToRoom(Room room);
    }
}