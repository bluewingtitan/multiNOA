using System;
using MultiNoa.Matchmaking;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.Rooms;
using MultiNoa.Networking.Server;
using MultiNoa.Networking.Transport;

namespace ExampleProject
{
    public class DummyClient: IMatchmakingClient
    {
        private static ulong currId = 0;
        
        private readonly int mmr;
        private readonly ulong id;

        public DummyClient(int mmr)
        {
            this.mmr = mmr;
            id = currId++;
        }

        public event IClient.ClientEventDelegate OnClientConnected;
        public event IClient.ClientEventDelegate OnClientReady;
        public event IClient.ClientEventDelegate OnClientDisconnected;
        public void InvokeOnClientConnected()
        {
            
        }

        public void InvokeOnClientReady()
        {
            
        }

        public void InvokeOnClientDisconnected()
        {
            
        }

        public string GetUsername()
        {
            return id + ":" + mmr;
        }

        public void SetUsername(string username, bool synced)
        {
            
        }

        public void SendData(object data)
        {
            
        }

        public ConnectionBase GetConnection()
        {
            return null;
        }

        public void Disconnect()
        {
            
        }

        public ServerBase GetServer()
        {
            return null;
        }

        public ulong GetId()
        {
            return 0;
        }

        public Room GetRoom()
        {
            return null;
        }

        public void MoveToRoom(Room room)
        {
            
        }

        public bool GetAuthorityGroup(string @group)
        {
            return true;
        }

        public string[] GetAuthorityGroups()
        {
            return Array.Empty<string>();
        }

        public void AddToGroup(string @group)
        {
            
        }

        public int GetMmr(int channel)
        {
            return mmr;
        }
    }
}