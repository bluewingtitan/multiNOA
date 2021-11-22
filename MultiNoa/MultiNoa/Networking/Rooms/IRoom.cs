using MultiNoa.GameSimulation;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.Server;

namespace MultiNoa.Networking.Rooms
{
    public interface IRoom
    {
        private static ulong _currentRoomId = 0;
        private static object _lock = 42;

        /// <summary>
        /// Returns a room id unique to this app instance.
        /// </summary>
        /// <returns></returns>
        protected static ulong GetNewRoomId()
        {
            lock (_lock)
            {
                _currentRoomId++;
                return _currentRoomId - 1;
            }
        }
        
        
        /// <summary>
        /// Returns the server containing this room.
        /// </summary>
        /// <returns></returns>
        IServer GetServer();

        /// <summary>
        /// Rooms may be named, though the name is not required to be unique.
        /// </summary>
        /// <returns></returns>
        string GetRoomName();

        /// <summary>
        /// Returns the room-id. Room-ids should be unique to their respective server-instance.
        /// </summary>
        /// <returns></returns>
        ulong GetRoomId();


        /// <summary>
        /// Try adding client to room. Supports password-protection per API, fully open rooms don't have to react to the password-parameter though.
        /// </summary>
        /// <param name="client">Client to add</param>
        /// <param name="password">Password passed from client</param>
        /// <returns></returns>
        bool TryAddClient(IClient client, string password = null);
        
        
        /// <summary>
        /// Try finding and returning client with specified id
        /// </summary>
        /// <param name="id">Id of client</param>
        /// <param name="client">Client found</param>
        /// <returns>If a client was found</returns>
        bool TryGetClient(ulong id, out IClient client);
        
        
        IDynamicThread GetRoomThread();

    }
}