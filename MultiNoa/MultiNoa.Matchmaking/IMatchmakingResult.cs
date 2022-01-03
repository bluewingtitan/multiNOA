namespace MultiNoa.Matchmaking
{
    public interface IMatchmakingResult
    {
        public IMatchmakingPlayerData[] GetTeamA();
        public IMatchmakingPlayerData[] GetTeamB();
        public MatchmakingMode GetMatchmakingMode();
        public int GetChannel();
    }
}