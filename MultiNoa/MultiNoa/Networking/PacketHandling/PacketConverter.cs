using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MultiNoa.Extensions;
using MultiNoa.Logging;
using MultiNoa.Networking.Data;
using MultiNoa.Networking.Data.DataContainer;

namespace MultiNoa.Networking.PacketHandling
{
    /// <summary>
    /// A helper class that allows the full conversion of 
    /// </summary>
    public static class PacketConverter
    {
        private static readonly ConcurrentDictionary<int, PacketClassInfo> Infos = new ConcurrentDictionary<int, PacketClassInfo>();
        
        /// <summary>
        /// Use to register all packet structures in an assembly at once.
        /// </summary>
        /// <param name="a"></param>
        public static void RegisterAssembly(Assembly a)
        {
            var types = from t in a.DefinedTypes
                where t.IsDefined(typeof(PacketStruct))
                select t;

            foreach (var typeInfo in types)
            {
                CachePacketStructure(typeInfo);
            }
        }
        
        /// <summary>
        /// May be used on program load to have all important packet structures ready.
        /// </summary>
        public static void CachePacketStructure(Type t)
        {
            // Check if t is valid type.
            if (!(t.GetCustomAttribute(typeof(PacketStruct)) is PacketStruct attribute))
            {
                throw new ArgumentException($"Passed type {t.FullName} does not contain PacketClass Attribute");
            }

            if (Infos.ContainsKey(attribute.PacketId))
            {
                MultiNoaLoggingManager.Logger.Warning($"Duplicate registration attempt of packet type with id #{attribute.PacketId}: {t.FullName}. Only keeping first scan");
                return;
            }

            var isInternal = false;
            if (attribute.PacketId < 0)
            {
                isInternal = (t.GetCustomAttribute(typeof(MultiNoaInternal)) is MultiNoaInternal);

                if (!isInternal)
                {
                    throw new CustomAttributeFormatException($"Attribute {typeof(HandlerMethod).FullName} should not use packet ids under 0, as those are reserved for multiNoa internal usage!\n" +
                                                             $"Problematic Attribute: {attribute.GetType().Name} at {t.FullName}");
                }
            }
            
            // sorry for the mess, but mum never told me to clean up my code...
            
            var props = t.GetProperties()
                .Where(e => e.GetCustomAttributes(typeof(NetworkProperty), true).Length > 0)
                .Select(e =>
                {
                    bool useDataContainerManager = !e.PropertyType.GetInterfaces().Contains(typeof(INetworkDataContainer));
                    // Check for valid property!

                    // Not getting single specific attribute, as other attributes will be implemented at this point later on too!
                    var data = e.GetCustomAttributes().ToArray();

                    foreach (var attr in data)
                        if (attr is NetworkProperty prop)
                        {
                            prop.useDataContainerManager = useDataContainerManager;
                            return new KeyValuePair<PropertyInfo, NetworkProperty>(e, prop);
                        }

                    return new KeyValuePair<PropertyInfo, NetworkProperty>(null, null);

                })
                .Where(e => e.Key != null && e.Value != null)
                .ToList();
            
            
            
            // Sorting properties of class by hashcode of name
            props.Sort((pair1, pair2) =>
                string.Compare(pair1.Key.Name, pair2.Key.Name, StringComparison.Ordinal)
            );
            
            Infos[attribute.PacketId] = new PacketClassInfo(t, new Dictionary<PropertyInfo, NetworkProperty>(props), attribute);
            if(!isInternal)
                MultiNoaLoggingManager.Logger.Debug($"Cached PacketConversion for type '{t.FullName}'");
        }


        public static List<byte> ObjectToByte(object o, bool skipTypeCheck = false, bool writeLength = false)
        {
            var bytes = new List<byte>();
            
            var type = o.GetType();

            if (!(type.GetCustomAttribute(typeof(PacketStruct)) is PacketStruct attribute))
            {
                throw new PacketConversionException($"Passed type {type.FullName} does not contain PacketClass Attribute");
            }


            if (!Infos.ContainsKey(attribute.PacketId))
            {
                CachePacketStructure(type);
            }
            
            var data = Infos[attribute.PacketId];

            if (!skipTypeCheck && data.Type != type)
                throw new PacketConversionException($"ID is not unique: {type.FullName} tried to use #{attribute.PacketId} assigned to {data.Type.FullName}");


            // write packet id into byte-array
            using (var nInt = new NetworkInt(attribute.PacketId))
            {
                bytes.AddRange(nInt.TurnIntoBytes());
            }
            

            foreach (var (prop, attributeData) in data.Props)
            {
                var value = prop.GetValue(o);

                if (value == null)
                {
                    MultiNoaLoggingManager.Logger.Warning(
                        $"Failed to convert property {prop.Name} to bytes: Prop was null");
                    continue;
                }
                
                if (attributeData.useDataContainerManager)
                {
                    bytes.AddRange(DataContainerManager.ToBytes(value));
                    continue;
                }

                // Already checked that prop implements INetworkDataContainer!
                if (value is INetworkDataContainer c)
                {
                    bytes.AddRange(c.TurnIntoBytes());
                    continue;
                }
                MultiNoaLoggingManager.Logger.Warning(
                    $"Failed to convert property {prop.Name} to bytes: Prop had no defined conversion method");
            }

            // Write length!
            if(writeLength) 
                bytes.InsertRange(0, new NetworkInt(bytes.Count).TurnIntoBytes());

            return bytes;
        }

