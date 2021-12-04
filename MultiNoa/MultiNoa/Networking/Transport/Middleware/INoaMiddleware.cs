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
        /// <summary>
        /// Only executed once when registering the middleware.
        ///
        /// If this returns false, results of the middleware won't be stored or applied onto the chain later on.
        /// </summary>
        /// <returns>If this middleware EVER modifies the input data</returns>
        public bool DoesModify();
        public byte[] OnSend(byte[] data, ConnectionBase connection);
        public byte[] OnReceive(byte[] data, ConnectionBase connection);
    }
}