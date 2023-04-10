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
            // Socket clientSocket = _listener.Accept();

            // recv
            byte[] recvBuff = new byte[1024];
            int recvBytes = clientSocket.Receive(recvBuff);
            string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);

            Console.WriteLine($"> {recvData}");

            // send
            byte[] sendBuff = Encoding.UTF8.GetBytes("To Client : hello");
            int sendBytes = clientSocket.Send(sendBuff);
            Console.WriteLine($"< {sendBytes} bytes");
            // disconnect
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();

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