        public static object BytesToObject(byte[] b, bool containsLength = false)
        {
            if (containsLength)
            {
                // Strip length
                b = b.GetSubarray(4, b.Length - 4);
            }

            var pId = 0;
            
            using (var idContainer = new NetworkInt())
            {
                idContainer.LoadFromBytes(b);
                b = b.GetSubarray(4, b.Length - 4);
                pId = idContainer.GetTypedValue();
            }
            

            if (!Infos.ContainsKey(pId))
            {
                throw new PacketConversionException($"No Packet Type known with Id #{pId}");
            }
            
            
            var data = Infos[pId];

            var type = data.Type;
            
            if (!(type.GetCustomAttribute(typeof(PacketStruct)) is PacketStruct attribute))
            {
                throw new PacketConversionException($"Type {type} does not contain PacketClass Attribute");
            }
            
            var instance = Activator.CreateInstance(type);



            foreach (var (prop, attributeData) in data.Props)
            {
                var l = 0;
                
                if (attributeData.useDataContainerManager)
                {
                    var value = DataContainerManager.ToValueType(b, prop.PropertyType, out l);
                    b = b.GetSubarray(l, b.Length - l);
                    prop.SetValue(instance, value);
                }
                else
                {
                    using (var container = Activator.CreateInstance(prop.PropertyType) as INetworkDataContainer)
                    {
                        if (container == null)
                        {
                            throw new PacketConversionException(
                                $"Was not able to create instance of INetworkDataContainer of type {prop.PropertyType.FullName}");
                        }
                        
                        l = container.LoadFromBytes(b);
                        b = b.GetSubarray(l, b.Length - l);

                        prop.SetValue(instance, container);
                    }
                }
            }

            return instance;
        }
        
        
        public static T BytesToObject<T>(byte[] b, out int readBytes, bool containsPacketId = true, bool skipTypeCheck = false, bool containsLength = false)
        {
            readBytes = 0;
            if (containsLength)
            {
                // Strip length
                b = b.GetSubarray(4, b.Length - 4);
                readBytes += 4;
            }
            
            var type = typeof(T);
            if (!(type.GetCustomAttribute(typeof(PacketStruct)) is PacketStruct attribute))
            {
                throw new PacketConversionException($"Passed type {type} does not contain PacketClass Attribute");
            }
            
            object instance = Activator.CreateInstance<T>();


            if (!Infos.ContainsKey(attribute.PacketId))
            {
                CachePacketStructure(type);
            }

            var data = Infos[attribute.PacketId];
            
            
            if (containsPacketId)
            {
                var pId = 0;
                using (var idContainer = new NetworkInt())
                {
                    readBytes += idContainer.LoadFromBytes(b);
                    b = b.GetSubarray(4, b.Length - 4);

                    pId = idContainer.GetTypedValue();
                }

                if (!skipTypeCheck && pId != attribute.PacketId)
                    throw new PacketConversionException($"Tried to parse packet with type-id #{pId} to {type.FullName}, should be #{attribute.PacketId}\n" +
                                                        $"Is parameter \"containsLength\" set right?");
            }


            foreach (var (prop, attributeData) in data.Props)
            {
                var l = 0;
                if (attributeData.useDataContainerManager)
                {
                    var value = DataContainerManager.ToValueType(b, prop.PropertyType, out l);
                    readBytes += l;
                    b = b.GetSubarray(l, b.Length - l);
                    prop.SetValue(instance, value);
                }
                else
                {
                    using (var container = Activator.CreateInstance(prop.PropertyType) as INetworkDataContainer)
                    {
                        if (container == null)
                        {
                            throw new PacketConversionException(
                                $"Was not able to create instance of INetworkDataContainer of type {prop.PropertyType.FullName}");
                        }
                        
                        l = container.LoadFromBytes(b);
                        readBytes += l;
                        b = b.GetSubarray(l, b.Length - l);

                        prop.SetValue(instance, container);
                    }
                }
            }

            return (T) instance;
        }
        
        
    }


    public class PacketConversionException : Exception
    {
        public PacketConversionException(string msg) : base(msg){}
    }

    internal class PacketClassInfo
    {
        internal readonly Type Type;
        internal readonly PacketStruct PacketStruct;
        internal readonly Dictionary<PropertyInfo, NetworkProperty> Props;

        public PacketClassInfo(Type type, Dictionary<PropertyInfo, NetworkProperty> props, PacketStruct packetStruct)
        {
            Props = props;
            PacketStruct = packetStruct;
            Type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class NetworkProperty : Attribute
    {
        internal bool useDataContainerManager = false;
        
        /// <summary>
        /// Grants extra information to the Packet Converter, including index (missing indices will be skipped) of the Property.
        /// </summary>
        /// <param name="index">Force specific index of field in/from (de-)serialized packet</param>
        public NetworkProperty()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public class PacketStruct : Attribute
    {
        public readonly int PacketId;
        public PacketStruct(int packetId)
        {
            PacketId = packetId;
        }
    }
    
    
    
    
}