using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;

namespace Server
{
	public class PacketHandler
	{
		public static void SPingHandler(PacketSession session, IMessage packet)
		{
			S_Ping decode = packet as S_Ping;
			Console.WriteLine($"SPing: {decode.Time}");
		}

		
	}
}

