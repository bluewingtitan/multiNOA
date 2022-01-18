using System;
using System.Net.Sockets;
using MultiNoa.Logging;

namespace MultiNoa.Networking.Transport.Connection
{
    public class TcpDistantConnection: ConnectionBase
    {
        public TcpClient Socket;
        private readonly object _lockObj = 0;

        private NetworkStream _stream;
        private Packet _receivedData;
        private byte[] _receiveBuffer;
        private string _address;

        public TcpDistantConnection(string protocolVersion): base(protocolVersion)
        {}

        public void Connect(TcpClient socket)
        {
            Socket = socket;
            Socket.ReceiveBufferSize = MultiNoaSetup.DataBufferSize;
            Socket.SendBufferSize = MultiNoaSetup.DataBufferSize;

            _stream = Socket.GetStream();

            _receivedData = new Packet();
            _receiveBuffer = new byte[MultiNoaSetup.DataBufferSize];

            _stream.BeginRead(_receiveBuffer, 0, MultiNoaSetup.DataBufferSize, ReceiveCallback, null);

            _address = socket.Client.RemoteEndPoint.ToString();
        }
        
        protected override void OnDisconnect()
        {
            Socket?.Close();
            _stream.Dispose();
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
                _stream.BeginRead(_receiveBuffer, 0, MultiNoaSetup.DataBufferSize, ReceiveCallback, null);
            }
            catch (Exception e)
            {
                MultiNoaLoggingManager.Logger.Error("Error receiving tcp: \n" + e.ToString());
                Disconnect();
            }
        }
        

        public override string GetEndpointIp()
        {
            return _address;
        }

        protected override void TransferData(byte[] data)
        {
            var bytes = data.Length;
            lock (_lockObj)
            {
                _stream.BeginWrite(data, 0, bytes, null, null).AsyncWaitHandle.WaitOne();
            }
        }

        
        
    }
}