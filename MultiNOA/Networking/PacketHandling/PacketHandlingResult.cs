namespace MultiNOA.Networking.PacketHandling
{
    public readonly struct PacketHandlingResult
    {
        public readonly bool WasSucessful;

        public PacketHandlingResult(bool wasSucessful)
        {
            WasSucessful = wasSucessful;
        }
    }
}