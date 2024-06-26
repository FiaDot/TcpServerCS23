﻿using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class Player
	{
		public PlayerInfo Info { get; set; } = new PlayerInfo()
		{
			NetMoveInfo = new NetMove()
		};
		
		public GameRoom Room { get; set; }
		public ClientSession Session { get; set; }

	}
}
