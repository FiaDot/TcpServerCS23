using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore;

class GameSession : Session
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"| OnConnected : ${endPoint}");

        // send
        byte[] sendBuff = Encoding.UTF8.GetBytes("To Client : hello");
        Send(sendBuff);

        // 1초 대기 후 접속 끊기
        Thread.Sleep(1000);
        Disconnect();
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"| OnDisconnected : ${endPoint}");
    }

    public override int OnRecv(ArraySegment<byte> buffer)
    {
        string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        Console.WriteLine($"> {recvData}");

        return buffer.Count;
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine($"< Send Transffered {numOfBytes} bytes ");
    }
}

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

        _listener.Init(endPoint, () => { return new GameSession(); });

        Console.WriteLine($"Listening...{port}");

        while (true)
        {
            ;
        }      
    }
}

