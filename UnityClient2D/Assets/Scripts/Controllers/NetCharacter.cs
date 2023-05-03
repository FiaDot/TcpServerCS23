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

	public void InitPos()
	{
		transform.position = new Vector3(NetMoveInfo.Pos.X, NetMoveInfo.Pos.Y, NetMoveInfo.Pos.Z);
	}


void Start()
    {
        
    }

 
    void Update()
    {
        
    }

    
}
