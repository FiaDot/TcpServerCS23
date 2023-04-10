using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
	public class Listener
    {
        Socket _listenSocket;

        public void Init(IPEndPoint endPoint)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _listenSocket.Bind(endPoint);

            int backlog = 10;
            _listenSocket.Listen(backlog);
        }

        public Socket Accept()
        {
            return _listenSocket.Accept();
        }

    }
}

