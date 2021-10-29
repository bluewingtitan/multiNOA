using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using MultiNoa.Logging;
using MultiNOA.Networking.Common.NetworkData;
using MultiNOA.Networking.Common.NetworkData.DataContainer;

namespace MultiNoa.Networking.PacketHandling
{
    public static class DataContainerManager
    {
        private static readonly ConcurrentDictionary<Type, DataContainerInfo> Infos = new ConcurrentDictionary<Type, DataContainerInfo>();
        
        
        
        /// <summary>
        /// Use to register all packet structures in an assembly at once.
        /// </summary>
        /// <param name="a"></param>
        public static void RegisterAssembly(Assembly a)
        {
            var types = from t in a.DefinedTypes
                where t.IsDefined(typeof(DataContainer))
                select t;

            foreach (var typeInfo in types)
            {
                CacheDataContainer(typeInfo);
            }
        }
        
        public static void CacheDataContainer(Type t)
        {
            // Check if t is valid type.
            if (!(t.GetCustomAttribute(typeof(DataContainer)) is DataContainer attribute))
            {
                throw new ArgumentException($"Passed type {t.FullName} does not contain PacketClass Attribute");
            }

            if (Infos.ContainsKey(attribute.DataType) && !attribute.OverrideDefault)
            {
                MultiNoaLoggingManager.Logger.Warning($"Duplicate registration attempt of DataContainer for base type {attribute.DataType.FullName}. If you want to use the implementation of {t.FullName} set overrideDefault=true");
                return;
            }
            
            // Check if type is valid data container
            var isValid = t.GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(INetworkDataContainer<>) &&
                x.GetGenericArguments().Contains(attribute.DataType));

            if (!isValid)
            {
                MultiNoaLoggingManager.Logger.Warning($"Tried to register type {t.FullName} as data container for {attribute.DataType.FullName}, but type does not implement INetworkDataContainer<{attribute.DataType}");
                return;
            }
            
            
            var info = new DataContainerInfo(attribute.DataType, t, attribute);

            Infos[attribute.DataType] = info;
        }


        public static byte[] ToBytes(object o)
        {
            var t = o.GetType();

            if (!Infos.ContainsKey(t))
            {
                throw new Exception($"Tried to convert type without known converter: {t.FullName}.\nMake sure to cache a DataContainer for this type");
            }

            var info = Infos[t];

            if (!(Activator.CreateInstance(info.DataContainerType) is INetworkDataContainer container))
            {
                throw new Exception($"Was not able to instantiate {info.DataContainerType.FullName}. Does type have parameterless constructor?");
            }

            if (container.SetValue(o))
            {
                return container.TurnIntoBytes();
            }
            else
            {
                throw new Exception($"Was not able to set value of {info.DataContainerType.FullName} to object of type {t.FullName}");
            }
        }

        public static object ToValueType(byte[] b, Type t, out int byteLength)
        {
            if (!Infos.ContainsKey(t))
            {
                throw new Exception($"Tried to convert type without known converter: {t.FullName}.\nMake sure to cache a DataContainer for this type");
            }

            var info = Infos[t];

            if (!(Activator.CreateInstance(info.DataContainerType) is INetworkDataContainer container))
            {
                throw new Exception($"Was not able to instantiate {info.DataContainerType.FullName}. Does type have parameterless constructor?");
            }

            byteLength = container.LoadFromBytes(b);

            return container.GetValue();
        }

        public static T ToValueType<T>(byte[] b, out int byteLength)
        {
            return (T) ToValueType(b, typeof(T), out byteLength);
        }
        
    }


    internal class DataContainerInfo
    {
        public readonly Type DataType;
        public readonly Type DataContainerType;
        public readonly DataContainer DataContainerAttribute;

        public DataContainerInfo(Type dataType, Type dataContainerType, DataContainer dataContainerAttribute)
        {
            DataType = dataType;
            DataContainerType = dataContainerType;
            DataContainerAttribute = dataContainerAttribute;
        }
    }
    
    [AttributeUsage(AttributeTargets.Struct)]
    public class DataContainer : Attribute
    {
        public readonly bool OverrideDefault;
        public readonly Type DataType;
        public DataContainer(Type dataType, bool overrideDefault = false)
        {
            DataType = dataType;
            this.OverrideDefault = overrideDefault;
        }
    }
}