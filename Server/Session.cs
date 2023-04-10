using System;
using System.Net.Sockets;
using System.Text;

namespace Server
{
	public class Session
	{
		Socket _socket;
		int _disconnected = 0;

		public void Start(Socket socket)
		{
			_socket = socket;

			SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
			recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            // recvArgs.UserToken // 식별자로 customize 가능
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

			RegisterRecv(recvArgs);
        }

		public void Send(byte[] sendBuff)
		{
			_socket.Send(sendBuff);
		}

		public void Disconnect()
		{			
			// 이미 접속 해제 호출 됐었다면 패스
			if (Interlocked.Exchange(ref _disconnected, 1) == 1)
				return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신 
        void RegisterRecv(SocketAsyncEventArgs args)
        {
			bool pending = _socket.ReceiveAsync(args);
			if ( pending == false)
			{
				// 바로 받아짐
				OnRecvCompleted(null, args);
			}

		}

		void OnRecvCompleted(object? sender, SocketAsyncEventArgs args)
		{
			if ( args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
			{
                // 성공
				try
				{
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"> {recvData}");
                } catch ( Exception e )
				{
					// 혹시에 예외처리
					Console.WriteLine($"|OnRecvComplted Failed {e}");
				}
            }
			else
			{
				// TODO : disconnect
			}
		}
        #endregion
    }
}

