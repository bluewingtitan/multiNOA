﻿using System;
using System.Net.Sockets;
using MultiNoa.GameSimulation;
using MultiNoa.Logging;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.Data.DataContainer;
using MultiNoa.Networking.PacketHandling;

namespace MultiNoa.Networking.Transport.Connection
{
    public class TcpDistantConnection: IConnection
    {
        private static readonly IPacketHandler DefaultHandler = new PacketReflectionHandler();
        public TcpClient Socket;

        private NetworkStream _stream;
        private Packet _receivedData;
        private ClientBase _client;
        private IPacketHandler _handler;
        private byte[] _receiveBuffer;
        private string _address;
        private readonly Action _onDisconnect;

        private IDynamicThread currentThread;
        
        
        private readonly ExecutionScheduler _handlers = new ExecutionScheduler();

        public TcpDistantConnection(Action onDisconnect)
        {
            _client = null;
            _handler = DefaultHandler;
            _onDisconnect = onDisconnect;
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
        
        public void ChangeThread(IDynamicThread newThread)
        {
            if (currentThread != null)
            {
                currentThread.RemoveUpdatable(this); 
                currentThread = newThread;
            }
            newThread.AddUpdatable(this);
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
            var bytes = data.Length;
            MultiNoaLoggingManager.Logger.Debug($"Sending {bytes} bytes to {GetEndpointIp()}");
            _stream.BeginWrite(data, 0, bytes, null, null);
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
    }
}