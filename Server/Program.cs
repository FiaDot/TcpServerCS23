using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;
class Program
{
    static Listener _listener = new Listener();

    static void OnAcceptHandler(Socket clientSocket)
    {
        Console.WriteLine($"| OnAcceptHandler");
        try
        {            
            Session session = new Session();
            session.Start(clientSocket);

            // send
            byte[] sendBuff = Encoding.UTF8.GetBytes("To Client : hello");
            session.Send(sendBuff);

            // 0.1초 대기 후 접속 끊기
            Thread.Sleep(100);
            session.Disconnect();
        }
        catch (Exception e)
        { 
            Console.WriteLine(e);
        }
    }

    static void Main(string[] args)
    {
        Console.WriteLine("Starting Server...");


        int port = 7777;
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, port);

        _listener.Init(endPoint, OnAcceptHandler);

        Console.WriteLine($"Listening...{port}");

        while (true)
        {
            ;
        }      
    }
}

