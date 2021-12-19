using MultiNoa.Extensions;
using MultiNoa.Logging;
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
        protected string _username = "User";

        public string GetUsername()
        {
            return _username;
        }

        public void SetUsername(string username, bool synced = true)
        {
            if (synced)
            {
                // TODO: Sync!
                
            }

            _username = username;
        }

        #endregion

        protected Room CurrentRoom { get; private set; }
        
        public ClientBase(string username)
        {
            _username = username;
        }
        
        private byte _rightGroups = 0;
        
        internal event IClient.ClientReadyDelegate OnClientConnected;
        public event IClient.ClientReadyDelegate OnClientReady;
        public abstract void SendData(object data);
        public abstract ConnectionBase GetConnection();
        public abstract void Disconnect();
        
        public bool GetAuthorityGroup(AuthorityGroup group)
        {
            return group == AuthorityGroup.Default || _rightGroups.ToBitArray()[(int) group-1];
        }
        public void AddToGroup(AuthorityGroup group)
        {
            if(group == AuthorityGroup.Default)
                return;
            
            var c = _rightGroups.ToBitArray();
            c[(int) group-1] = true;

            var r = c.ToByte();
            if (r == null)
            {
                MultiNoaLoggingManager.Logger.Warning($"Failed to add client to group {group.ToString()}: failed to convert");
                return;
            }
            
            _rightGroups = (byte) r;
        }

        public void RemoveFromGroup(AuthorityGroup group)
        {
            if(group == AuthorityGroup.Default)
                return;
            
            var c = _rightGroups.ToBitArray();
            c[(int) group-1] = false;

            var r = c.ToByte();
            if (r == null)
            {
                MultiNoaLoggingManager.Logger.Warning($"Failed to remove client to group {group.ToString()}: failed to convert");
                return;
            }
            
            _rightGroups = (byte) r;
        }
        

        public void AddOnClientConnected(IClient.ClientReadyDelegate callback)
        {
            OnClientConnected += callback;
        }
        public void RemoveOnClientConnected(IClient.ClientReadyDelegate callback)
        {
            OnClientConnected -= callback;
        }
        public void InvokeOnClientConnected() => OnClientConnected?.Invoke(this);
        
        
        public void AddOnOnClientReady(IClient.ClientReadyDelegate callback)
        {
            OnClientReady += callback;
        }
        public void RemoveOnClientReady(IClient.ClientReadyDelegate callback)
        {
            OnClientReady -= callback;
        }
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