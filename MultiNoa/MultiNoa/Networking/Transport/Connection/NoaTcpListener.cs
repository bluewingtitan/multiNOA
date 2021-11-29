using System;
using System.Net;
using MultiNoa.Logging;

namespace MultiNoa.Networking.Transport.Connection
{
    public class NoaTcpListener: ConnectionListener
    {
        private readonly System.Net.Sockets.TcpListener _tcpListener;
        private bool _running = true;
        private readonly string _protocolVersion;
        
        public NoaTcpListener(ushort port, string protocolVersion) : base(port)
        {
            _protocolVersion = protocolVersion;
            _tcpListener = new System.Net.Sockets.TcpListener(IPAddress.Any, Port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
        }

        private void TcpConnectCallback(IAsyncResult result)
        {
            if(!_running) return;
            
            var client = _tcpListener.EndAcceptTcpClient(result);
            _tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
            MultiNoaLoggingManager.Logger.Verbose($"Incoming connection from {client.Client.RemoteEndPoint}...");
            
            var c = new TcpDistantConnection(() => { return;}, _protocolVersion);
            c.Connect(client);

            OnConnection?.Invoke(c);
        }

        public override event OnConnectionDelegate OnConnection;
        public override void StopListening()
        {
            _running = false;
            _tcpListener.Stop();
        }
    }
}