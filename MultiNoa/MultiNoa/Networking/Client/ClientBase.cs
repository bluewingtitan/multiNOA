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

        public ulong ClientId { get; set; }
        
        protected Room CurrentRoom { get; private set; }
        
        public ClientBase(string username)
        {
            _username = username;
        }
        
        private readonly List<string> _groups = new List<string>();
        
        public event IClient.ClientEventDelegate OnClientConnected;
        public event IClient.ClientEventDelegate OnClientDisconnected;
        public event IClient.ClientEventDelegate OnClientReady;
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

        public void InvokeOnClientConnected() => OnClientConnected?.Invoke(this);
        public void InvokeOnClientReady() => OnClientReady?.Invoke(this);
        public void InvokeOnClientDisconnected() => OnClientDisconnected?.Invoke(this);


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