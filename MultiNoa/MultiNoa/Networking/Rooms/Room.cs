using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MultiNoa.GameSimulation;
using MultiNoa.Logging;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.Server;

namespace MultiNoa.Networking.Rooms
{
    /// <summary>
    /// Room implementation used in aton too.
    /// Supports a thread-save mode.
    /// </summary>
    public class Room
    {
        private static ulong _roomId = 0;
        private static object _roomIdLock = 0;

        private static ulong GetNewRoomId()
        {
            lock (_roomIdLock)
            {
                return _roomId++;
            }
        }
        
        private readonly IDictionary<ulong, IServersideClient> _clients;
        
        protected readonly string Password;
        protected readonly string Roomname;

        protected readonly IDynamicThread Thread;
        protected readonly ServerBase Server;
        protected readonly ulong RoomId;


        #region Events
        public delegate void ClientEventDelegate(IClient c);
        public event ClientEventDelegate OnClientJoinRoom;
        public event ClientEventDelegate OnClientLeaveRoom;


        public delegate void RoomEventDelegate();
        public event RoomEventDelegate OnRoomClosed;
        
        #endregion

        /// <summary>
        /// MultiNoa default IRoom implementation
        /// </summary>
        /// <param name="server">The overlaying server managing this room</param>
        /// <param name="thread">The internal thread</param>
        /// <param name="roomName">Name of the room, purely representative</param>
        /// <param name="threadSaveMode">Should this room use a threadsave Dictionary-Implementation?</param>
        /// <param name="password">Password for a client to join</param>
        public Room(ServerBase server, IDynamicThread thread, string roomName = "Room", bool threadSaveMode = false, string password = null)
        {
            if(threadSaveMode)
                _clients = new ConcurrentDictionary<ulong, IServersideClient>();
            else
                _clients = new Dictionary<ulong, IServersideClient>();

            RoomId = GetNewRoomId();
            
            Password = password;
            Server = server;
            Thread = thread;
            Roomname = roomName;
        }

        public ServerBase GetServer()
        {
            return Server;
        }

        public virtual string GetRoomName()
        {
            return Roomname;
        }

        public virtual ulong GetRoomId()
        {
            return RoomId;
        }

        public bool TryAddClient(IServersideClient client, string password = null)
        {
            if (_clients.ContainsKey(client.Id))
                return true;
            
            if (!string.IsNullOrEmpty(Password) && !string.Equals(password, Password))
            {
                return false;
            }
            
            MultiNoaLoggingManager.Logger.Debug($"Moving client {client.GetConnection().GetEndpointIp()} to room '{Roomname}'");
            
            
            var success = _clients.TryAdd(client.Id, client);
            
            if(success)
            {
                // move client to room, remove from old room
                client.Room?.RemoveClient(client);
            
                if (client.Room?.GetRoomThread() != Thread)
                {
                    client.GetConnection().ChangeThread(Thread);
                }

                client.MoveToRoomCallback(this);
                
                
                OnClientJoinRoom?.Invoke(client);
            }
            


            return success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newRoomForClients"></param>
        /// <returns>Array of clients the room wasn't able to move to the new room</returns>
        public IServersideClient[] CloseRoom(Room newRoomForClients)
        {
            List<IServersideClient> illegalClients = new List<IServersideClient>();
            foreach (var (_, c) in _clients.ToArray())
            {
                if (!newRoomForClients.TryAddClient(c))
                {
                    illegalClients.Add(c);
                }
            }
            
            _clients.Clear();
            OnRoomClosed?.Invoke();

            return illegalClients.ToArray();
        }
        
        
        public bool TryGetClient(ulong id, out IServersideClient client)
        {
            return _clients.TryGetValue(id, out client);
        }

        internal void RemoveClient(IServersideClient client)
        {
            _clients.Remove(client.Id);
            OnClientLeaveRoom?.Invoke(client);
        }

        public void Broadcast(object message, IServersideClient exclude = null)
        {
            MultiNoaLoggingManager.Logger.Debug($"Broadcasting to {_clients.Count} clients");
            foreach (var (_, client) in _clients)
            {
                if (exclude == client)
                    continue;
                client.SendData(message);
            }
        }
        
        public IDynamicThread GetRoomThread()
        {
            return Thread;
        }
    }
}