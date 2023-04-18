using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;

namespace Server
{
	public class PacketHandler
	{
		// public static void S_PingHandler(PacketSession session, IMessage packet)
		// {
		// 	S_Ping decode = packet as S_Ping;
		// 	// ClientSession serverSession = session as ClientSession;
		// 	
		// 	Console.WriteLine($"SPing: Time={decode.Time}");
		// }		
		
		public static void C_ChatHandler(PacketSession session, IMessage packet)
		{
			S_Chat chatPacket = packet as S_Chat;
			ClientSession serverSession = session as ClientSession;
			
			Console.WriteLine(chatPacket.Context);
		}
	}
}

