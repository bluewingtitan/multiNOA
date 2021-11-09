using MultiNoa.Networking.Data.DataContainer;
using MultiNoa.Networking.PacketHandling;

namespace ExampleProject.Packets
{
    [PacketStruct(PacketId.Message)]
    public struct Message
    {
        [NetworkProperty] public NetworkInt SenderId { get; private set; }
        [NetworkProperty] public NetworkString Title { get; private set; }
        [NetworkProperty] public NetworkString MainText{ get; private set; }

        public Message(int senderId, string title, string mainText)
        {
            SenderId = new NetworkInt(senderId);
            Title = new NetworkString(title);
            MainText = new NetworkString(mainText);
        }

        public override string ToString()
        {
            return "Sent by: " + SenderId.GetTypedValue() + "\n"
                + "Title: " + Title.GetTypedValue() + "\n" +
                "Message: " + MainText.GetTypedValue();
        }
    }
}