using System.Net;
using System.Net.Sockets;
using System.Text;

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

        Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            socket.Connect(endPoint);
            Console.WriteLine($"Connected to {socket.RemoteEndPoint}");

            byte[] sendBuff = Encoding.UTF8.GetBytes("To Server : Hello");
            int sendBytes = socket.Send(sendBuff);
            Console.WriteLine($"< {sendBytes} bytes");

            byte[] recvBuff = new byte[1024];
            int recvBytes = socket.Receive(recvBuff);
            string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
            Console.WriteLine($"> {recvData}");

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        } catch (Exception e)
        {
            Console.WriteLine(e);
        }        
    }
}

