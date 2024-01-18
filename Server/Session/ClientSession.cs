using System;
using System.Diagnostics;
using ServerCore;
using System.Net;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;
using Google.Protobuf.Collections;
using System.Numerics;

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

        private readonly int pingPeriod = 1000;

        private int rtt = 0;

        // send queue for contents
        object _lock = new object();
        List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();

        // 클라이언트 Input 저장 
        // List<Inputs> inputs = new List<Inputs>();

        public void SimulateInputs(C_ActionInput actionInput)
        {
            Queue<Inputs> inputs = new Queue<Inputs>();
            foreach (Inputs input in actionInput.InputList)
            {
                inputs.Enqueue(input);
            }
            // a.DeliveryTime;
            // a.Inputs;
            // a.StartTickNumber;

            MovePlayer(inputs, actionInput.StartTickNumber);
        }

        uint server_tick_number = 0;
        uint server_tick_accumulator = 0;
        uint server_snapshot_rate = 10;
        float moveSpeed = 5f / (float)server_snapshot_rate;

        // location, rotation만 사용
        StateMessage rigidbody = new StateMessage();
        void MovePlayer(Queue<Inputs> inputs, uint startTickNumber)
        {
            // TODO : something...
            while (inputs.Count > 0)
            {
                // 쌓여있는거 하나씩 빼줌
                Inputs input_msg = inputs.Dequeue();

                // 마지막 상태만 받아와
                uint max_tick = startTickNumber + (uint)inputs.Count - 1;

                // 서버 틱 보다 크다면 새로운 메시지 -> 서버 틱 이전 값은 스킵   
                if (max_tick < server_tick_number)
                    continue;

                // 어디부터 시작해야 하나?
                uint start_i = server_tick_number > startTickNumber
                    ? (server_tick_number - startTickNumber)
                    : 0;

                // 모든 입력에 대해 물리 연산
                for (int i = (int)start_i; i < inputs.Count; i++)
                {
                    // PrePhysicsStep(input_msg.inputs[i]);
                    // server_physics_scene.Simulate(dt);

                    ++server_tick_number;
                    ++server_tick_accumulator;

                    // 서버 스냅샷 주기 (서버 처리 주기)
                    if (server_tick_accumulator < server_snapshot_rate)
                        continue;

                    // 서버 처리 주기에 도달했으니
                    server_tick_accumulator = 0;

                    // 서버에서 움직인 결과를 클라이언트에 전송
                    StateMessage state_msg = new StateMessage();
                    // state_msg.DeliveryTime = Time.time + this.latency;
                    state_msg.TickNumber = server_tick_number;
                    state_msg.Position = rigidbody.Position;
                    // state_msg.Rotation = server_rigidbody.rotation;
                    // state_msg.Velocity = server_rigidbody.velocity;
                    // state_msg.AngularVelocity = server_rigidbody.angularVelocity;
                    // client_state_msgs.Enqueue(state_msg);
                }

            }
        }

        void SimulateStep(Inputs inputs)
        {
            if (inputs.Up)
            {
                rigidbody.Position.Z += 1 * moveSpeed;
                //     rigidbody.AddForce(this.local_player_camera_transform.forward * this.player_movement_impulse, ForceMode.Impulse);
            }
            if (inputs.Down)
            {
                rigidbody.Position.Z -= 1 * moveSpeed;
                //     rigidbody.AddForce(-this.local_player_camera_transform.forward * this.player_movement_impulse, ForceMode.Impulse);
            }
            if (inputs.Left)
            {
                rigidbody.Position.X -= 1 * moveSpeed;
                //     rigidbody.AddForce(-this.local_player_camera_transform.right * this.player_movement_impulse, ForceMode.Impulse);
            }
            if (inputs.Right)
            {
                rigidbody.Position.Y -= 1 * moveSpeed;
                //     rigidbody.AddForce(this.local_player_camera_transform.right * this.player_movement_impulse, ForceMode.Impulse);
            }

            // if (rigidbody.transform.position.y <= this.player_jump_y_threshold && inputs.jump)
            // {
            //     rigidbody.AddForce(this.local_player_camera_transform.up * this.player_movement_impulse, ForceMode.Impulse);
            // }
        }

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

            // Console.WriteLine($"< {msgId} ");

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

                // MyPlayer.Info.PosInfo.State = CreatureState.Idle;
                // MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;

                MyPlayer.Info.NetMoveInfo.Pos = new vector3Net();
                MyPlayer.Info.NetMoveInfo.Pos.X = 0.0f;
                MyPlayer.Info.NetMoveInfo.Pos.Y = 0.0f;
                MyPlayer.Info.NetMoveInfo.Pos.Z = 0.0f;

                MyPlayer.Info.NetMoveInfo.Dir = new vector3Net();
                MyPlayer.Info.NetMoveInfo.Dir.X = 0.0f;
                MyPlayer.Info.NetMoveInfo.Dir.Y = 0.0f;
                MyPlayer.Info.NetMoveInfo.Dir.Z = 0.0f;

                MyPlayer.Info.NetMoveInfo.Acc = new vector3Net();
                MyPlayer.Info.NetMoveInfo.Acc.X = 0.0f;
                MyPlayer.Info.NetMoveInfo.Acc.Y = 0.0f;
                MyPlayer.Info.NetMoveInfo.Acc.Z = 0.0f;

                MyPlayer.Info.NetMoveInfo.Rot = new vector3Net();
                MyPlayer.Info.NetMoveInfo.Rot.X = 0.0f;
                MyPlayer.Info.NetMoveInfo.Rot.Y = 0.0f;
                MyPlayer.Info.NetMoveInfo.Rot.Z = 0.0f;

                MyPlayer.Info.NetMoveInfo.RotAcc = new vector3Net();
                MyPlayer.Info.NetMoveInfo.RotAcc.X = 0.0f;
                MyPlayer.Info.NetMoveInfo.RotAcc.Y = 0.0f;
                MyPlayer.Info.NetMoveInfo.RotAcc.Z = 0.0f;

                MyPlayer.Info.NetMoveInfo.Speed = 0.0f;
                MyPlayer.Info.NetMoveInfo.ServerTime = 0;
                MyPlayer.Info.NetMoveInfo.ClientTime = 0;
                MyPlayer.Info.NetMoveInfo.Flag = 0;

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
            pkt.Time = System.Environment.TickCount;
            Send(pkt);

            GameLogic.Instance.PushAfter(pingPeriod, Ping);
        }

        public void HandlePong(C_Pong pong)
        {
            _pingPongTick = System.Environment.TickCount64;
            rtt = System.Environment.TickCount - pong.Time;
            Console.WriteLine($"| HandlePong RTT={rtt}");
        }

    }

}

