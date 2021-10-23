using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using MultiNoa.GameSimulation;
using MultiNoa.Networking.Client;
using MultiNOA.Networking.Common.NetworkData.DataContainer;
using MultiNoa.Networking.PacketHandling;

namespace MultiNoa.Networking.Transport.Connection
{
    public class TcpConnection: IConnection
    {
        private const int DataBufferSize = 4096;
        private IPAddress _address = null;
        private TcpClient Socket;
        private readonly IClient client;
        
        private NetworkStream stream;
        private byte[] receiveBuffer;
        private Action _onDisconnect;

        private IPacketHandler _handler;

        private ExecutionScheduler _handlers = new ExecutionScheduler();
        
        public TcpConnection(Action onDisconnect, IPacketHandler handler, IClient client)
        {
            _onDisconnect = onDisconnect;
            _handler = handler;
            this.client = client;
        }
        
        public void Connect(IPAddress serverIp, int port)
        {
            _address = serverIp;
            
            Socket = new TcpClient
            {
                ReceiveBufferSize = DataBufferSize,
                SendBufferSize = DataBufferSize
            };

            receiveBuffer = new byte[DataBufferSize];
            Socket.BeginConnect(serverIp, port, ConnectCallback, Socket);
        }
        
        private void ConnectCallback(IAsyncResult result)
        {
            Socket.EndConnect(result);

            if (!Socket.Connected)
            {
                return;
            }

            stream = Socket.GetStream();
            

            stream.BeginRead(receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
        }

        public void Disconnect()
        {
            Socket.Dispose();
            _onDisconnect?.Invoke();
        }
        
        public void SetPacketHandler(IPacketHandler newHandler)
        {
            _handler = newHandler;
        }

        public IPAddress GetEndpointIp()
        {
            return _address;
        }

        public void SendData(byte[] data)
        {
            if (Socket != null)
            {
                stream.BeginWrite(data, 0, data.Length, null, null);
            }
        }

        public IClient GetClient()
        {
            return client;
        }


        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLegth = stream.EndRead(result);
                if (byteLegth <= 0)
                {
                    Disconnect();
                    return;
                }
                
                byte[] data = new byte[byteLegth];
                Array.Copy(receiveBuffer, data, byteLegth);

                HandleData(data);
                


                // Start listening again
                stream.BeginRead(receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
            }
            catch (Exception e)
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
                packetLenght = packet.Read<NetworkInt>().GetValue();
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
                    packetLenght = packet.Read<NetworkInt>().GetValue();
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
    }
}