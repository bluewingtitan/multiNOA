namespace MultiNoa.Matchmaking.Engine
{
    public class NoaMatchmakingResult: IMatchmakingResult
    {
        private readonly IMatchmakingPlayerData[] _teamA;
        private readonly IMatchmakingPlayerData[] _teamB;

        public NoaMatchmakingResult(IMatchmakingPlayerData[] teamA, IMatchmakingPlayerData[] teamB)
        {
            _teamA = teamA;
            _teamB = teamB;
        }
        
        public IMatchmakingPlayerData[] GetTeamA() => _teamA;
        public IMatchmakingPlayerData[] GetTeamB() => _teamB;
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