using System.Net;
using Server;

namespace ServerCore;


class Program
{
    static Listener _listener = new Listener();

    static void Main(string[] args)
    {
        Console.WriteLine("Starting Server...");


        int port = 7777;
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, port);

        _listener.Init(endPoint, () => { return new ClientSession(); });

        Console.WriteLine($"Listening...{port}");

        while (true)
        {
            ;
        }      
    }
}

