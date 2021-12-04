using MultiNoa.Logging;

namespace MultiNoa.Networking.Transport.Middleware
{
    public class NoaNetworkLoggingMiddleware: INoaMiddleware
    {
        public bool DoesModify()
        {
            return false;
        }

        public byte[] OnSend(byte[] data, ConnectionBase connection)
        {
            MultiNoaLoggingManager.Logger.Verbose($"Sending {data.Length} bytes to {connection.GetEndpointIp()}");
            return data;
        }

        public byte[] OnReceive(byte[] data, ConnectionBase connection)
        {
            MultiNoaLoggingManager.Logger.Verbose($"Received {data.Length} bytes from {connection.GetEndpointIp()}");
            return data;
        }
    }
}