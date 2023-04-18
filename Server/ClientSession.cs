using System;
using ServerCore;
using System.Net;
using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server
{
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    class ClientSession : PacketSession
    {
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

            Send(new ArraySegment<byte>(sendBuffer));
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
            
            S_Chat chat = new S_Chat()
			{
				Context = "안녕하세요"
			};
			Send(chat);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
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
            Console.WriteLine($"< Send Transffered {numOfBytes} bytes ");
        }
    }


}

