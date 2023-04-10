using System;
using System.Net.Sockets;
using System.Text;

namespace Server
{
	public class Session
	{
		Socket _socket;

		// 접속 해제 처리 여부 (0=미처리,1=처리완료)
		int _disconnected = 0;

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
		Queue<byte[]> _sendQueue = new Queue<byte[]>();
		bool _pending = false;
		object _lock = new object();

        public void Start(Socket socket)
		{
			_socket = socket;

			SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
			recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            // recvArgs.UserToken // 식별자로 customize 가능
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv(recvArgs);
        }

        public void Send(byte[] sendBuff)
		{
            // _socket.Send(sendBuff);
            // 잦은 호출과 인자 재사용이 포인트

            // 멀티스레드에서는 인자 공유됨으로 오류 발생
            // _sendArgs.SetBuffer(sendBuff, 0, sendBuff.Length);
			lock(_lock)
			{
                _sendQueue.Enqueue(sendBuff);
				if (_pending == false)
				{
					RegisterSend();
				}
            }            
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

		void RegisterSend()
		{
			// send에서 호출되므로 별도의 락이 필요 없음
            _pending = true;

            byte[] buff = _sendQueue.Dequeue();
			_sendArgs.SetBuffer(buff, 0, buff.Length);

			bool pending = _socket.SendAsync(_sendArgs);
			if ( pending == false )
			{
				OnSendCompleted(null, _sendArgs);
			}
		}

		void OnSendCompleted(object sender, SocketAsyncEventArgs args)
		{
            // [락 처리가 필요한 이유]
            // 1. RegisterSend 에서 호출
            // 2. _sendArgs callback 으로 호출

			lock(_lock)
			{
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    // 성공
                    try
                    {                        
						// 전송중에 send를 호출해서 send queue에 뭔가 더 들어와 있을 때
						if ( _sendQueue.Count > 0 )
						{
							RegisterSend();
						}
						else
						{
							// 다 보냄
                            _pending = false;
                        }
                    }
                    catch (Exception e)
                    {
                        // 혹시에 예외처리
                        Console.WriteLine($"|OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }            
        }

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
				Disconnect();				
			}
		}
        #endregion
    }
}

