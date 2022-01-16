using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using MultiNoa.GameSimulation;
using MultiNoa.Logging;
using MultiNoa.Networking.Server;

namespace MultiNoa.Networking.Rooms
{
    public class RoomPool<T1> where T1: Room
    {

        public delegate T1 CreateRoomDelegate(ServerBase server, IDynamicThread thread, string roomName = "Room", bool threadSaveMode = false, string password = null);

        private readonly CreateRoomDelegate _createRoomDelegate;
        private readonly bool _threadSave;
        private readonly IDictionary<ulong, T1> _rooms;
        private readonly Room _baseRoom;

        public RoomPool(CreateRoomDelegate createRoomDelegate, Room baseRoom, bool threadSaveMode = false)
        {
            _baseRoom = baseRoom;
            _createRoomDelegate = createRoomDelegate;
            _threadSave = threadSaveMode;
            if(threadSaveMode)
                _rooms = new ConcurrentDictionary<ulong, T1>();
            else
                _rooms = new Dictionary<ulong, T1>();
        }

        public T1 OpenRoom(ServerBase server, IDynamicThread thread, string roomName = "Room", string password = null)
        {
            var newRoom = _createRoomDelegate(server, thread, roomName, _threadSave, password);
            _rooms[newRoom.GetRoomId()] = newRoom;

            return newRoom;
        }
        
        public T1 GetRoom(ulong roomId)
        {
            return _rooms[roomId];
        }


        public void CloseRoom(ulong roomId)
        {
            var room = _rooms[roomId];
            
            if(room == null)
                return;

            var abandoned = room.CloseRoom(_baseRoom);

            if(abandoned.Length > 0)
            {
                MultiNoaLoggingManager.Logger.Information(
                    $"Error: Room was closed, but {abandoned} clients stayed in. Disconnecting said clients.");

                foreach (var c in abandoned)
                    c.Disconnect();
                
            }

        }


    }
}