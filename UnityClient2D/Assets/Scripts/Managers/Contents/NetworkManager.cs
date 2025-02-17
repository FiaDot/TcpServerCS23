using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;
using System.Net.Sockets;

public class NetworkManager
{
	ServerSession _session = new ServerSession();

	public void Send(IMessage sendBuff)
	{
		_session.Send(sendBuff);
	}
	// public void Send(ArraySegment<byte> sendBuff)
	// {
	// 	_session.Send(sendBuff);
	// }

	public void Init()
	{
		// 호스트 이름 가져오기
		string host = Dns.GetHostName();

		// 호스트 엔트리 가져오기
		IPHostEntry ipHost = Dns.GetHostEntry(host);		
	
		IPAddress ipAddr = null;

		// AddressList를 순회하며 IPv4 주소 찾기
		foreach (IPAddress address in ipHost.AddressList)
		{									
			// IPv4 주소인지 확인			
			if (address.AddressFamily == AddressFamily.InterNetwork) // IPv4 확인
			{				
				ipAddr = address;
				break; // IPv4 주소를 찾았으면 루프 종료
			}
		}
		
		// IPv4 주소를 찾지 못했을 경우 처리
		if (ipAddr == null)
		{
			Debug.LogWarning("IPv4 주소를 찾을 수 없습니다. 첫 번째 주소를 사용합니다.");
			ipAddr = ipHost.AddressList[0]; // 첫 번째 주소 (IPv6일 수 있음)로 대체
		}
			
		IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
		
		Debug.Log($"Connect to: {ipAddr}");
		Connector connector = new Connector();

		// [Deprecated] for web player
		//Security.PrefetchSocketPolicy(ipAddr.ToString(), 7777); 
		
		connector.Connect(endPoint,
			() => { return _session; },
			1);
	}

	public void Update()
	{
		List<PacketMessage> list = PacketQueue.Instance.PopAll();
		foreach (PacketMessage packet in list)
		{
			Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
			if (handler != null)
				handler.Invoke(_session, packet.Message);
		}	
	}

}
