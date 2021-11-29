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
    public class TcpConnection: IConnection
    {
        private static readonly IPacketHandler DefaultHandler = new PacketReflectionHandler();
        
        private string _address = null;
        private TcpClient _socket;
        private ClientBase _client;
        
        private NetworkStream _stream;
        private byte[] _receiveBuffer;
        private readonly Action _onDisconnect;

        private IDynamicThread currentThread;
        private readonly string _protocolVersion;

        private IPacketHandler _handler;

        private readonly ExecutionScheduler _handlers = new ExecutionScheduler();
        
        /// <summary>
        /// Constructs a new TcpConnection. Will use a PacketReflectionHandler if handler-parameter is not populated or null.
        /// </summary>
        /// <param name="onDisconnect">Callback handling a disconnect</param>
        /// <param name="client">client managing this connection</param>
        /// <param name="handler">Packet handler to use</param>
        public TcpConnection(Action onDisconnect, string protocolVersion)
        {
            _protocolVersion = protocolVersion;
            _onDisconnect = onDisconnect;
            _handler = DefaultHandler;
            this._client = null;
        }

        public void Connect(string serverIp, int port)
        {
            _address = serverIp;
            
            _socket = new TcpClient
            {
                ReceiveBufferSize = IConnection.DataBufferSize,
                SendBufferSize = IConnection.DataBufferSize
            };

            _receiveBuffer = new byte[IConnection.DataBufferSize];
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
            

            _stream.BeginRead(_receiveBuffer, 0, IConnection.DataBufferSize, ReceiveCallback, null);
        }

        public void Disconnect()
        {
            _socket.Dispose();
            _onDisconnect?.Invoke();
        }
        
        public void ChangeThread(IDynamicThread newThread)
        {
            if (currentThread != null)
            {
                currentThread.RemoveUpdatable(this); 
                currentThread = newThread;
            }
            newThread.AddUpdatable(this);
        }
        

        public string GetProtocolVersion()
        {
            return _protocolVersion;
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
            if (_socket != null)
            {
                var bytes = data.Length;
                MultiNoaLoggingManager.Logger.Debug($"Sending {bytes} bytes to {GetEndpointIp()}");
                _stream.BeginWrite(data, 0, bytes, null, null);
            }
        }

        public ClientBase GetClient()
        {
            return _client;
        }

        public void SetClient(ClientBase client)
        {
            _client = client;
            ChangeThread(client.GetRoom().GetRoomThread());
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
                _stream.BeginRead(_receiveBuffer, 0, IConnection.DataBufferSize, ReceiveCallback, null);
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
    }
}