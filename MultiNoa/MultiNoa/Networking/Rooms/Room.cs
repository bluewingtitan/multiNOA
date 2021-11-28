using System.Collections.Concurrent;
using System.Collections.Generic;
using MultiNoa.GameSimulation;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.Server;

namespace MultiNoa.Networking.Rooms
{
    /// <summary>
    /// Room implementation used in aton too.
    /// Supports a thread-save mode.
    /// </summary>
    public class Room: IRoom
    {
        protected readonly IDictionary<ulong, IClient> Clients;
        protected readonly string Password;
        protected readonly string Roomname;

        protected readonly IDynamicThread Thread;
        protected readonly IServer Server;
        protected readonly ulong RoomId;

        /// <summary>
        /// MultiNoa default IRoom implementation
        /// </summary>
        /// <param name="server">The overlaying server managing this room</param>
        /// <param name="thread">The internal thread</param>
        /// <param name="roomName">Name of the room, purely representative</param>
        /// <param name="threadSaveMode">Should this room use a threadsave Dictionary-Implementation?</param>
        /// <param name="password">Password for a client to join</param>
        public Room(IServer server, IDynamicThread thread, string roomName = "Room",bool threadSaveMode = false, string password = null)
        {
            if(threadSaveMode)
                Clients = new ConcurrentDictionary<ulong, IClient>();
            else
                Clients = new Dictionary<ulong, IClient>();

            RoomId = IRoom.GetNewRoomId();
            Password = password;
            Server = server;
            Thread = thread;
            Roomname = roomName;
        }
        
        
        public IServer GetServer()
        {
            return Server;
        }

        public string GetRoomName()
        {
            return Roomname;
        }

        public ulong GetRoomId()
        {
            return RoomId;
        }

        public bool TryAddClient(IClient client, string password = null)
        {
            if (!string.IsNullOrEmpty(Password) && !string.Equals(password, Password))
            {
                return false;
            }
            
            return Clients.TryAdd(client.GetId(), client);
        }

        public bool TryGetClient(ulong id, out IClient client)
        {
            return Clients.TryGetValue(id, out client);
        }

        public IDynamicThread GetRoomThread()
        {
            return Thread;
        }
    }
}