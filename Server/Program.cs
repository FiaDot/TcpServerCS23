using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game;
using ServerCore.Util;
using Timer = System.Timers.Timer;

namespace ServerCore;


class Program
{
    static Listener _listener = new Listener();

    static void GameLogicTask()
    {
	    while (true)
	    {
		    GameLogic.Instance.Update();
		    Thread.Sleep(0);
	    }
    }

            
    // send thread
    static void NetworkTask()
    {
	    while (true)
	    {
		    List<ClientSession> sessions = SessionManager.Instance.GetSessions();
		    foreach (ClientSession session in sessions)
		    {
			    session.FlushSend();
		    }

		    Thread.Sleep(0);
	    }
    }
            
    private static List<System.Timers.Timer> _timers = new List<Timer>();
    static void TickRoom(GameRoom room, int tick = 100)
    {
        var timer = new System.Timers.Timer();
        timer.Interval = tick;
        timer.Elapsed += ((o, e ) => { room.Update(); });
        timer.AutoReset = true;
        timer.Enabled = true;
        _timers.Add(timer);
    }
    
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
        
        
        
        
        GameRoom room = RoomManager.Instance.Add(1);
        TickRoom(room, 50);
        
        Console.WriteLine("Starting Server...");
        
        int port = 7777;
        IPEndPoint endPoint = new IPEndPoint(NetworkInterfaceHelper.GetPublicIp(), port);
        
        
        // 생성과 동시에 세션 메니져 추가
        // _listener.Init(endPoint, () => { return new ClientSession(); });
        
        _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
        
        Console.WriteLine($"Listening... BIND {endPoint.ToString()}");

        // NetworkTask
        {
	        Thread t = new Thread(NetworkTask);
	        t.Name = "Network Send";
	        t.Start();
        }
			
        // JobTimer.Instance.Push(FlushRoom);
        // JobTimer.Instance.Push(() => Console.WriteLine("250"), 250);
        // JobTimer.Instance.Push(() => Console.WriteLine("500"), 500);
        // while (true)
        // {
        //     // JobTimer.Instance.Flush();
        //     // GameRoom room = RoomManager.Instance.Find(1);
        //     // room.Push(room.Flush);
        //     Thread.Sleep(100);
        // }      
        
        // GameLogic
        Thread.CurrentThread.Name = "GameLogic";
        GameLogicTask();
    }
}

