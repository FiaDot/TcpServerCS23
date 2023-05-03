using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;

namespace Server.Game
{
	public class GameRoom : JobSerializer
	{
		public int RoomId { get; set; }
		List<Player> _players = new List<Player>();

		// 누군가 주기적으로 호출해줘야 한다 call by GameLogic
		public void Update()
		{
			Flush();
		}

		public void Init(int mapId)
		{
			// _map.LoadMap(mapId);
		}

		
		public void EnterGameHandler()
		{
				
		}
		
		public void EnterGame(Player newPlayer)
		{
			if (newPlayer == null)
				return;

			_players.Add(newPlayer);
			newPlayer.Room = this;

			// 본인한테 정보 전송
			{
				S_EnterGame enterPacket = new S_EnterGame();
				enterPacket.Player = newPlayer.Info;
				newPlayer.Session.Send(enterPacket);

				S_Spawn spawnPacket = new S_Spawn();
				foreach (Player p in _players)
				{
					if (newPlayer != p)
						spawnPacket.Players.Add(p.Info);
				}

				newPlayer.Session.Send(spawnPacket);
			}

			// 타인한테 정보 전송
			{
				S_Spawn spawnPacket = new S_Spawn();
				spawnPacket.Players.Add(newPlayer.Info);
				foreach (Player p in _players)
				{
					if (newPlayer != p)
						p.Session.Send(spawnPacket);
				}
			}
		}
		
		public void LeaveGame(int playerId)
		{
			Player player = _players.Find(p => p.Info.PlayerId == playerId);
			if (player == null)
				return;

			_players.Remove(player);
			player.Room = null;

			// 본인한테 정보 전송
			{
				S_LeaveGame leavePacket = new S_LeaveGame();
				player.Session.Send(leavePacket);
			}

			// 타인한테 정보 전송
			{
				S_Despawn despawnPacket = new S_Despawn();
				despawnPacket.PlayerIds.Add(player.Info.PlayerId);
				foreach (Player p in _players)
				{
					if (player != p)
						p.Session.Send(despawnPacket);
				}
			}

		}

		
		public void HandleMove(Player player, C_Move movePacket)
		{
			if (player == null)
				return;

			NetMove netMoveInfo = movePacket.NetMoveInfo;
			PlayerInfo info = player.Info;
			
			// 일단 서버에서 좌표 이동
			info.NetMoveInfo = netMoveInfo;
			
			// 다른 플레이어한테도 알려준다
			S_Move resMovePacket = new S_Move();
			resMovePacket.PlayerId = player.Info.PlayerId;
			resMovePacket.NetMoveInfo = movePacket.NetMoveInfo;

			Broadcast(resMovePacket);
		}

		
		public void HandleSkill(Player player, C_Skill skillPacket)
		{
			// if (player == null)
			// 	return;
			//
			// PlayerInfo info = player.Info;
			// if (info.PosInfo.State != CreatureState.Idle)
			// 	return;
			//
			// // TODO : 스킬 사용 가능 여부 체크
			//
			// // 통과
			// info.PosInfo.State = CreatureState.Skill;
			//
			// S_Skill skill = new S_Skill() { Info = new SkillInfo() };
			// skill.PlayerId = info.PlayerId;
			// skill.Info.SkillId = 1;
			// Broadcast(skill);
			//
			// // TODO : 데미지 판정
			// Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
			// Player target = _map.Find(skillPos);
			// if (target != null)
			// {
			// 	Console.WriteLine("Hit Player !");
			// }
		}
		
		public void Broadcast(IMessage packet)
		{
			foreach (Player p in _players)
			{
				p.Session.Send(packet);
			}
		}
		
	}
}
