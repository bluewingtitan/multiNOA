namespace MultiNoa.Matchmaking.Engine
{
    public class NoaMatchmakingResult: IMatchmakingResult
    {
        private readonly IMatchmakingPlayerData[] _teamA;
        private readonly IMatchmakingPlayerData[] _teamB;
        private readonly MatchmakingMode _mode;
        private readonly int _channel;

        public NoaMatchmakingResult(IMatchmakingPlayerData[] teamA, IMatchmakingPlayerData[] teamB, MatchmakingMode mode, int channel)
        {
            _teamA = teamA;
            _teamB = teamB;
            _mode = mode;
            _channel = channel;
        }
        
        public IMatchmakingPlayerData[] GetTeamA() => _teamA;
        public IMatchmakingPlayerData[] GetTeamB() => _teamB;
        public MatchmakingMode GetMatchmakingMode() => _mode;

        public int GetChannel() => _channel;
    }

    public class NoaMatchmakingPlayerData : IMatchmakingPlayerData
    {
        private readonly IMatchmakingClient _client;
        private readonly double _expectedPoints;

        public NoaMatchmakingPlayerData(IMatchmakingClient client, double expectedPoints)
        {
            _client = client;
            _expectedPoints = expectedPoints;
        }

        public IMatchmakingClient GetClient() => _client;
        public double GetExpectedPoints() => _expectedPoints;
    }
}