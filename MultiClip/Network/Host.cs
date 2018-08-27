using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MultiClip.Models;

namespace MultiClip.Network
{
    /// <summary>
    /// Describes a network instance of the application.
    /// </summary>
    public class Host
    {
        /// <summary>
        /// The unique identifier of the application instance.
        /// </summary>
        public Guid MachineGuid { get; private set; }
        /// <summary>
        /// The IP address of the host machine.
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }
        /// <summary>
        /// The name of the active user on the machine.
        /// </summary>
        public string UserName { get; private set; }
        /// <summary>
        /// The public name of the device.
        /// </summary>
        public string MachineName { get; private set; }

        /// <summary>
        /// A Host object representing the current machine.
        /// </summary>
        public static Host Loopback => new Host(AppState.Current.UserSettings.MachineGuid, NetConfig.Loopback, Environment.UserName, Environment.MachineName);

        public Host(Guid instanceId, IPEndPoint endPoint, string userName, string machineName)
        {
            MachineGuid = instanceId;
            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            MachineName = machineName ?? throw new ArgumentNullException(nameof(machineName));
        }

        public override bool Equals(object obj)
        {
            return obj is Host && Equals(MachineGuid, ((Host)obj).MachineGuid);
        }

        public override int GetHashCode()
        {
            return MachineGuid.GetHashCode();
        }

        public static async Task<TResponse> SendAsync<TRequest, TResponse>(IPEndPoint endPoint, Request<TRequest, TResponse> request)
            where TRequest : Request<TRequest, TResponse>
            where TResponse : Response<TResponse>
        {
            request = request ?? throw new ArgumentNullException(nameof(request));
            byte[] bytes = request.GetBytes();

            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(endPoint.Address, endPoint.Port);
                using (NetworkStream stream = client.GetStream())
                {
                    await NetUtils.WriteInt32Async(stream, bytes.Length);
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                
                    int responseSize = await NetUtils.ReadInt32Async(stream);
                    byte[] responseBytes = await NetUtils.ReadAsync(stream, responseSize);

                    return Response<TResponse>.FromBytes(responseBytes);
                }
            }
        }

        public Task<TResponse> SendAsync<TRequest, TResponse>(Request<TRequest, TResponse> request)
            where TRequest : Request<TRequest, TResponse>
            where TResponse : Response<TResponse>
        {
            return SendAsync(EndPoint, request);
        }
    }
}
