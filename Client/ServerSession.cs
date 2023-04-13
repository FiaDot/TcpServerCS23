﻿using ServerCore;
using System.Net;
using System.Text;

namespace Client
{
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    public class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"| OnConnected : ${endPoint}");

            Packet packet = new Packet() { size = 4, packetId = 12 };

            // send 
            for (int i = 0; i < 5; i++)
            {
                byte[] buffer = BitConverter.GetBytes(packet.size);
                byte[] buffer2 = BitConverter.GetBytes(packet.packetId);

                // use SendBuffer
                ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

                Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
                Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
                ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size);
                Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"| OnDisconnected : ${endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"> {recvData}");

            // Disconnect();
            //socket.Shutdown(SocketShutdown.Both);
            //socket.Close();

            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"< Send Transffered {numOfBytes} bytes ");
        }
    }

}
