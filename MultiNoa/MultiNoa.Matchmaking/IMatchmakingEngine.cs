namespace MultiNoa.Matchmaking
{
    public interface IMatchmakingEngine
    {
        #region Client-Related
        
        public void AddClient(IMatchmakingClient client, int channel);
        public void RemoveClient(IMatchmakingClient client);
        public void RemoveClient(ulong clientId);
        
        /// <returns>Amount of clients searching for a match</returns>
        public int GetClientsSearching();
        
        #endregion

        #region Channels
        
        public void DefineChannel(int channelId, MatchmakingChannelConfig config);
        

        #endregion

    }
}