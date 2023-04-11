using System;
namespace ServerCore
{
	// TLS 쓰기때문에 lock 불필요
	public class SendBufferHelper
	{
		public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>( () => { return null; });

		public static int ChunkSize { get; set; } = 4096 * 100;


		public static ArraySegment<byte> Open(int reservedSize)
		{
			// 없으면 할당
			if (CurrentBuffer.Value == null)
				CurrentBuffer.Value = new SendBuffer(ChunkSize);

			// 요청한거보다 남은공간이 부족하면 할당 
			if (CurrentBuffer.Value.FreeSize < reservedSize)
				CurrentBuffer.Value = new SendBuffer(ChunkSize);

			return CurrentBuffer.Value.Open(reservedSize);
		}

		public static ArraySegment<byte> Close(int usedSize)
		{
			return CurrentBuffer.Value.Close(usedSize);
		}
	}

    // 다수의 스레드가 참조 하지만
    // Write 는 하나의 스레드에서 SendBufferHelp.Open, Close 를 통해서 하기에 문제 없음 

    // case1. T1 - Send - Enqueue - RegisterSend
    // case2. T1 - Send - Enqueue
	//		  T2				  - RegissterSend				
    public class SendBuffer
	{
		byte[] _buffer;
		int _usedSize = 0;

		public int FreeSize { get { return _buffer.Length - _usedSize; } }

		public SendBuffer(int chunkSize)
		{
			_buffer = new byte[chunkSize];
		}

		public ArraySegment<byte> Open(int reservedSize)
		{
			if (reservedSize > FreeSize)
				return null;

			return new ArraySegment<byte>(_buffer, _usedSize, reservedSize);
		}

		// 쓰고 영역 반환
		public ArraySegment<byte> Close(int usedSize)
		{
			ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
			_usedSize += usedSize;
			return segment;
		}
		
	}
}

  