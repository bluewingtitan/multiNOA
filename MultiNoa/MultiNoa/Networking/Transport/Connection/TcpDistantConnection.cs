using System;
using System.Net.Sockets;
using MultiNoa.GameSimulation;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.Data.DataContainer;
using MultiNoa.Networking.PacketHandling;

namespace MultiNoa.Networking.Transport.Connection
{
    public class TcpDistantConnection: IConnection
    {
        public TcpClient Socket;

        private NetworkStream _stream;
        private Packet _receivedData;
        private readonly IClient _client;
        private IPacketHandler _handler;
        private byte[] _receiveBuffer;
        private string _address;
        private readonly Action _onDisconnect;
        
        
        private readonly ExecutionScheduler _handlers = new ExecutionScheduler();

        public TcpDistantConnection(IClient client, IPacketHandler handler, Action onDisconnect)
        {
            this._client = client;
            _handler = handler;
            _onDisconnect = onDisconnect;
            
            client.GetServer().GetServerThread().AddUpdatable(this);
        }
        
        public void Connect(TcpClient socket)
        {
            Socket = socket;
            Socket.ReceiveBufferSize = IConnection.DataBufferSize;
            Socket.SendBufferSize = IConnection.DataBufferSize;

            _stream = Socket.GetStream();

            _receivedData = new Packet();
            _receiveBuffer = new byte[IConnection.DataBufferSize];

            _stream.BeginRead(_receiveBuffer, 0, IConnection.DataBufferSize, ReceiveCallback, null);

            _address = socket.Client.RemoteEndPoint.ToString();

        }
        
        public void Disconnect()
        {
            Socket?.Close();
            _stream = null;
            _receivedData = null;
            Socket = null;
            _onDisconnect?.Invoke();
        }
        


        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLegth = _stream.EndRead(result);
                if (byteLegth <= 0)
                {
                    Disconnect();
                    return;
                }
                
                byte[] data = new byte[byteLegth];
                Array.Copy(_receiveBuffer, data, byteLegth);

                HandleData(data);
                


                // Start listening again
                _stream.BeginRead(_receiveBuffer, 0, IConnection.DataBufferSize, ReceiveCallback, null);
            }
            catch (Exception)
            {
                Disconnect();
            }
        }
        
        
        /// <summary>
        /// Handles a byte-array containing one or multiple individual packets
        /// </summary>
        /// <param name="data"></param>
        private void HandleData(byte[] data)
        {
            var packetLenght = 0;
            var packet = new Packet(data);

            if (packet.UnreadLength() >= 4)
            {
                packetLenght = packet.Read<NetworkInt>().GetTypedValue();
                if (packetLenght <= 0)
                {
                    return;
                }
            }
            
            while (packetLenght > 0 && packetLenght <= packet.UnreadLength())
            {
                
                // Do packet analysis now and prepare/schedule handling for next tick
                byte[] packetBytes = packet.ReadBytes(packetLenght);
                _handlers.ScheduleExecution(_handler.PrepareHandling(packetBytes, this));
                
                // Analyze next packet contained in bytes
                packetLenght = 0;
                if (packet.UnreadLength() >= 4)
                {
                    packetLenght = packet.Read<NetworkInt>().GetTypedValue();
                    if (packetLenght <= 0)
                    {
                        return;
                    }
                }

                if (packetLenght <= 1)
                {
                    return;
                }
            }
        }
        
        
        
        public void Update()
        {
            _handlers.ExecuteAll();
        }

        public void PerSecondUpdate()
        {
            
        }

        public void SetPacketHandler(IPacketHandler newHandler)
        {
            _handler = newHandler;
        }

        public string GetEndpointIp()
        {
            return _address;
        }

        public void SendData(byte[] data)
        {
            _stream.BeginWrite(data, 0, data.Length, null, null);
        }

        public IClient GetClient()
        {
            return _client;
        }
    }
}