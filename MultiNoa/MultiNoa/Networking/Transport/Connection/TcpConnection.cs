using System;
using System.Net;
using System.Net.Sockets;
using MultiNoa.GameSimulation;
using MultiNoa.Logging;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.Data.DataContainer;
using MultiNoa.Networking.PacketHandling;

namespace MultiNoa.Networking.Transport.Connection
{
    public class TcpConnection: ConnectionBase
    {
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
                ReceiveBufferSize = ConnectionBase.DataBufferSize,
                SendBufferSize = ConnectionBase.DataBufferSize
            };

            _receiveBuffer = new byte[ConnectionBase.DataBufferSize];
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
            

            _stream.BeginRead(_receiveBuffer, 0, ConnectionBase.DataBufferSize, ReceiveCallback, null);
        }

        protected override void OnDisconnect()
        {
            _socket.Dispose();
        }

        public override string GetEndpointIp()
        {
            return _address;
        }

        protected override void TransferData(byte[] data)
        {
            if (_socket != null)
            {
                var bytes = data.Length;
                //MultiNoaLoggingManager.Logger.Debug($"Sending {bytes} bytes to {GetEndpointIp()}");
                _stream.BeginWrite(data, 0, bytes, null, null);
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

                HandleData(data);
                


                // Start listening again
                _stream.BeginRead(_receiveBuffer, 0, ConnectionBase.DataBufferSize, ReceiveCallback, null);
            }
            catch (Exception e)
            {
                MultiNoaLoggingManager.Logger.Error("Error receiving tcp: \n" + e.ToString());
                Disconnect();
            }
        }

        public override void PerSecondUpdate()
        {
            
        }
    }
}