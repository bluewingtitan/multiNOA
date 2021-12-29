using MultiNoa.Networking.Client;

namespace MultiNoa.Matchmaking
{
    public interface IMatchmakingClient: IServersideClient
    {
        /// <summary>
        /// Used as primary number to match clients. Return constant if not needed.
        /// </summary>
        /// <param name="channel">Channel to get MMR for</param>
        /// <returns>Clients Matchmaking Rating</returns>
        public int GetMmr(int channel);
    }
}