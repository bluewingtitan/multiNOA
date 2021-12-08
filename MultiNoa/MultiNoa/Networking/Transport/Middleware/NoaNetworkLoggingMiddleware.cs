using System.Collections.Generic;
using MultiNoa.Logging;

namespace MultiNoa.Networking.Transport.Middleware
{
    public class NoaNetworkLoggingMiddleware: INoaMiddleware
    {
        public bool DoesModify() => false;

        public List<byte> OnSend(List<byte> data, ConnectionBase connection)
        {
            MultiNoaLoggingManager.Logger.Verbose($"Sending {data.Count} bytes to {connection.GetEndpointIp()}");
            return data;
        }

        public List<byte> OnReceive(List<byte> data, ConnectionBase connection)
        {
            MultiNoaLoggingManager.Logger.Verbose($"Received {data.Count} bytes from {connection.GetEndpointIp()}");
            return data;
        }
    }
}