using System;
using System.Diagnostics;
using ServerCore;
using System.Net;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;

namespace Server
{
    // class Packet
    // {
    //     public ushort size;
    //     public ushort packetId;
    // }

    public class ClientSession : PacketSession
    {
        public Player MyPlayer { get; set; }
		public int SessionId { get; set; }

		private long _pingPongTick = 0;

		private readonly int pingPeriod = 5000;
		
		// send queue for contents
		object _lock = new object();
		List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();
		
        public void Send(IMessage message)
        {
            string messageName = message.Descriptor.Name.Replace("_", string.Empty);
            // TODO : try catch
            MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), messageName);

            int size = message.CalculateSize();
            
            byte[] sendBuffer = new byte[size + 4];
            // write size
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            // write id
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            // write encoded
            Array.Copy(message.ToByteArray(), 0, sendBuffer, 4, size);

            Console.WriteLine($"< {msgId} ");
            
            // 바로 안보내고 sendQueue에 넣음 (send thread 분리)
            // Send(new ArraySegment<byte>(sendBuffer));
            
            lock (_lock)
            {
	            _reserveQueue.Add(sendBuffer);
            }
        }
        
        // 실제 Network IO 보내는 부분
		public void FlushSend()
		{
			List<ArraySegment<byte>> sendList = null;

			lock (_lock)
			{
				if (_reserveQueue.Count == 0)
					return;

				sendList = _reserveQueue;
				_reserveQueue = new List<ArraySegment<byte>>();
			}

			Send(sendList);
		}

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"| OnConnected : ${endPoint}");

            // send
            //byte[] sendBuff = Encoding.UTF8.GetBytes("To Client : hello");
            //Send(sendBuff);


            //Packet packet = new Packet() { size = 4, packetId = 10 };
            //byte[] buffer = BitConverter.GetBytes(packet.size);
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);

            //// serialize
            ///*
            //byte[] sendBuff = new byte[1024];
            //// src, srcIdx, dst, dstIdx, len
            //Array.Copy(buffer, 0, sendBuff, 0, buffer.Length);
            //Array.Copy(buffer2, 0, sendBuff, buffer.Length, buffer2.Length);
            //Send(sendBuff);
            //*/

            //// use SendBuffer
            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            //// byte[] sendBuff = new byte[1024];
            //// src, srcIdx, dst, dstIdx, len
            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);       
            //Send(sendBuff);

            // 1초 대기 후 접속 끊기
            // Thread.Sleep(5000);
            // Disconnect();
            
            // S_Chat chat = new S_Chat()
			// {
			// 	Context = "안녕하세요"
			// };
			// Send(chat);
            
            MyPlayer = PlayerManager.Instance.Add();
			{
				MyPlayer.Info.Name = $"Player_{MyPlayer.Info.PlayerId}";
				MyPlayer.Info.PosInfo.State = CreatureState.Idle;
				MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;

				MyPlayer.Info.PosInfo.Pos = new vector3Net();
				MyPlayer.Info.PosInfo.Pos.X = 0.0f;
				MyPlayer.Info.PosInfo.Pos.Y = 0.0f;
				MyPlayer.Info.PosInfo.Pos.Z = 0.0f;
				
				MyPlayer.Info.PosInfo.Rot = new vector3Net();
				MyPlayer.Info.PosInfo.Rot.X = 0.0f;
				MyPlayer.Info.PosInfo.Rot.Y = 0.0f;
				MyPlayer.Info.PosInfo.Rot.Z = 0.0f;
				
				MyPlayer.Session = this;
			}

			RoomManager.Instance.Find(1).EnterGame(MyPlayer);
			
			GameLogic.Instance.PushAfter(pingPeriod, Ping);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
	        // RoomManager.Instance.Find(1).LeaveGame(MyPlayer.Info.PlayerId);
	        GameRoom room = RoomManager.Instance.Find(1);
	        room.Push(room.LeaveGame, MyPlayer.Info.PlayerId);
	        
            SessionManager.Instance.Remove(this);
            
            Console.WriteLine($"| OnDisconnected : ${endPoint}");
        }

        //public override int OnRecv(ArraySegment<byte> buffer)
        //{
        //    string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        //    Console.WriteLine($"> {recvData}");

        //    return buffer.Count;
        //}

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            //ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            //ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2); // +2 (size field)

            //Console.WriteLine($"PacketId={packetId},Size={size}");

            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            // Console.WriteLine($"< Send Transferred {numOfBytes} bytes ");
        }
        
        public void Ping()
        {
	        if (_pingPongTick > 0)
	        {
		        long delta = System.Environment.TickCount64 - _pingPongTick;
		        if (delta > 5 * 1000) // sec
		        {
			        Console.WriteLine("Disconnected by heartbeat");
			        Disconnect();
			        return;
		        }
	        }

	        S_Ping pkt = new S_Ping();
	        Send(pkt);
	        
	        GameLogic.Instance.PushAfter(pingPeriod, Ping);
        }
        
        public void HandlePong()
        {
	        _pingPongTick = System.Environment.TickCount64;
	        // Console.WriteLine($"| HandlePong tick={_pingPongTick}");
        }
    }

}

