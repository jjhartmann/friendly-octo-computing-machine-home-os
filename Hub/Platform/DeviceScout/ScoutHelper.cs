using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;

namespace HomeOS.Hub.Platform.DeviceScout
{
    public class ScoutHelper
    {
        public const int DefaultDeviceDiscoveryPeriodSec = 30;
        public const int DefaultNumPeriodsToForgetDevice = 5;

        public static bool BroadcastRequest(byte[] request, int portNumber, VLogger logger)
        {
            try
            {
                foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (netInterface.OperationalStatus != OperationalStatus.Up)
                        continue;

                    foreach (var netAddress in netInterface.GetIPProperties().UnicastAddresses)
                    {
                        //only send to IPv4 and non-loopback addresses
                        if (netAddress.Address.AddressFamily != AddressFamily.InterNetwork ||
                            IPAddress.IsLoopback(netAddress.Address))
                            continue;

                        IPEndPoint localEp = new IPEndPoint(netAddress.Address, portNumber);
                        using (var client = new UdpClient(localEp))
                        {
                            //logger.Log("Sending bcast packet from {0}", localEp.ToString());
                            client.Client.EnableBroadcast = true;
                            var endPoint = new IPEndPoint(IPAddress.Broadcast, portNumber);
                            client.Connect(endPoint);
                            client.Send(request, request.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log("Exception while sending UDP request. \n {0}", e.ToString());
                return false;
            }

            return true;
        }

        [System.Runtime.InteropServices.DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref int PhyAddrLen);

        public static string GetMacAddressByIP(System.Net.IPAddress ipAddress)
        {
            byte[] macBytes = new byte[6];
            int length = 6;
            SendARP(BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0), 0, macBytes, ref length);
            return BitConverter.ToString(macBytes, 0, 6);
        }

        public static NetworkInterface GetInterface(IPPacketInformation packetInfo)
        {

            int interfaceId = packetInfo.Interface;

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var netInterface in interfaces)
            {
                var ipv4InterfaceProps = netInterface.GetIPProperties().GetIPv4Properties();

                if (ipv4InterfaceProps != null &&
                    ipv4InterfaceProps.Index == interfaceId)
                    return netInterface;
            }

            return null;
        }

        public static bool IsMyAddress(IPAddress iPAddress)
        {
            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (var netAddress in netInterface.GetIPProperties().UnicastAddresses)
                {
                    if (netAddress.Address.Equals(iPAddress))
                        return true;
                }
            }

            return false;
        }
    }
}
