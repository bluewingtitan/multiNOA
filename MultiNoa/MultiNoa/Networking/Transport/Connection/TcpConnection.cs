using System;
using System.Net.Sockets;
using System.Threading;
using MultiNoa.Logging;

namespace MultiNoa.Networking.Transport.Connection
{
    public class TcpConnection: ConnectionBase
    {
        private readonly object _lockObj = 0;
        
        private string _address = null;
        private TcpClient _socket;
        
        private NetworkStream _stream;
        private byte[] _receiveBuffer;

        /// <summary>
        /// Constructs a new TcpConnection. Will use a PacketReflectionHandler if handler-parameter is not populated or null.
        /// </summary>
        public TcpConnection(string protocolVersion): base(protocolVersion)
        {
        }

        public void Connect(string serverIp, int port)
        {
            _address = serverIp;
            
            _socket = new TcpClient
            {
                ReceiveBufferSize = MultiNoaSetup.DataBufferSize,
                SendBufferSize = MultiNoaSetup.DataBufferSize
            };

            _receiveBuffer = new byte[MultiNoaSetup.DataBufferSize];
            _socket.BeginConnect(serverIp, port, ConnectCallback, _socket);
        }
        
        private void ConnectCallback(IAsyncResult result)
        {
            _socket.EndConnect(result);

            if (!_socket.Connected)
            {
                return;
            }

            _stream = _socket.GetStream();
            

            _stream.BeginRead(_receiveBuffer, 0, MultiNoaSetup.DataBufferSize, ReceiveCallback, null);
        }

        protected override void OnDisconnect()
        {
            _socket.Dispose();
            _stream.Dispose();
            _stream = null;
            _receiveBuffer = null;
            _socket = null;
        }

        public override string GetEndpointIp()
        {
            return _address;
        }

        protected override void TransferData(byte[] data)
        {
            if (_socket == null) return;
            var bytes = data.Length;

            // Never write into the stream multiple times!
            lock (_lockObj)
            {
                _stream.BeginWrite(data, 0, bytes, null, null).AsyncWaitHandle.WaitOne();
            }
        }


        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = _stream.EndRead(result);
                //MultiNoaLoggingManager.Logger.Debug($"Received {byteLength} bytes from {GetEndpointIp()}");
                if (byteLength <= 0)
                {
                    //Disconnect();
                    return;
                }
                
                byte[] data = new byte[byteLength];
                Array.Copy(_receiveBuffer, data, byteLength);
                
                // Start listening again
                _stream.BeginRead(_receiveBuffer, 0, MultiNoaSetup.DataBufferSize, ReceiveCallback, null);
                
                // Handle the data
                HandleData(data);
            }
            catch (Exception e)
            {
                MultiNoaLoggingManager.Logger.Error("Error receiving tcp: \n" + e.ToString());
                Disconnect();
            }
        }

    }
}