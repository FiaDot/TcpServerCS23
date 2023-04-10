using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting Server...");


        int port = 7777;
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, port);

        Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            listenSocket.Bind(endPoint);

            int backlog = 10;
            listenSocket.Listen(backlog);


            while (true)
            {
                Console.WriteLine($"Listening...{port}");

                Socket clientSocket = listenSocket.Accept();

                // recv
                byte[] recvBuff = new byte[1024];
                int recvBytes = clientSocket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);

                Console.WriteLine($"> ${recvData}");

                // TODO : send
                byte[] sendBuff = Encoding.UTF8.GetBytes("From Server");
                clientSocket.Send(sendBuff);

                // disconnect
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        } catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}

