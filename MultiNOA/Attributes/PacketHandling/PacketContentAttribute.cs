using System;

namespace MultiNOA.Attributes.PacketHandling
{
    /// <summary>
    /// Sets a property as being part of a packet this class/struct represents.
    /// Specify an index for each property. Doubled indices will lead to overrides.
    ///
    /// Only works on bool, byte, string, int, float, long, doubles
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PacketContentAttribute
    {
        public readonly byte Index;
        public PacketContentAttribute(byte index)
        {
            Index = index;
        }
    }
}