using System.Collections.Generic;
using MultiNoa.GameSimulation;
using MultiNoa.Networking.Client;

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
        private readonly Dictionary<int, NoaMatchmakingChannel> _channels = new Dictionary<int, NoaMatchmakingChannel>();
        public event IMatchmakingEngine.TeamsGeneratedDelegate OnTeamsGenerated;

        
        
        public NoaMatchmakingEngine(string name = "Matchmaking")
        {
            // Runs at 5 tps to be more flexible regarding adding/removing clients in matchmaking
            _thread = new DynamicThread(5, name);
            _thread.AddUpdatable(this);
        }
        
        
        public void AddClient(IMatchmakingClient client, int channel)
        {
            _thread.ScheduleExecution(() =>
            {
                if (_channels.ContainsKey(channel))
                {
                    client.OnClientDisconnected += RemoveIClientOnDisconnect;
                    _channels[channel].AddClient(client);
                }
            });
        }

        public void RemoveClient(IMatchmakingClient client)
        {
            _thread.ScheduleExecution(() =>
            {
                foreach (var (_, channel) in _channels)
                {
                    channel.RemoveClient(client);
                }
            });
        }
        
        
        private void RemoveIClientOnDisconnect(IClient client)
        {
            // We know that client implements IMatchmaking client, as the only way it's getting assigned this function as event call back is in a function where it's passed as IMatchmakingClient.
            RemoveClient((IMatchmakingClient) client);
        }

        public void RemoveClient(ulong clientId)
        {
            _thread.ScheduleExecution(() =>
            {
                foreach (var (_, channel) in _channels)
                {
                    channel.RemoveClient(clientId);
                }
            });
        }

        public int GetClientsSearching()
        {
            return 0;
        }

        public void DefineChannel(int channelId, MatchmakingChannelConfig config)
        {
            NoaMatchmakingChannel channel = config.Mode switch
            {
                MatchmakingMode.Fast => new NoaFastMatchmakingChannel(channelId, config, _thread),
                MatchmakingMode.Flexible => new NoaFlexibleMatchmakingChannel(channelId, config, _thread),
                MatchmakingMode.Static => new NoaStaticMatchmakingChannel(channelId, config, _thread),
                _ => new NoaFastMatchmakingChannel(channelId, config, _thread)
            };

            _thread.ScheduleExecution(() =>
            {
                _channels[channelId] = channel;
            });
        }

        public void Stop()
        {
            _thread.Stop();
            // TODO: remove all clients from channels
        }


        public void Update() {}

        private int _counter = GenerationSeconds;
        public void PerSecondUpdate()
        {
            _counter--;
            if (_counter <= 0)
            {
                _counter = GenerationSeconds;

                var results = new List<IMatchmakingResult>();

                foreach (var (_, channel) in _channels)
                {
                    results.AddRange(channel.DoAGeneration());
                }

                foreach (var r in results)
                {
                    foreach (var d in r.GetTeamA())
                        RemoveClient(d.GetClient());

                    foreach (var d in r.GetTeamB())
                        RemoveClient(d.GetClient());
                }
                
                OnTeamsGenerated?.Invoke(results.ToArray());
            }
        }
    }
}