using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MultiClip.Network.Messages;
using System.Diagnostics;
using MultiClip.Utilities;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace MultiClip.Network
{
    /// <summary>
    /// Manages the discovery and network instances of the application.
    /// </summary>
    public sealed class LanScanner
    {
        private bool _backgroundScanningEnabled;
        private Thread _scanningThread;
        // A lock around the background scanning state.
        private readonly object _scanningStateLock;

        public Dispatcher Dispatcher { get; private set; }

        public ObservableCollection<Host> CurrentHosts { get; private set; }
        /// <summary>
        /// The interval between two background scans.
        /// </summary>
        public TimeSpan ScanInterval { get; private set; }

        /// <summary>
        /// Enables the background scanning for network instances.
        /// </summary>
        public bool BackgroundScanningEnabled
        {
            get => _backgroundScanningEnabled;
            set
            {
                SetBackgroundScanning(value);
            }
        }

        public LanScanner(TimeSpan scanInterval, bool enableBackgroundScanning)
        {
            _scanningStateLock = new object();
            _backgroundScanningEnabled = enableBackgroundScanning;
            Dispatcher = Dispatcher.CurrentDispatcher;
            CurrentHosts = new ObservableCollection<Host>();
            ScanInterval = scanInterval;

            if (enableBackgroundScanning)
            {
                EnableBackgroundScanning();
            }
        }

        /// <summary>
        /// Makes a single scan for active network instances.
        /// </summary>
        /// <returns>The list of network instances discovered.</returns>
        public async Task ScanNetworkAsync()
        {
            await UpdateHosts();
        }

        /// <summary>
        /// Sets the state of the background scanning thread.
        /// </summary>
        private void SetBackgroundScanning(bool value)
        {
            lock (_scanningStateLock)
            {
                if (value == _backgroundScanningEnabled)
                {
                    return;
                }

                if (value)
                {
                    EnableBackgroundScanning();
                }
                else
                {
                    DisableBackgroundScanning();
                }
                _backgroundScanningEnabled = value;
            }
        }

        /// <summary>
        /// Enables the background scanning thread.
        /// </summary>
        private void EnableBackgroundScanning()
        {
            _scanningThread = new Thread(ScanningLoop);
            _scanningThread.Start();
        }

        /// <summary>
        /// Disables the background scanning thread.
        /// </summary>
        private void DisableBackgroundScanning()
        {
            _scanningThread?.Join();
            _scanningThread = null;
        }

        /// <summary>
        /// The background scanning loop.
        /// </summary>
        private void ScanningLoop()
        {
            while (_backgroundScanningEnabled)
            {
                UpdateHosts().Wait();
                Thread.Sleep(ScanInterval);
            }
        }

        /// <summary>
        /// Iterates through a list of IP Addresses of network devices
        /// and determines which of them are running an instance of this application.
        /// </summary>
        /// <param name="networkDevices">The list of network devices</param>
        /// <returns>The list of application hosts</returns>
        private async Task<Host[]> FindAppHostsAsync(IEnumerable<IPAddress> networkDevices)
        {
            // A message requesting info is sent to every device
            // and it's response is transformed into a Host object;
            // if there is no response, the result is null.
            IEnumerable<Task<Host>> appHostTasks = networkDevices.Select(async ipAddress =>
            {
                try
                {
                    IPEndPoint endPoint = new IPEndPoint(ipAddress, NetConfig.Port);
                    PingResponse response = await Host.SendAsync(endPoint, new PingRequest());
#if DEBUG
                    return new Host(response.MachineGuid, endPoint, response.UserName, response.MachineName);
#else
                    if (response.MachineGuid != Host.Loopback.MachineGuid)
                    {
                        return new Host(response.MachineGuid, endPoint, response.UserName, response.MachineName);
                    }
                    else
                    {
                        return null;
                    }
#endif
                }
                catch (Exception e) when (e is SocketException || e is TimeoutException)
                {
                    // If an error occurs, the machine is not running
                    // an instance of this application.
                    return null;
                }
            });

            return (await Task.WhenAll(appHostTasks))
                // Ignore null values.
                .Where(host => host != null)
                .GroupBy(host => host.MachineGuid)
                .Select(x => x.First())
                .ToArray();
        }

        private async Task UpdateHosts()
        {
            Host[] activeHosts;

            try
            {
                // Get a list of all devices to which this machine has access.
                IPAddress[] networkDevices = await Lan.FindNetworkDevicesAsync();
                foreach (var ipAddress in networkDevices)
                {
                    Debug.WriteLine($"Ping reply from {ipAddress} was OK.");
                }

                // From the list of network devices, obtain the information
                // of these instances which are running this application.
                activeHosts = await FindAppHostsAsync(networkDevices);
                foreach (var host in activeHosts)
                {
                    Debug.WriteLine($"Detected app host {host.UserName}@{host.MachineName}/{host.EndPoint}");
                }
            }
            catch (Exception e)
            {
                Logger.Default.LogWarn(LogEvents.NetErr, e);
#if DEBUG
                e.Notify();
#endif
                return;
            }

            Dispatcher.Invoke(() =>
            {
                var missingHosts = CurrentHosts.Where(oldHost => !activeHosts
                    .Any(activeHost => activeHost.MachineGuid == oldHost.MachineGuid))
                    .ToList();
                foreach (var host in missingHosts)
                {
                    CurrentHosts.Remove(host);
                }
                var newHosts = activeHosts.Where(activeHost => !CurrentHosts
                    .Any(oldHost => oldHost.MachineGuid == activeHost.MachineGuid));
                foreach (var host in newHosts)
                {
                    CurrentHosts.Add(host);
                }
            });
        }
    }
}
