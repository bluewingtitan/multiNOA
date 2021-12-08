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
        private static readonly IPacketHandler DefaultHandler = new PacketReflectionHandler();
        
        private string _address = null;
        private TcpClient _socket;
        private ClientBase _client;
        
        private NetworkStream _stream;
        private byte[] _receiveBuffer;

        private IDynamicThread currentThread;
        private readonly string _protocolVersion;

        private IPacketHandler _handler;

        private readonly ExecutionScheduler _handlers = new ExecutionScheduler();
        
        /// <summary>
        /// Constructs a new TcpConnection. Will use a PacketReflectionHandler if handler-parameter is not populated or null.
        /// </summary>
        public TcpConnection(string protocolVersion): base()
        {
            _protocolVersion = protocolVersion;
            _handler = DefaultHandler;
            this._client = null;
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
        
        public override void ChangeThread(IDynamicThread newThread)
        {
            if (currentThread != null)
            {
                currentThread.RemoveUpdatable(this); 
                currentThread = newThread;
            }
            newThread.AddUpdatable(this);
        }
        

        public override string GetProtocolVersion()
        {
            return _protocolVersion;
        }
        
        public override void SetPacketHandler(IPacketHandler newHandler)
        {
            _handler = newHandler;
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
                MultiNoaLoggingManager.Logger.Debug($"Sending {bytes} bytes to {GetEndpointIp()}");
                _stream.BeginWrite(data, 0, bytes, null, null);
            }
        }

        public override ClientBase GetClient()
        {
            return _client;
        }

        public override void SetClient(ClientBase client)
        {
            _client = client;
        }


        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLegth = _stream.EndRead(result);
                MultiNoaLoggingManager.Logger.Debug($"Received {byteLegth} bytes from {GetEndpointIp()}");
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
                MultiNoaLoggingManager.Logger.Debug($"Parsing packet of size {packetLenght}");
                
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

        public override void Update()
        {
            _handlers.ExecuteAll();
        }

        public override void PerSecondUpdate()
        {
            
        }
    }
}