using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;

namespace Server
{
	public class PacketHandler
	{
		public static void C_ChatHandler(PacketSession session, IMessage packet)
		{
			C_Chat chatPacket = packet as C_Chat;
			ClientSession serverSession = session as ClientSession;
			
			Console.WriteLine(chatPacket.Context);
		}
	}
}

