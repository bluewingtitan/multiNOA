using System.Collections.Generic;
using MultiNoa.GameSimulation;

namespace MultiNoa.Matchmaking.Engine
{
    public abstract class NoaMatchmakingChannel
    {
        protected readonly int _channelId;
        protected readonly IDynamicThread _thread;
        
        // Sorted by MMR
        protected SortedList<decimal, IMatchmakingClient> _clients =
            new SortedList<decimal, IMatchmakingClient>();

        // Used to get the client by the ulong.
        protected Dictionary<ulong, IMatchmakingClient> _idClients = new Dictionary<ulong, IMatchmakingClient>();

        protected readonly MatchmakingChannelConfig Config;

        protected NoaMatchmakingChannel(int channelId, MatchmakingChannelConfig config, IDynamicThread thread)
        {
            _channelId = channelId;
            Config = config;
            _thread = thread;
        }


        public void AddClient(IMatchmakingClient c)
        {
            _thread.ScheduleExecution(() =>
            {
                _idClients[c.GetId()] = c;
             
                         // 150mmr and client id of 244 will lead to 150.244, which makes it unique but still comparable to players with the same mmr.
                         var decimalId = decimal.Parse("0." + c.GetId());
                         
                         var uniqueMmr = c.GetMmr(_channelId) + decimalId;
             
                         _clients[uniqueMmr] = c;
            });
            
            
        }

        public void RemoveClient(ulong id)
        {
            _thread.ScheduleExecution(() =>
            {
                if(!_idClients.ContainsKey(id))
                                return;
                            
                var c = _idClients[id];
                _clients.RemoveAt(_clients.IndexOfValue(c));
                _idClients.Remove(id);
            });
        }

        public void RemoveClient(IMatchmakingClient c)
        {
            _thread.ScheduleExecution(() =>
            {
                if(!_clients.ContainsValue(c))
                    return;
                
                _clients.RemoveAt(_clients.IndexOfValue(c));
                _idClients.Remove(c.GetId());
            });
        }
        
        public abstract IMatchmakingResult[] DoAGeneration();
    }
}