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
        #endregion

        Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
        Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();

        // ClientSession?
        public void Register()
		{
            _onRecv.Add((ushort)MsgId.SPing, MakePacket<S_Ping>);
            _handler.Add((ushort)MsgId.SPing, PacketHandler.SPingHandler);
        }

        void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IMessage, new()
        {
            T p = new T();
            p.MergeFrom(buffer.Array);
            

            Action<PacketSession, IMessage>? action = null;
            // TODO : protobuf id 필요 한데 ;;
            if ( _handler.TryGetValue((ushort)MsgId.SPing, out action) )
                action.Invoke(session, p);
        }

        public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
        {
            ushort count = 0;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count); // +2 (size field)
            count += 2;

            Action<PacketSession, ArraySegment<byte>>? action = null;
            if ( _onRecv.TryGetValue(packetId, out action) )
            {
                action.Invoke(session, buffer);
            }
        }      
    }
}

