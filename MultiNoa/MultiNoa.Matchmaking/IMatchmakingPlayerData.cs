namespace MultiNoa.Matchmaking
{
    public interface IMatchmakingPlayerData
    {
        public IMatchmakingClient GetClient();
        public double GetExpectedPoints();
    }
}