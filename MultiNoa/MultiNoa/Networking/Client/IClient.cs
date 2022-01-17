using MultiNoa.Networking.Rooms;
using MultiNoa.Networking.Server;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.Client
{
    public interface IClient
    {
        public delegate void ClientEventDelegate(IClient client);

        event ClientEventDelegate OnClientConnected;
        event ClientEventDelegate OnClientReady;
        event ClientEventDelegate OnClientDisconnected;

        public void InvokeOnClientConnected();
        public void InvokeOnClientReady();
        public void InvokeOnClientDisconnected();
        
        
        public string Username { get; }
        public void SetUsername(string username, bool synced);
        public void SendData(object data);
        public ConnectionBase GetConnection();
        public void Disconnect();
    }

    public interface IGroupableClient
    {
        public bool GetAuthorityGroup(string group);
        public string[] GetAuthorityGroups();
        public void AddToGroup(string group);
    }

    public interface IUserSideClientAddons
    {
        public ulong IdOnServer { get; set; }
        public void SetUsername(string username);
    }
    
    public interface IUserSideClient: IClient, IUserSideClientAddons, IGroupableClient{}
    

    public interface IServersideClientAddons
    {
        public ServerBase GetServer();
        public ulong Id { get; }
        public Room Room { get; }
        public void MoveToRoomCallback(Room room);
    }

    public interface IServersideClient: IClient, IServersideClientAddons
    {
        
    }

    

    public interface IUniversalClient: IUserSideClient, IServersideClient, IGroupableClient
    {
    }

}