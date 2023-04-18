using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using UnityEngine;

public class ServerSession : PacketSession
{
	public void Send(IMessage message)
	{
		string messageName = message.Descriptor.Name.Replace("_", string.Empty);
		// TODO : try catch
		MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), messageName);

		int size = message.CalculateSize();

		byte[] sendBuffer = new byte[size + 4];
		// write size
		Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
		// write id
		Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
		// write encoded
		Array.Copy(message.ToByteArray(), 0, sendBuffer, 4, size);

		Send(new ArraySegment<byte>(sendBuffer));
	}
	
	public override void OnConnected(EndPoint endPoint)
	{
		Debug.Log($"OnConnected : {endPoint}");
		

	}

	public override void OnDisconnected(EndPoint endPoint)
	{
		Debug.Log($"OnDisconnected : {endPoint}");
	}

	public override void OnRecvPacket(ArraySegment<byte> buffer)
	{
		PacketManager.Instance.OnRecvPacket(this, buffer);
	}

	public override void OnSend(int numOfBytes)
	{
		// Debug.Log($"Transferred bytes: {numOfBytes}");
	}
}