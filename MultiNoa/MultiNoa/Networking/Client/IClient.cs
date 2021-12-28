using MultiNoa.Networking.Rooms;
using MultiNoa.Networking.Server;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.Client
{
    public interface IClient
    {
        public delegate void ClientReadyDelegate(IClient client);

        public void AddOnClientConnected(ClientReadyDelegate callback);
        public void RemoveOnClientConnected(ClientReadyDelegate callback);
        public void InvokeOnClientConnected();
        
        public void AddOnOnClientReady(ClientReadyDelegate callback);
        public void RemoveOnClientReady(ClientReadyDelegate callback);
        public void InvokeOnClientReady();
        
        
        public string GetUsername();
        public void SetUsername(string username, bool synced);
        public void SendData(object data);
        public ConnectionBase GetConnection();
        public void Disconnect();
    }

    public interface IUserSideClientAddons
    {
        public void SetUsername(string username);
    }
    
    public interface IUserSideClient: IClient, IUserSideClientAddons{}
    

    public interface IServersideClientAddons
    {
        public ServerBase GetServer();
        public ulong GetId();
        public Room GetRoom();
        public void MoveToRoom(Room room);

        public bool GetAuthorityGroup(string group);
        public string[] GetAuthorityGroups();
        public void AddToGroup(string group);
    }

    public interface IServersideClient: IClient, IServersideClientAddons
    {
        
    }

    

    public interface IUniversalClient: IUserSideClient, IServersideClient
    {
    }

}