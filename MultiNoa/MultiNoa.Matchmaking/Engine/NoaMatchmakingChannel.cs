using System.Collections.Generic;
using MultiNoa.GameSimulation;

namespace MultiNoa.Matchmaking.Engine
{
    public abstract class NoaMatchmakingChannel
    {
        private readonly int _channelId;
        private readonly int _goalRoomSize;
        
        // Sorted by MMR
        private SortedList<decimal, IMatchmakingClient> _clients =
            new SortedList<decimal, IMatchmakingClient>();

        // Used to get the client by the ulong.
        private Dictionary<ulong, IMatchmakingClient> _idClients = new Dictionary<ulong, IMatchmakingClient>();

        protected NoaMatchmakingChannel(int channelId, int goalRoomSize)
        {
            _channelId = channelId;
            _goalRoomSize = goalRoomSize;
        }


        public void AddClient(IMatchmakingClient c)
        {
            _idClients[c.GetId()] = c;

            // 150mmr and client id of 244 will lead to 150.244, which makes it unique but still comparable to players with the same mmr.
            var decimalId = decimal.Parse("0." + c.GetId());
            
            var uniqueMmr = c.GetMmr(_channelId) + decimalId;

            _clients[uniqueMmr] = c;
        }

        public void RemoveClient(ulong id)
        {
            if(!_idClients.ContainsKey(id))
                return;
            
            var c = _idClients[id];
            _clients.RemoveAt(_clients.IndexOfValue(c));
            _idClients.Remove(id);
        }

        public void RemoveClient(IMatchmakingClient c)
        {
            if(!_clients.ContainsValue(c))
                return;
                
            _clients.RemoveAt(_clients.IndexOfValue(c));
            _idClients.Remove(c.GetId());
        }
        
        public abstract void DoAGeneration();
    }
}