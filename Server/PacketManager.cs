using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;

namespace Server
{
	public class PacketManager
	{
		#region singleton
		static PacketManager? _instance;
		public static PacketManager Instance
		{
			get
			{
				if ( null == _instance )
					_instance = new PacketManager();
                return _instance;
            }
		}

        PacketManager()
        {
            Register();
        }
        #endregion

        Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
        Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();        

        public void Register()
		{            
            _onRecv.Add((ushort)MsgId.SPing, MakePacket<S_Ping>);
            _handler.Add((ushort)MsgId.SPing, PacketHandler.S_PingHandler);
        }

        public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
        {
            ushort count = 0;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count); // +2 (size field)
            count += 2;

            Action<PacketSession, ArraySegment<byte>, ushort>? action = null;
            if (_onRecv.TryGetValue(packetId, out action))
            {
                action.Invoke(session, buffer, packetId);
            }
        }

        void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
        {
            T packet = new T();
            // len(2) + id(2) 해서 4바이트 조절
            packet.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
            
            Action<PacketSession, IMessage>? action = null;
            
            if ( _handler.TryGetValue(id, out action) )
                action.Invoke(session, packet);
        }


    }
}

