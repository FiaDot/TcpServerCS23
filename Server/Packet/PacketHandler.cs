using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using Server;
using Server.Game;

public class PacketHandler
{
	public static void C_ChatHandler(PacketSession session, IMessage packet)
	{
		C_Chat recvPacket = packet as C_Chat;
		ClientSession clientSession = session as ClientSession;

		Console.WriteLine($"> C_ChatHandler {recvPacket.Context}");
	}
	
	// public static void C_MoveHandler(PacketSession session, IMessage packet)
	// {
	// 	C_Move recvPacket = packet as C_Move;
	// 	ClientSession clientSession = session as ClientSession;
	//
	// 	Console.WriteLine($"> C_MoveHandler : {recvPacket.PosInfo.PosX},{recvPacket.PosInfo.PosY}");
	// 	
	// 	// NOTE : MT에서 취약 지점 
	// 	if (clientSession.MyPlayer == null)
	// 		return;
	// 	if (clientSession.MyPlayer.Room == null)
	// 		return;
	// 	
	// 	// 서버에서 먼저 좌표이동
	// 	PlayerInfo info = clientSession.MyPlayer.Info;
	// 	info.PosInfo = recvPacket.PosInfo;
	//
	// 	// TODO : broadcasting room
	// 	S_Move move = new S_Move();
	// 	move.PlayerId = clientSession.MyPlayer.Info.PlayerId;
	// 	move.PosInfo = recvPacket.PosInfo;
	//
	// 	clientSession.MyPlayer.Room.Broadcast(move);
	// }
	
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		Console.WriteLine($"> C_Move ({movePacket.PosInfo.Pos.X},{movePacket.PosInfo.Pos.X},{movePacket.PosInfo.Pos.Z})");

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;

		// room.HandleMove(player, movePacket);
		room.Push(room.HandleMove, player, movePacket);
	}

	public static void C_SkillHandler(PacketSession session, IMessage packet)
	{
		C_Skill skillPacket = packet as C_Skill;
		ClientSession clientSession = session as ClientSession;

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;

		// room.HandleSkill(player, skillPacket);
		room.Push(room.HandleSkill, player, skillPacket);
	}

	public static void C_PongHandler(PacketSession session, IMessage packet)
	{
		Console.WriteLine($"> C_Pong");
		
		C_Pong recv = packet as C_Pong;
		ClientSession clientSession = session as ClientSession;
		clientSession.HandlePong(recv);
	}
	
	public static void C_RttHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = session as ClientSession;
		clientSession.Send(packet);
	}
}


