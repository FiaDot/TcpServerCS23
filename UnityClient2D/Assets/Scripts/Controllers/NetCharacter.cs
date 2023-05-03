using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class NetCharacter : MonoBehaviour
{
	public int Id { get; set; }

	public float _speed = 5.0f;

	public bool IsMine { get; set; }

	private NetMove _netMoveInfo = new NetMove();

	public NetMove NetMoveInfo
	{
		get { return _netMoveInfo; }
		set { _netMoveInfo = value; }
	}

	Vector3 ToVector3(vector3Net netValue)
	{
		return new Vector3(netValue.X, netValue.Y, netValue.Z);
	}
	
	// 서버 접속 후 초기 위치 지정
	public void InitPos()
	{
		transform.position = ToVector3(NetMoveInfo.Pos);
	}
	
	void Start()
    {
        
    }

 
    void Update()
    {
        
    }

    
}
