using System;
using System.Net;
using System.Net.Sockets;
using static System.Collections.Specialized.BitVector32;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;

        // Action<Socket> _onAcceptHandler;
        Func<Session> _sessionFactory;


        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory = sessionFactory;

            _listenSocket.Bind(endPoint);

            int backlog = 10;
            _listenSocket.Listen(backlog);

            // 여러개 대기를 걸어주면 초기접속 빠르게 가능!
            // for(int n=0;n<10;n ++)
            // {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);

            RegisterAccept(args);
            // } 
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 재사용을 위해 초기화 필요
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)
            {
                // 대기 없이 바로 접속됨
                OnAcceptCompleted(null, args);
            }

            // 추후 통보됨
        }

        void OnAcceptCompleted(object? sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                // GameSession은 Session을 상속받아 사용.
                // 그래서 생성할 클래스를 factory로 변경
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);

                // 접속시 클라이언트 끊기면 에러 발생함 !!!!!
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            // 재등록
            RegisterAccept(args);
        }

        //public Socket Accept()
        //{
        //    // _listenSocket.AcceptAsync();
        //    return _listenSocket.Accept();
        //}

    }
}

