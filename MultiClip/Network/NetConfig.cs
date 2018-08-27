using System.Net;

namespace MultiClip.Network
{
    public static class NetConfig
    {
        /// <summary>
        /// The local port, on which all instances of the application communicate.
        /// </summary>
        public const int Port = 9583;

        /// <summary>
        /// The size of the request header in bytes.
        /// </summary>
        public const int RequestHeaderSize = 4;

        /// <summary>
        /// A an IPEndPoint object, targeting the application instance on this machine.
        /// </summary>
        public static IPEndPoint Loopback => new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), Port);
    }
}
