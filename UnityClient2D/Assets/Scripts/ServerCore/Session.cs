using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        // [size(2)][packetId(2)][ ... ][size(2)][packetId(2)][ ... ]
        // dataSize = [size(2)][packetId(2)]
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;

            while (true)
            {
                // 최소한 헤더는 파싱할 수 있는지 확인
                if (buffer.Count < HeaderSize)
                    break;

                // 패킷이 완전체로 도착했는지 확인 ( header size + packet 전체 크기를 dataSize로 쓰고있음. 통으로! )
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                // 여기까지 왔으면 패킷 조립 가능 ( ArraySegment는 struct로 stack 사용)
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
	{
		Socket _socket;

		// 접속 해제 처리 여부 (0=미처리,1=처리완료)
		int _disconnected = 0;

		RecvBuffer _recvBuffer = new RecvBuffer(1024);

        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
		Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
		// bool _pending = false;
		object _lock = new object();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

		// 상속처리를 위한 이벤트
		public abstract void OnConnected(EndPoint endPoint);
		public abstract int OnRecv(ArraySegment<byte> buffer);
		public abstract void OnSend(int numOfBytes);
		public abstract void OnDisconnected(EndPoint endPoint);

        void Clear()
        {
            // 접속 해제시 버퍼 해제
            lock(_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Start(Socket socket)
		{
			_socket = socket;


            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            // recvArgs.UserToken // 식별자로 customize 가능
            // _recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send(ArraySegment<byte> sendBuff)
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
            Clear();
        }

        #region 네트워크 통신

		void RegisterSend()
		{
			// 이미 접속 끊긴 상태
			if (1 == _disconnected)
				return;

            // send에서 호출되므로 별도의 락이 필요 없음
            // _pending = true;

            // 불필요 코드 인듯 ...
            // _pendingList.Clear(); 

            while ( _sendQueue.Count > 0 )
			{
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                // _sendArgs.SetBuffer(buff, 0, buff.Length);
                // or _sendArgs.BufferList 사용 (개별 패킷에 대해 .Add는 안됨)
                _pendingList.Add(buff);

            }

            _sendArgs.BufferList = _pendingList;
			

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false)
                {
                    OnSendCompleted(null, _sendArgs);
                }
            } catch ( Exception e )
            {
                Console.WriteLine($"! RegisterSend failed {e}");
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
            // 이미 접속 끊긴 상태
            if (1 == _disconnected)
                return;

            // 받기 전에 버퍼 공간 확보
            _recvBuffer.Clean();

			// 쓰기 세그먼트에다가 받으면 됨
			ArraySegment<byte> segment = _recvBuffer.WriteSegment;
			_recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            { 
			    bool pending = _socket.ReceiveAsync(_recvArgs);
			    if ( pending == false)
			    {
				    // 바로 받아짐
				    OnRecvCompleted(null, _recvArgs);
			    }
            } catch (Exception e )
            {
                Console.WriteLine($"! RegisterRecv failed {e}");
            }
        }

		void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
		{
			if ( args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
			{
                // 성공
				try
				{
					// write corsor 이동
					if ( !_recvBuffer.OnWrite(args.BytesTransferred) )
					{ 
						Disconnect();
						return;
					}

                    // OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
                    int processedLen = OnRecv(_recvBuffer.ReadSegment);
					if (processedLen < 0 || _recvBuffer.DataSize < processedLen )
					{
						// 컨텐츠쪽에서 잘못된 사이즈를 반환 했거나 가지고 있는 사이즈 보다 많이 처리했다면 
						Disconnect();
						return;
					}

					// 처리한만큼 read 이동
					if ( !_recvBuffer.OnRead(processedLen))
					{
						Disconnect();
						return;
					}

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

