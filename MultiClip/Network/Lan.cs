using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MultiClip.Network
{
    public static class Lan
    {
        /// <summary>
        /// Creates a list of all the network devices responding to a ping request
        /// connected to the same local area networks as this PC.
        /// </summary>
        /// <returns>An array of the IPAddresses of the devices.</returns>
        public static async Task<IPAddress[]> FindNetworkDevicesAsync()
        {
            IEnumerable<IPAddress> possibleHosts = GetAllPossibleHosts()
                .Where(ipAddress => ipAddress.ToString() != "0.0.0.0");

            // Ping every possible host machine
            // and return it's IP Address if the machine responds.
            // The result is null otherwise.
            IEnumerable<Task<IPAddress>> pingTasks = possibleHosts.Select(async ipAddress =>
            {
                Ping ping = new Ping();
                PingReply reply = null;
                try
                {
                    reply = await ping.SendPingAsync(ipAddress);
                }
                catch (Exception e) when (e is SocketException || e is PingException)
                {
                    return null;
                }

                if (reply.Status != IPStatus.Success)
                {
                    return null;
                }

                return ipAddress;
            });

            return (await Task.WhenAll(pingTasks))
                // Ignore the devices which have not responded.
                .Where(ipAddress => ipAddress != null)
                .ToArray();
        }

        /// <summary>
        /// Creates a list of all the possible hosts on the currently active networks
        /// the computer is connected to. Notice: These IP addresses are always valid,
        /// but may not be taken by a network device.
        /// </summary>
        private static IEnumerable<IPAddress> GetAllPossibleHosts()
        {
            // Get a list of all valid network interfaces.
            IEnumerable<NetworkInterface> netInterfaces = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(IsValidInterface);

            // A list of all possible network device addresses on the connected
            // network interfaces.
            LinkedList<IPAddress> ipAddresses = new LinkedList<IPAddress>();

            foreach (var netInterface in netInterfaces)
            {
                IPInterfaceProperties ipInfo = netInterface.GetIPProperties();
                UnicastIPAddressInformationCollection unicastAddresses = ipInfo.UnicastAddresses;

                foreach (var unicastAddress in unicastAddresses)
                {
                    IPAddress localAddress = unicastAddress.Address;
                    IPAddress subnetMask = unicastAddress.IPv4Mask;
                    IEnumerable<IPAddress> gateways = ipInfo.GatewayAddresses
                        .Select(x => x.Address.MapToIPv4())
                        .Where(IsValidGateway);
                    foreach (var gateway in gateways)
                    {
                        if (gateway.AddressFamily == AddressFamily.InterNetwork)
                        {
                            byte[] subnetBytes = subnetMask.GetAddressBytes();
                            if (subnetBytes[0] == 0 || subnetBytes[1] == 0)
                            {
                                continue;
                            }

                            IEnumerable<IPAddress> addresses = GetValidAddresses(gateway, subnetMask)
                                .Where(address => address != localAddress
                                    && !gateways.Contains(address));

                            foreach (var address in addresses)
                            {
                                ipAddresses.AddLast(address);
                            }
                        }
                    }
                }
            }

            return ipAddresses;
        }

        private static bool IsValidGateway(IPAddress ipAddress)
        {
            // The gateway address is invalid (reserved) when
            // the first 3 bytes of the address are 0.
            byte[] bytes = ipAddress.GetAddressBytes();
            return !bytes.Take(3).SequenceEqual(new byte[] { 0, 0, 0 });
        }

        /// <summary>
        /// Checks if the interface is valid (is a WLAN or Ethernet and is neither virtual nor loopback).
        /// </summary>
        private static bool IsValidInterface(NetworkInterface ni)
        {
            var addr = ni.GetIPProperties().GatewayAddresses.FirstOrDefault();
            return
                addr != null
                && !addr.Address.ToString().Equals("0.0.0.0")
                && ni.OperationalStatus == OperationalStatus.Up
                && (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                    || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet);
        }

        /// <summary>
        /// Creates a list of the valid IP addresses with the specified
        /// gateway and subnet mask.
        /// </summary>
        private static IEnumerable<IPAddress> GetValidAddresses(IPAddress gateway, IPAddress subnetMask)
        {
            byte[] ipBytes = gateway.GetAddressBytes();
            byte[] maskBytes = subnetMask.GetAddressBytes();

            byte[] startIPBytes = new byte[ipBytes.Length];
            byte[] endIPBytes = new byte[ipBytes.Length];

            // Calculate the bytes of the start and end IP addresses.
            for (int i = 0; i < ipBytes.Length; i++)
            {
                startIPBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                endIPBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
            }

            // Convert the bytes to IP addresses.
            IPAddress startIP = new IPAddress(startIPBytes);
            IPAddress endIP = new IPAddress(endIPBytes);

            return GetAddressesBetween(startIP, endIP);
        }

        /// <summary>
        /// Creates a list of the valid IP addresses between the given addresses.
        /// </summary>
        private static IEnumerable<IPAddress> GetAddressesBetween(IPAddress startIP, IPAddress endIP)
        {
            byte[] startIPBytes = startIP.GetAddressBytes();
            byte[] endIPBytes = endIP.GetAddressBytes();

            int targetByteIndex = -1;

            for (int i = 0; i < startIPBytes.Length; i++)
            {
                if (startIPBytes[i] != endIPBytes[i])
                {
                    targetByteIndex = i;
                    break;
                }
            }

            if (targetByteIndex == -1)
            {
                return new IPAddress[] { };
            }

            byte minValue = startIPBytes[targetByteIndex];
            byte maxValue = endIPBytes[targetByteIndex];
            byte[] newIPBytes = new byte[startIPBytes.Length];

            List<IPAddress> results = new List<IPAddress>();

            for (int value = minValue; value <= maxValue; value++)
            {
                startIPBytes.CopyTo(newIPBytes, 0);
                newIPBytes[targetByteIndex] = (byte)value;
                results.Add(new IPAddress(newIPBytes));
            }

            newIPBytes[targetByteIndex] = Byte.MaxValue;
            return results.Concat(GetAddressesBetween(new IPAddress(newIPBytes), endIP));
        }

    }
}
