using System;
using System.Net;
using System.Net.Sockets;
using MultiNoa.Logging;

namespace MultiNoa.Networking.Transport.Connection
{
    /// <summary>
    /// Utility Class simplifying working with Tcp by creating a basic layer between
    /// handling Tcp-Listening and the data-handling itself.
    /// </summary>
    public class NoaTcpListener
    {
        private readonly TcpListener _tcpListener;
        private bool _running = true;

        public delegate void TcpConnectCallbackDelegate(TcpClient client);

        private readonly TcpConnectCallbackDelegate _fallback;


        public NoaTcpListener(int port, TcpConnectCallbackDelegate fallback)
        {
            _tcpListener = new TcpListener(IPAddress.Any, port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);

            _fallback = fallback;
        }


        private void TcpConnectCallback(IAsyncResult result)
        {
            if(!_running) return;
            var client = _tcpListener.EndAcceptTcpClient(result);
            _tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
            MultiNoaLoggingManager.Logger.Verbose($"Incoming connection from {client.Client.RemoteEndPoint}...");
            
            _fallback.Invoke(client);
        }


        public void StopListening()
        {
            _running = false;
            _tcpListener.Stop();
        }
        
    }
}