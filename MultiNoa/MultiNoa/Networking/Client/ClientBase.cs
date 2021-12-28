using System.Collections.Generic;
using MultiNoa.Extensions;
using MultiNoa.Logging;
using MultiNoa.Networking.ControlPackets;
using MultiNoa.Networking.Rooms;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.Client
{
    /// <summary>
    /// The base implementation of IClient
    /// May be used as a base for any further client implementation.
    ///
    /// Allready contains basic room management for Serverside clients (As this part is critical and designed for one basic implementation)
    /// </summary>
    public abstract class ClientBase: IClient
    {
        #region Synced Fields

        private string _username = "User";

        public string GetUsername()
        {
            return _username;
        }

        public void SetUsername(string username, bool synced)
        {
            if (synced)
            {
                GetRoom()?.Broadcast(new NoaControlPackets.FromServer.SyncUsername
                {
                    NewUsername = username
                });
            }

            _username = username;
        }
        
        public void SetUsername(string username)
        {
            SetUsername(username, true);
        }

        #endregion

        protected Room CurrentRoom { get; private set; }
        
        public ClientBase(string username)
        {
            _username = username;
        }
        
        private readonly List<string> _groups = new List<string>();
        
        internal event IClient.ClientReadyDelegate OnClientConnected;
        public event IClient.ClientReadyDelegate OnClientReady;
        public abstract void SendData(object data);
        public abstract ConnectionBase GetConnection();
        public abstract void Disconnect();
        
        public bool GetAuthorityGroup(string group) =>
            string.IsNullOrEmpty(group) || group.Equals(AuthorityGroups.Default) || _groups.Contains(group);
        
        public string[] GetAuthorityGroups() => _groups.ToArray();
        
        public void AddToGroup(string group)
        {
            if(!_groups.Contains(group))
                _groups.Add(group);
        }
        
        public void RemoveFromGroup(string group)
        {
            _groups.Remove(group);
        }

        public void AddOnClientConnected(IClient.ClientReadyDelegate callback) => OnClientConnected += callback;
        public void RemoveOnClientConnected(IClient.ClientReadyDelegate callback) => OnClientConnected -= callback;
        public void InvokeOnClientConnected() => OnClientConnected?.Invoke(this);
        public void AddOnOnClientReady(IClient.ClientReadyDelegate callback) => OnClientReady += callback;
        public void RemoveOnClientReady(IClient.ClientReadyDelegate callback) => OnClientReady -= callback;
        public void InvokeOnClientReady() => OnClientReady?.Invoke(this);

        
        public Room GetRoom()
        {
            return CurrentRoom;
        }
        
        public void MoveToRoom(Room room)
        {
            GetRoom()?.RemoveClient((IServersideClient) this);
            
            if (GetRoom()?.GetRoomThread() != room.GetRoomThread())
            {
                GetConnection().ChangeThread(room.GetRoomThread());
            }
            
            
            CurrentRoom = room;
            OnMovedToRoom(room);
        }

        protected virtual void OnMovedToRoom(Room room)
        {
        }
    }
}