using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace MultiNoa.Networking.PacketHandling
{
    public static class PacketConverter
    {
        private static readonly ConcurrentDictionary<Type, PacketClassInfo> Infos = new ConcurrentDictionary<Type, PacketClassInfo>();

        /// <summary>
        /// May be used on program load to have all important packet structures ready.
        /// </summary>
        public static void CachePacketStructure(Type t)
        {
            var props = t.GetProperties()
                .Where(e => e.GetCustomAttributes(typeof(NetworkProperty), true).Length > 0)
                .Select(e =>
                {
                    var data = e.GetCustomAttributesData();

                    for (int i = 0; i < data.Count; i++)
                        if (data[i].AttributeType == typeof(NetworkProperty))
                            return new KeyValuePair<PropertyInfo, CustomAttributeData>(e, data[i]);

                    return new KeyValuePair<PropertyInfo, CustomAttributeData>(null, null);

                })
                .Where(e => e.Key != null && e.Value != null)
                .ToList();
            
            props.Sort((pair, valuePair) =>
                ((int) pair.Value.ConstructorArguments[0].Value) -
                ((int) valuePair.Value.ConstructorArguments[0].Value)
            );
            
            Infos[t] = new PacketClassInfo(new Dictionary<PropertyInfo, CustomAttributeData>());
        }


        public static byte[] ObjectToByte(object o)
        {
            var bytes = new List<byte>();
            
            var type = o.GetType();
            if (!(type.IsValueType && !type.IsEnum))
            {
                throw new ArgumentException("Passed object is not a class or struct.");
            }


            if (!Infos.ContainsKey(type))
            {
                CachePacketStructure(type);
            }

            var data = Infos[type];


            foreach (var (prop, attributeData) in data.Props)
            {
                // TODO: Do actual conversion
                prop.GetValue(o);
            }

            return bytes.ToArray();
        }
        
        
    }

    internal class PacketClassInfo
    {
        internal readonly Dictionary<PropertyInfo, CustomAttributeData> Props;

        public PacketClassInfo(Dictionary<PropertyInfo, CustomAttributeData> props)
        {
            Props = props;
        }
        
        
        
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class NetworkProperty : System.Attribute
    {
        public readonly int Index = 0;
        
        /// <summary>
        /// Grants extra information to the Packet Converter, including index (missing indices will be skipped) of the Property.
        /// </summary>
        /// <param name="index">Force specific index of field in/from (de-)serialized packet</param>
        public NetworkProperty(int index = -1)
        {
            Index = index;
        }
    }
}