namespace MultiNoa.Matchmaking
{
    public interface IMatchmakingEngine
    {
        public void AddClient(IMatchmakingClient client, int channel);
        public void RemoveClient(IMatchmakingClient client);
        public void RemoveClient(ulong clientId);

        
        /// <returns>Amount of clients searching for a match</returns>
        public int GetClientsSearching();
    }
}