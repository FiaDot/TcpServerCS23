﻿using System.Net;
using System.Net.Sockets;
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

        // 로컬 아이피 받기
        List<IPAddress> localAddresses = new List<IPAddress>();
        foreach (IPAddress ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork) // filter out ipv4
            {
                localAddresses.Add(ipAddress);
                // Console.WriteLine(ipAddress.ToString());
            }
        }

        endPoint = new IPEndPoint(localAddresses[1], port);
        
        // IPv6, IPv4 모두 나옴 ㅡㅡ;
        /*
            ::1
            fe80::1%1
            127.0.0.1
            10.43.100.178
            218.38.137.28
            fe80::8c4a:5eff:fed2:f959%6
            ...
         */
        // foreach(IPAddress ip in ipHost.AddressList)
        // {
        //     Console.WriteLine(ip.ToString());
        // }
        
        
        _listener.Init(endPoint, () => { return new ClientSession(); });

        
        Console.WriteLine($"Listening... BIND {localAddresses[1]}:{port}");

        while (true)
        {
            ;
        }      
    }
}

