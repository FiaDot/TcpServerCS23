using System;
namespace ServerCore
{
	public class RecvBuffer
	{
        /*

		1. init
		[rw][][][][][][][][][]

		2. not enough body
		[r][][][w][][][][][][]

		3. not enough length
		[r][w][][][][][][][][]

		4. not enough space
		[][][][][][][][][rw][] 

		*/

        ArraySegment<byte> _buffer;
		int _readPos;
		int _writePos;

		public RecvBuffer(int bufferSize)
		{
			_buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
		}

		// 미처리 크기
		public int DataSize { get { return _writePos - _readPos;  } }

		// 남은 공간
		public int FreeSize { get { return _buffer.Count - _writePos; } }

		// 어디 부터 읽으면 됨?
		public ArraySegment<byte> ReadSegment
		{
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
		}

		// 어디에 recv 받을것인가?
		public ArraySegment<byte> WriteSegment
		{
			get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
		}

		public void Clean()
		{
			int dataSize = DataSize;
			if ( 0 == dataSize )
			{
				// 남은 데이터가 없으면 모든 데이터 다 처리 상태. 커서 위치만 리셋
				_readPos = _writePos = 0;
			}
			else
			{
				// 남은 데이터 있으면 시작 위치로 복사
				Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
				_readPos = 0;
				_writePos -= dataSize;
			}
		}

		public bool OnRead(int numOfBytes)
		{
			if (numOfBytes > DataSize)
				return false; // 오류!

			_readPos += numOfBytes;
			return true;
		}

		public bool OnWrite(int numOfBytes)
		{
			if (numOfBytes > FreeSize)
				return false; // 오류!

			_writePos += numOfBytes;
			return true;				
		}
	}
}

