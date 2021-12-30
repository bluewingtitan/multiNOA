using System.Collections.Generic;
using MultiNoa.GameSimulation;

namespace MultiNoa.Matchmaking.Engine
{
    public abstract class NoaMatchmakingChannel
    {
        protected readonly int _channelId;
        protected readonly IDynamicThread _thread;
        
        // Used to get the client by the ulong.
        protected List<IMatchmakingClient> _clients = new List<IMatchmakingClient>();

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
                _clients.Add(c);
            });
            
            
        }

        public void RemoveClient(ulong id)
        {
            _thread.ScheduleExecution(() =>
            {
                var c = _clients.Find(client => client.GetId() == id);
                if (c != null)
                    _clients.Remove(c);
            });
        }

        public void RemoveClient(IMatchmakingClient c)
        {
            _thread.ScheduleExecution(() =>
            {
                _clients.Remove(c);
            });
        }
        
        public abstract IMatchmakingResult[] DoAGeneration();
    }
}