using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace Client;

class Program
{
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"| OnConnected : ${endPoint}");

            Packet packet = new Packet() { size = 4, packetId = 12 };

            // send
            for (int i=0;i<5;i++)
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

    static void Main(string[] args)
    {
        Console.WriteLine("Starting Client...");

        int port = 7777;
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, port);

        Connector connector = new Connector();
        connector.Connect(endPoint, () => { return new GameSession(); });

        while(true)
        {
            try
            {

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Thread.Sleep(100);
        }

    }
}

