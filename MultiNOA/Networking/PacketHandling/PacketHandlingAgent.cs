using System;
using System.Linq;
using System.Reflection;
using MultiNOA.Attributes.PacketHandling;

namespace MultiNOA.Networking.PacketHandling
{
    /// <summary>
    /// Dynamically sits between the network layer and your PacketHandlers. Acts on a specific enum type.
    /// </summary>
    /// <typeparam name="T1">Enum Type</typeparam>
    public class PacketHandlingAgent<T1> where T1 : Enum
    {
        private readonly MethodInfo[] _methodInfos;
        private readonly bool _staticMode;
        private readonly Type _handlerType;
        private readonly IPacketHandler _handlerInstance;

        
        
        /// <summary>
        /// Creates a Packet Handling Agent that uses an Instance of a handler (use for non-static methods)
        /// </summary>
        /// <param name="handlerInstance">Instance of class containing handler methods</param>
        public PacketHandlingAgent(IPacketHandler handlerInstance)
        {
            _staticMode = false;
            _methodInfos = GetHandlerMethods(handlerInstance.GetType(), false);
            _handlerInstance = handlerInstance;
            _handlerType = handlerInstance.GetType();
        }

        
        
        /// <summary>
        /// Creates a Packet Handling Agent that uses static methods of a Type
        /// </summary>
        /// <param name="handlerType">Type that contains static methods</param>
        public PacketHandlingAgent(Type handlerType)
        {
            _handlerType = handlerType;
            _staticMode = true;
            _methodInfos = GetHandlerMethods(handlerType, true);
        }



        private MethodInfo[] GetHandlerMethods(Type t, bool staticMethods)
        {
            var methods = t.GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(PacketHandlerAttribute), false).Length > 0)
                .Where(m => m.IsStatic == staticMethods)
                .ToArray();
            return methods;
        }


    }

    public interface IPacketHandler
    {
        
    }
    
}