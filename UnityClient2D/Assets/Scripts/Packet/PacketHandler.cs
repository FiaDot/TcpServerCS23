using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PacketHandler
{
	public static void S_ChatHandler(PacketSession session, IMessage packet)
	{
		S_Chat chatPacket = packet as S_Chat;
		ServerSession serverSession = session as ServerSession;

		Debug.Log($"> S_ChatHandler : {chatPacket.Context}");

		C_Chat encode = new C_Chat();
        encode.Context = "test";
        serverSession.Send(encode);
        Debug.Log($"< C_Chat : {encode.Context}");
	}

	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame recvPacket = packet as S_EnterGame;
		ServerSession serverSession = session as ServerSession;
		
		Debug.Log($"> S_EnterGameHandler : {recvPacket.Player}");
		Managers.Object.Add(recvPacket.Player, myPlayer: true);
	}
	
	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame recvPacket = packet as S_LeaveGame;
		ServerSession serverSession = session as ServerSession;
		
		Debug.Log($"> S_LeaveGameHandler");
		Managers.Object.RemoveMyPlayer();
	}
	
	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn recvPacket = packet as S_Spawn;
		ServerSession serverSession = session as ServerSession;
		
		Debug.Log($"> S_SpawnHandler : {recvPacket.Players.Count}");
		foreach (PlayerInfo player in recvPacket.Players)
		{
			Managers.Object.Add(player, myPlayer: false);
		}
	}
	
	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn recvPacket = packet as S_Despawn;
		ServerSession serverSession = session as ServerSession;
		
		Debug.Log($"> S_DespawnHandler : {recvPacket.PlayerIds.Count}");
		foreach (int id in recvPacket.PlayerIds)
		{
			Debug.Log($"| S_DespawnHandler pid: {id}");
			Managers.Object.Remove(id);
		}
	}
	
	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move recvPacket = packet as S_Move;
		ServerSession serverSession = session as ServerSession;
		
		Debug.Log($"> S_MoveHandler : {recvPacket.PlayerId}");

		GameObject go = Managers.Object.FindById(recvPacket.PlayerId);
		if (null == go)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if (null == cc)
			return;
		
		cc.PosInfo = recvPacket.PosInfo;
	}
	
	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill skillPacket = packet as S_Skill;

		GameObject go = Managers.Object.FindById(skillPacket.PlayerId);
		if (go == null)
			return;

		PlayerController pc = go.GetComponent<PlayerController>();
		if (pc != null)
		{
			pc.UseSkill(skillPacket.Info.SkillId);
		}
	}
}
