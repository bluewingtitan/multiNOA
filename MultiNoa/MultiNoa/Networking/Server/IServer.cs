using MultiNoa.GameSimulation;
using MultiNoa.Networking.Client;

namespace MultiNoa.Networking.Server
{
    public interface IServer
    {
        /// <summary>
        /// Try finding and returning client with specified id
        /// </summary>
        /// <param name="id">Id of client</param>
        /// <param name="client">Client found</param>
        /// <returns>If a client was found</returns>
        bool TryGetClient(ulong id, out IClient client);

        /// <summary>
        /// Stops server
        /// </summary>
        void Stop();

        IDynamicThread GetServerThread();
    }
}