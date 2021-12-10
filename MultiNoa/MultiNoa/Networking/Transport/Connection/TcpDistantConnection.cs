using System;
using System.Net.Sockets;
using MultiNoa.GameSimulation;
using MultiNoa.Logging;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.Data.DataContainer;
using MultiNoa.Networking.PacketHandling;

namespace MultiNoa.Networking.Transport.Connection
{
    public class TcpDistantConnection: ConnectionBase
    {
        public TcpClient Socket;

        private NetworkStream _stream;
        private Packet _receivedData;
        private byte[] _receiveBuffer;
        private string _address;

        public TcpDistantConnection(string protocolVersion): base(protocolVersion)
        {
        }

        public void Connect(TcpClient socket)
        {
            Socket = socket;
            Socket.ReceiveBufferSize = ConnectionBase.DataBufferSize;
            Socket.SendBufferSize = ConnectionBase.DataBufferSize;

            _stream = Socket.GetStream();

            _receivedData = new Packet();
            _receiveBuffer = new byte[ConnectionBase.DataBufferSize];

            _stream.BeginRead(_receiveBuffer, 0, ConnectionBase.DataBufferSize, ReceiveCallback, null);

            _address = socket.Client.RemoteEndPoint.ToString();
        }
        
        protected override void OnDisconnect()
        {
            Socket?.Close();
            _stream = null;
            _receivedData = null;
            Socket = null;
        }
        


        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLegth = _stream.EndRead(result);
                //MultiNoaLoggingManager.Logger.Debug($"Received {byteLegth} bytes from {GetEndpointIp()}");
                if (byteLegth <= 0)
                {
                    Disconnect();
                    return;
                }
                
                byte[] data = new byte[byteLegth];
                Array.Copy(_receiveBuffer, data, byteLegth);

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

        public override string GetEndpointIp()
        {
            return _address;
        }

        protected override void TransferData(byte[] data)
        {
            var bytes = data.Length;
            //MultiNoaLoggingManager.Logger.Debug($"Sending {bytes} bytes to {GetEndpointIp()}");
            _stream.BeginWrite(data, 0, bytes, null, null);
        }

        
        
    }
}