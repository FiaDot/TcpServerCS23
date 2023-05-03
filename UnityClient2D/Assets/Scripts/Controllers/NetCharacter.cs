using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class NetCharacter : MonoBehaviour
{
	public int Id { get; set; }

	[SerializeField]
	public float _speed = 5.0f;
	
	public bool IsMine { get; set; }

	private NetMove _netMoveInfo = new NetMove();

	public NetMove NetMoveInfo
	{
		get { return _netMoveInfo; }
		set { _netMoveInfo = value; }
	}

	// public Vector3 Dir
	// {
	// 	get { return ToVector3(_netMoveInfo.Dir); }
	// 	set
	// 	{
	// 		
	// 	}
	// }

	private Vector3 inputDir;
	
	
	Vector3 ToVector3(vector3Net netValue)
	{
		return new Vector3(netValue.X, netValue.Y, netValue.Z);
	}

	vector3Net ToVector3Net(Vector3 v)
	{
		vector3Net net = new vector3Net();
		net.X = v.x;
		net.Y = v.y;
		net.Z = v.z;
		return net;
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
	    if (IsMine)
	    {
		    InputMovement();
	    }
	    
    }

    void InputMovement()
    {
	    Vector3 destPos = Vector3.zero;
	    Vector3 dir = Vector3.zero;
	    
	    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
		{
			dir = Vector3.up;
		}
		else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
		{
			dir = Vector3.down;
		}
		else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
		{
			dir = Vector3.left;
		}
		else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
	    {
		    dir = Vector3.right;
		}
	    
	    if ( dir != Vector3.zero )
	    {
			destPos += dir * (_speed * Time.deltaTime);
			// Debug.Log(destPos);

			transform.position += destPos;
	    }
    }

    
    
}
