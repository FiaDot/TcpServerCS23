using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace Client;

class Program
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"| OnConnected : ${endPoint}");

            // send
            for(int i=0;i<5;i++)
            {
                byte[] sendBuff = Encoding.UTF8.GetBytes($"To Server : Hello {i}");
                Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"| OnDisconnected : ${endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"> {recvData}");

            // Disconnect();
            //socket.Shutdown(SocketShutdown.Both);
            //socket.Close();

            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"< Send Transffered {numOfBytes} bytes ");
        }
    }

    static void Main(string[] args)
    {
        Console.WriteLine("Starting Client...");

        int port = 7777;
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, port);

        Connector connector = new Connector();
        connector.Connect(endPoint, () => { return new GameSession(); });

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

