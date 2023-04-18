using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
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
        // protobuf Send wrapper
        public void Send(IMessage message)
        {
            string messageName = message.Descriptor.Name.Replace("_", string.Empty);
            // TODO : try catch
            MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), messageName);

            int size = message.CalculateSize();

            byte[] sendBuffer = new byte[size + 4];
            // write size
            Array.Copy(BitConverter.GetBytes(size + 4), 0, sendBuffer, 0, sizeof(ushort));
            // write id
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            // write encoded
            Array.Copy(message.ToByteArray(), 0, sendBuffer, 4, size);

            Send(new ArraySegment<byte>(sendBuffer));
        }


        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"| OnConnected : ${endPoint}");

            //Packet packet = new Packet() { size = 4, packetId = 12 };

            //// send 
            //for (int i = 0; i < 5; i++)
            //{
            //    byte[] buffer = BitConverter.GetBytes(packet.size);
            //    byte[] buffer2 = BitConverter.GetBytes(packet.packetId);

            //    // use SendBuffer
            //    ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

            //    Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            //    Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            //    ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size);
            //    Send(sendBuff);
            //}

            // S_Ping encode = new S_Ping()
            // {
            //     Time = 1234,
            // };


            //int size = encode.CalculateSize();
            //// byte[] sendBuffer = encode.ToByteArray();
            //byte[] sendBuffer = new byte[size + 4];
            //// write size
            //Array.Copy(BitConverter.GetBytes(size + 4), 0, sendBuffer, 0, sizeof(ushort));
            //// write id
            //ushort id = (ushort)MsgId.SPing;
            //Array.Copy(BitConverter.GetBytes(id), 0, sendBuffer, 2, sizeof(ushort));
            //// write encoded
            //Array.Copy(encode.ToByteArray(), 0, sendBuffer, 4, size);

            //Send(new ArraySegment<byte>(sendBuffer));

            C_Chat encode = new C_Chat();
            encode.Context = "test";
            Send(encode);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"| OnDisconnected : {endPoint}");
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

