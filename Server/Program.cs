using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore;

class Knight
{
    public int hp;
    public int attack;
}

class Packet
{
    public ushort size;
    public ushort packetId;
}

class GameSession : PacketSession
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"| OnConnected : ${endPoint}");

        // send
        //byte[] sendBuff = Encoding.UTF8.GetBytes("To Client : hello");
        //Send(sendBuff);

        
            //Packet packet = new Packet() { size = 4, packetId = 10 };
            //byte[] buffer = BitConverter.GetBytes(packet.size);
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);

            //// serialize
            ///*
            //byte[] sendBuff = new byte[1024];
            //// src, srcIdx, dst, dstIdx, len
            //Array.Copy(buffer, 0, sendBuff, 0, buffer.Length);
            //Array.Copy(buffer2, 0, sendBuff, buffer.Length, buffer2.Length);
            //Send(sendBuff);
            //*/

            //// use SendBuffer
            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            //// byte[] sendBuff = new byte[1024];
            //// src, srcIdx, dst, dstIdx, len
            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);       
            //Send(sendBuff);

        // 1초 대기 후 접속 끊기
        Thread.Sleep(5000);
        Disconnect();
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"| OnDisconnected : ${endPoint}");
    }

    //public override int OnRecv(ArraySegment<byte> buffer)
    //{
    //    string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
    //    Console.WriteLine($"> {recvData}");

    //    return buffer.Count;
    //}

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2); // +2 (size field)

        Console.WriteLine($"PacketId={packetId},Size={size}");
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

