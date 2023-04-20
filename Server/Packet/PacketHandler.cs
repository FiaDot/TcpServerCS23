using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using Server;

public class PacketHandler
{
	public static void C_ChatHandler(PacketSession session, IMessage packet)
	{
		C_Chat recvPacket = packet as C_Chat;
		ClientSession clientSession = session as ClientSession;

		Console.WriteLine($"> C_ChatHandler {recvPacket.Context}");
	}
	
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move recvPacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		Console.WriteLine($"> C_MoveHandler : {recvPacket.PosInfo.PosX},{recvPacket.PosInfo.PosY}");
		
		
		if (clientSession.MyPlayer == null)
			return;
		if (clientSession.MyPlayer.Room == null)
			return;
		
		// 서버에서 먼저 좌표이동
		PlayerInfo info = clientSession.MyPlayer.Info;
		info.PosInfo = recvPacket.PosInfo;

		// TODO : broadcasting room
		S_Move move = new S_Move();
		move.PlayerId = clientSession.MyPlayer.Info.PlayerId;
		move.PosInfo = recvPacket.PosInfo;

		clientSession.MyPlayer.Room.Broadcast(move);
	}
}


