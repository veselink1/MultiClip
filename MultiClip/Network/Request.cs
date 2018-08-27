using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace MultiClip.Network
{
    public interface IResponse
    {
        byte[] GetBytes();
    }

    public abstract class Response<TResponse> : IResponse
        where TResponse : Response<TResponse>
    {
        public byte[] GetBytes()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BsonDataWriter(stream))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, this);
                return stream.ToArray();
            }
        }

        public static TResponse FromBytes(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            using (var reader = new BsonDataReader(stream))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<TResponse>(reader);
            }
        }
    }

    public interface IRequest
    {
        IResponse GetResponse(IPAddress remoteIP);
        byte[] GetBytes();
    }

    public static class Request
    {
        internal static readonly ConcurrentDictionary<int, Type> RequestTypes = new ConcurrentDictionary<int, Type>();

        public static IRequest FromBytes(byte[] bytes)
        {
            byte[] descriptorBytes = bytes.Take(4).ToArray();
            int descriptor = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(descriptorBytes, 0));
            
            if (RequestTypes.TryGetValue(descriptor, out Type type))
            {
                using (var stream = new MemoryStream(bytes, 4, bytes.Length - 4))
                using (var reader = new BsonDataReader(stream))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (IRequest)serializer.Deserialize(reader, type);
                }
            }
            else
            {
                throw new ArgumentException("Cannot parse bytes as a request object. Unknown descriptor!");
            }
        }
    }

    public abstract class Request<TRequest, TResponse> : IRequest
        where TRequest : Request<TRequest, TResponse>
        where TResponse : Response<TResponse>
    {
        private static readonly int Descriptor;

        [JsonIgnore]
        public Guid SenderGuid { get; set; }

        static Request()
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = typeof(TRequest).GetField("Descriptor", flags);
            if (field == null || field.FieldType != typeof(int) || !field.IsLiteral || field.IsInitOnly)
            {
                throw new InvalidProgramException(
                    $"Type '{typeof(TRequest)}' is derived from Request<T, U> but " +
                    $"does not declare a field in the form [const Int32 Descriptor]!");
            }

            Descriptor = (int)field.GetValue(null);
            Type type = Request.RequestTypes.GetOrAdd(Descriptor, typeof(TRequest));
            if (type != typeof(TRequest))
            {
                throw new InvalidProgramException(
                    $"No two request types ({typeof(TRequest)} and {type}) can have the same descriptor!");
            }
        }

        public virtual byte[] GetBytes()
        {
            using (var stream = new MemoryStream())
            {
                byte[] descriptorBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Descriptor));
                stream.Write(descriptorBytes, 0, descriptorBytes.Length);

                using (var writer = new BsonDataWriter(stream))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, this);
                }

                return stream.ToArray();
            }
        }

        public abstract IResponse GetResponse(IPAddress remoteIP);
    }
}
