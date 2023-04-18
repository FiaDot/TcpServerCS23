using System.Net;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;

namespace ServerCore;


class Program
{
    static Listener _listener = new Listener();

    static void Main(string[] args)
    {
        // PacketManager.Instance.Register();


        // // serialize 
        // S_Ping encode = new S_Ping()
        // {
        //     Time = 1234,
        // };
        //
        // int size = encode.CalculateSize();
        // byte[] sendBuffer = encode.ToByteArray();
        //
        // // MsgId.SPing
        //
        // // deserialize
        //
        // S_Ping decode = new S_Ping();
        // decode.MergeFrom(sendBuffer);
        //
        //
        // Console.WriteLine($"time={decode.Time}");
        
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

