using System;
using System.Net;
using System.Reflection;
using MultiClip.Models;

namespace MultiClip.Network.Messages
{
    public class PingRequest : Request<PingRequest, PingResponse>
    {
        private const int Descriptor = 682111482;

        /// <summary>
        /// The user name of the logged-in user.
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// The name of the device.
        /// </summary>
        public string MachineName { get; set; }
        /// <summary>
        /// The version number of the application.
        /// </summary>
        public string Version { get; set; }

        private static byte[] s_bytes = null;

        public PingRequest()
        {
            UserName = Environment.UserName;
            MachineName = Environment.MachineName;
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        }

        public override byte[] GetBytes()
        {
            if (s_bytes == null)
            {
                s_bytes = base.GetBytes();
            }
            return (byte[])s_bytes.Clone();
        }

        public override IResponse GetResponse(IPAddress remoteIP)
        {
            return PingResponse.GetResponse();
        }
    }

    public class PingResponse : Response<PingResponse>
    {
        /// <summary>
        /// The unique identifier of the application instance.
        /// </summary>
        public Guid MachineGuid { get; set; }
        /// <summary>
        /// The user name of the logged-in user.
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// The name of the device.
        /// </summary>
        public string MachineName { get; set; }
        /// <summary>
        /// The version number of the application.
        /// </summary>
        public string Version { get; set; }

        public PingResponse()
        {

        }

        public static PingResponse GetResponse()
        {
            return new PingResponse
            {
                MachineGuid = AppState.Current.UserSettings.MachineGuid,
                UserName = Environment.UserName,
                MachineName = Environment.MachineName,
                Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3),
            };
        }
    }
}
