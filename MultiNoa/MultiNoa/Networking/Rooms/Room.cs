using System.Collections.Concurrent;
using System.Collections.Generic;
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
        
        private readonly IDictionary<ulong, ClientBase> _clients;
        
        protected readonly string Password;
        protected readonly string Roomname;

        protected readonly IDynamicThread Thread;
        protected readonly ServerBase Server;
        protected readonly ulong RoomId;

        /// <summary>
        /// MultiNoa default IRoom implementation
        /// </summary>
        /// <param name="server">The overlaying server managing this room</param>
        /// <param name="thread">The internal thread</param>
        /// <param name="roomName">Name of the room, purely representative</param>
        /// <param name="threadSaveMode">Should this room use a threadsave Dictionary-Implementation?</param>
        /// <param name="password">Password for a client to join</param>
        public Room(ServerBase server, IDynamicThread thread, string roomName = "Room",bool threadSaveMode = false, string password = null)
        {
            if(threadSaveMode)
                _clients = new ConcurrentDictionary<ulong, ClientBase>();
            else
                _clients = new Dictionary<ulong, ClientBase>();

            RoomId = _roomId;
            _roomId++;
            
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

        public bool TryAddClient(ClientBase client, string password = null)
        {
            if (!string.IsNullOrEmpty(Password) && !string.Equals(password, Password))
            {
                return false;
            }
            
            MultiNoaLoggingManager.Logger.Debug($"Moving client {client.GetConnection().GetEndpointIp()} to room '{Roomname}'");
            
            
            var success = _clients.TryAdd(client.GetId(), client);
            
            if(success) client.MoveToRoom(this);


            return success;
        }

        public bool TryGetClient(ulong id, out ClientBase client)
        {
            return _clients.TryGetValue(id, out client);
        }

        internal void RemoveClient(ClientBase client)
        {
            _clients.Remove(client.GetId());
        }


        public void Broadcast(byte[] message, ClientBase exclude = null)
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