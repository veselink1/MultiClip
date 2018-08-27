using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MultiClip.Network
{
    /// <summary>
    /// A local server instance, responsible for receiving messages.
    /// </summary>
    public sealed class Server
    {
        private TcpListener _server;
        private Thread _serverThread;
        private bool _isRunning;
        private bool _isDirty;

        public Server()
        {
            _server = null;
            _serverThread = null;
            _isRunning = false;
            _isDirty = false;
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        public void Start()
        {
            if (_isDirty)
            {
                throw new InvalidOperationException();
            }
            _isDirty = true;
            _isRunning = true;
            _server = new TcpListener(new IPAddress(0), NetConfig.Port);
            _serverThread = new Thread(ProcessingLoop);
            _serverThread.Start();
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _serverThread.Join();
            _serverThread = null;
            _server.Stop();
            _server = null;
            _isDirty = false;
        }
        
        /// <summary>
        /// Reads and processes a single request.
        /// </summary>
        private void Process(IPAddress remoteAddr, NetworkStream stream)
        {
            try
            {
                int reqSize = NetUtils.ReadInt32(stream);
                // Used by ping requests.
                if (reqSize == 0)
                {
                    NetUtils.Write(stream, new byte[] { 0, 0, 0, 0 });
                    return;
                }

                byte[] reqBytes = NetUtils.Read(stream, reqSize);

                IRequest req = Request.FromBytes(reqBytes);
                IResponse res = req.GetResponse(remoteAddr);

                byte[] resBytes = res.GetBytes();
                byte[] resHeaderBytes = NetUtils.GetBytes(resBytes.Length);

                NetUtils.Write(stream, resHeaderBytes);
                NetUtils.Write(stream, resBytes);
            }
            catch
            {
                // Ignore failed requests.
            }
        }

        /// <summary>
        /// Listens for messages in a loop.
        /// </summary>
        private void ProcessingLoop()
        {
            _server.Start();
            // Enter the listening loop.
            while (_isRunning)
            {
                try
                {
                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    using (TcpClient client = _server.AcceptTcpClient())
                    {
                        // Get a stream object for reading and writing.
                        using (NetworkStream stream = client.GetStream())
                        {
                            Process((client.Client.RemoteEndPoint as IPEndPoint)?.Address, stream);
                        }
                    }
                }
                catch
                {
                    // Ignore.
                }
            }
        }
    }
}
