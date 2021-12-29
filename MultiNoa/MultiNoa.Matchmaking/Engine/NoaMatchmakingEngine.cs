namespace MultiNoa.Matchmaking.Engine
{
    /// <summary>
    /// A basic implementation of a dynamic matchmaking engine. Does one generation every 15 seconds.
    /// Uses an Elo System.
    ///
    /// If you are using this, make sure to update the elo after the match using the Elo Calculator. Expected scores and actual scores have to be numbers between 0 and 1.
    /// </summary>
    public class NoaMatchmakingEngine: IMatchmakingEngine
    {
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
            
        }
    }
}