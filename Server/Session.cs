using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
	public abstract class Session
	{
		Socket _socket;

		// 접속 해제 처리 여부 (0=미처리,1=처리완료)
		int _disconnected = 0;

        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
		Queue<byte[]> _sendQueue = new Queue<byte[]>();
		// bool _pending = false;
		object _lock = new object();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

		// 상속처리를 위한 이벤트
		public abstract void OnConnected(EndPoint endPoint);
		public abstract void OnRecv(ArraySegment<byte> buffer);
		public abstract void OnSend(int numOfBytes);
		public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket socket)
		{
			_socket = socket;


            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            // recvArgs.UserToken // 식별자로 customize 가능
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
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

				// 대기중인거 없음
				// if (_pending == false)
				if ( _pendingList.Count == 0)
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

			OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신

		void RegisterSend()
		{
			// send에서 호출되므로 별도의 락이 필요 없음
            // _pending = true;

			// 불필요 코드 인듯 ...
			// _pendingList.Clear();

			while( _sendQueue.Count > 0 )
			{
                byte[] buff = _sendQueue.Dequeue();
                // _sendArgs.SetBuffer(buff, 0, buff.Length);
                // or _sendArgs.BufferList 사용 (개별 패킷에 대해 .Add는 안됨)
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));                
            }

            _sendArgs.BufferList = _pendingList;
			
			
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
						_sendArgs.BufferList = null; // optional
                        _pendingList.Clear();

						OnSend(_sendArgs.BytesTransferred);
						
						// 전송중에 send를 호출해서 send queue에 뭔가 더 들어와 있을 때
						if ( _sendQueue.Count > 0 )
						{
							RegisterSend(); 
						}
						/*
						else
						{
							// 다 보냄
                            _pending = false;
                        }
						*/
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

        void RegisterRecv()
        {
			bool pending = _socket.ReceiveAsync(_recvArgs);
			if ( pending == false)
			{
				// 바로 받아짐
				OnRecvCompleted(null, _recvArgs);
			}

		}

		void OnRecvCompleted(object? sender, SocketAsyncEventArgs args)
		{
			if ( args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
			{
                // 성공
				try
				{
					OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));

					// 빠져있었음 ㅡ;
					RegisterRecv();
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

