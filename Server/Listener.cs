using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
	public class Listener
    {
        Socket _listenSocket;

        Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler = onAcceptHandler;

            _listenSocket.Bind(endPoint);

            int backlog = 10;
            _listenSocket.Listen(backlog);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);

            RegisterAccept(args);
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 재사용을 위해 초기화 필요
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);
            if ( pending == false)
            {
                // 대기 없이 바로 접속됨
                OnAcceptCompleted(null, args);
            }

            // 추후 통보됨
        }

        void OnAcceptCompleted(object? sender, SocketAsyncEventArgs args)
        {
            if ( args.SocketError == SocketError.Success )
            {
                // 성공
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            // 재등록
            RegisterAccept(args);
        }

        public Socket Accept()
        {
            // _listenSocket.AcceptAsync();
            return _listenSocket.Accept();
        }

    }
}

