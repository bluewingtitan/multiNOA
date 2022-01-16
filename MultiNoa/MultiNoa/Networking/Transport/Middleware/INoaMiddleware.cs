using System.Collections.Generic;

namespace MultiNoa.Networking.Transport.Middleware
{
    /// <summary>
    /// Used to implement special things before sending and after receiving.
    ///
    /// Middlewares will be inverted in execution order when receiving, allowing for encryption.
    ///
    /// For any byte[] x, a middleware needs to return x for OnReceive(OnSend(x))!
    ///
    /// If this middleware NEVER modifies the data, you may return false for DoesModify()
    /// All Middlewares returning true will always execute BEFORE any modifying middlewares.
    /// </summary>
    public interface INoaMiddleware
    {
        public MiddlewareTarget Target { get; }

        /// <summary>
        /// Called once before setup, and before calling DoesModify().
        /// </summary>
        public void Setup();

        /// <summary>
        /// Called every time a connection is fully established (AFTER the protocol check)
        /// </summary>
        /// <param name="connection"></param>
        public void OnConnectedServerside(ConnectionBase connection);
        
        public List<byte> OnSend(List<byte> data, ConnectionBase connection);
        
        public List<byte> OnReceive(List<byte> data, ConnectionBase connection);
    }
}