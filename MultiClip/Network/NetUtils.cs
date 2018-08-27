using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MultiClip.Network
{
    public static class NetUtils
    {
        public static byte[] GetBytes(int value)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
        }

        public static int ToInt32(byte[] value)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(value, 0));
        }

        public static int ReadInt32(NetworkStream stream)
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            return ToInt32(bytes);
        }

        public static void WriteInt32(NetworkStream stream, int value)
        {
            stream.Write(GetBytes(value), 0, 4);
        }

        public static async Task<int> ReadInt32Async(NetworkStream stream)
        {
            byte[] bytes = new byte[4];
            await stream.ReadAsync(bytes, 0, 4);
            return ToInt32(bytes);
        }

        public static Task WriteInt32Async(NetworkStream stream, int value)
        {
            return stream.WriteAsync(GetBytes(value), 0, 4);
        }

        public static byte[] Read(NetworkStream stream, int count)
        {
            byte[] bytes = new byte[count];
            stream.Read(bytes, 0, count);
            return bytes;
        }

        public static async Task<byte[]> ReadAsync(NetworkStream stream, int count)
        {
            byte[] bytes = new byte[count];
            await stream.ReadAsync(bytes, 0, count);
            return bytes;
        }

        public static void Write(NetworkStream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        public static Task WriteAsync(NetworkStream stream, byte[] bytes)
        {
            return stream.WriteAsync(bytes, 0, bytes.Length);
        }

        public static Task ConnectAsync(TcpClient client, IPAddress ipAddress, int port, int timeoutMs)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            client.ConnectAsync(ipAddress, port)
                .ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        tcs.TrySetResult(null);
                    }
                    else if (task.IsFaulted)
                    {
                        tcs.TrySetException(task.Exception);
                    }
                    else if (task.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                });
            Task.Delay(timeoutMs)
                .ContinueWith(task =>
                {
                    tcs.TrySetException(new TimeoutException());
                });
            return tcs.Task;
        }
    }
}
