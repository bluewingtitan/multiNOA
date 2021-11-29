namespace MultiNoa.Networking.Transport
{
    /// <summary>
    /// Interface for classes listening for incoming connections and handing them over to the server.
    /// </summary>
    public abstract class ConnectionListener
    {
        
        public delegate void OnConnectionDelegate(IConnection connection);

        protected ushort Port { get; }

        // Abstracted to make sure people use it in their implementation => needs to be part of it.
        public abstract event OnConnectionDelegate OnConnection;

        protected ConnectionListener(ushort port)
        {
            Port = port;
        }

        public abstract void StopListening();

    }
}