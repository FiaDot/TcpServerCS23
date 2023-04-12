using System.Net;
using ServerCore;

namespace Client;

class Program
{

    static void Main(string[] args)
    {
        Console.WriteLine("Starting Client...");

        int port = 7777;
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, port);

        Connector connector = new Connector();
        connector.Connect(endPoint, () => { return new ServerSession(); });

        while(true)
        {
            try
            {

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Thread.Sleep(100);
        }

    }
}

