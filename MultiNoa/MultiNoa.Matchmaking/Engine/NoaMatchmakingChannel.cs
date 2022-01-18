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
            _clients.Add(c);
            OnAddClient(c.Id);
        }

        public void RemoveClient(ulong id)
        {
            var c = _clients.Find(client => client.Id == id);
            if (c != null)
            {
                _clients.Remove(c);
            }
            OnRemoveClient(id);
        }

        

        public void RemoveClient(IMatchmakingClient c)
        {
            _clients.Remove(c);
            OnRemoveClient(c.Id);
        }
        
        protected virtual void OnRemoveClient(ulong id)
        {
        }
        
        protected virtual void OnAddClient(ulong id)
        {
            
        }
        
        public abstract IMatchmakingResult[] DoAGeneration();
    }
}