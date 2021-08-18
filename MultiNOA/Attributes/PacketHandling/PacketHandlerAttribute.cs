using System;
using System.Linq;
using System.Reflection;

namespace MultiNOA.Attributes.PacketHandling
{
    /// <summary>
    /// Marks a method as handler for a specific packet type.
    /// Use to create a packet handler class, usable together with PacketHandler
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PacketHandlerAttribute: Attribute
    {
        public readonly bool IsValid;
        public readonly Enum ActOn;
        public readonly Type PacketType;
        
        public PacketHandlerAttribute(Type actOnEnum, string enumString,Type packetType)
        {
            ActOn = (Enum) Enum.Parse(actOnEnum, enumString);
            PacketType = packetType;
            // Check for parameterless constructor
            ConstructorInfo parameterlessConstructor = null;
            foreach (var constructorInfo in packetType.GetConstructors())
            {
                if (!constructorInfo.IsAbstract && constructorInfo.GetParameters().Length == 0)
                {
                    parameterlessConstructor = constructorInfo;
                }
            }

            if (parameterlessConstructor == null)
            {
                IsValid = false;
                throw new ArgumentException($"Not able to use {packetType.Name} as packet type: no parameterless constructor.");
            }
            else
            {
                IsValid = true;
            }
        }
    }
}