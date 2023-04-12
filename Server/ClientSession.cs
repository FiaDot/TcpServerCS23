using System;
using ServerCore;
using System.Net;

namespace Server
{
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    class ClientSession : PacketSession
    {
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
            Thread.Sleep(5000);
            Disconnect();
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
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2); // +2 (size field)

            Console.WriteLine($"PacketId={packetId},Size={size}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"< Send Transffered {numOfBytes} bytes ");
        }
    }


}

