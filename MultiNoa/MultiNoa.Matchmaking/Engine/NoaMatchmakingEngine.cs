using MultiNoa.GameSimulation;

namespace MultiNoa.Matchmaking.Engine
{
    /// <summary>
    /// A basic implementation of a dynamic matchmaking engine. Does one generation every 5 seconds.
    /// Uses an Elo System.
    ///
    /// Only matches into a fixed room size. Very inefficient with bigger room sizes.
    ///
    /// It's from a game that does 1v1 exclusively after all...
    ///
    /// If you are using this, make sure to update the elo after the match using the Elo Calculator. Expected scores and actual scores have to be numbers between 0 and 1.
    /// </summary>
    public class NoaMatchmakingEngine: IMatchmakingEngine, IUpdatable
    {
        private const int GenerationSeconds = 5;
        private IDynamicThread _thread;
        
        
        public NoaMatchmakingEngine(string name = "Matchmaking")
        {
            // Runs at 5 tps to be more flexible regarding adding/removing clients in matchmaking
            _thread = new DynamicThread(5, name);
            _thread.AddUpdatable(this);
        }
        
        
        public void AddClient(IMatchmakingClient client, int channel)
        {
            
        }

        public void RemoveClient(IMatchmakingClient client)
        {
            
        }

        public void RemoveClient(ulong clientId)
        {
            
        }

        public int GetClientsSearching()
        {
            return 0;
        }

        public void DefineChannel(int channelId, MatchmakingChannelConfig config)
        {
            
        }


        public void Update() {}

        private int _counter = GenerationSeconds;
        public void PerSecondUpdate()
        {
            _counter--;
            if (_counter <= 0)
            {
                _counter = GenerationSeconds;
                // TODO: Execute all channels.
            }
        }
    }
}