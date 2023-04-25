using System.Net;
using System.Net.Sockets;

namespace ServerCore.Util;

public class NetworkInterfaceHelper
{
    public static IPAddress GetPublicIp()
    {
        string loopback = "127.0.0.1";
        IPAddress addr = IPAddress.Parse(loopback);
        
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress[] addrs = ipHost.AddressList;
        
        
        // IPv6, IPv4 모두 나옴 ㅡㅡ;
        /*
            ::1
            fe80::1%1
            127.0.0.1
            10.43.100.178
            218.38.137.28
            fe80::8c4a:5eff:fed2:f959%6
            ...
         */
        
        
        foreach (IPAddress ipAddress in addrs)
        {
            // filter out ipv4
            if (ipAddress.AddressFamily != AddressFamily.InterNetwork) 
                continue;

            if (!ipAddress.Equals(addr))
            {
                addr = ipAddress;
                break;
            }
        }

        return addr;
    }
}