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

	private Vector3 inputDir;
	
	// cache
	private NetMove _netMoveInfo = new NetMove();
	
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
	public void InitPos(NetMove netMoveInfo)
	{
		transform.position = ToVector3(netMoveInfo.Pos);
	}
	
	void Start()
    {
        
    }

 
    void Update()
    {
	    if (IsMine)
	    {
		    InputMovement();
		    SendMove();
		    return;
	    }

	    Interpolation();
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

    // [SerializeField]
    public float MOVING_INTERVAL_TIME = 0.3f;
    private float sendPeriod = 0f; // 이동 패킷 전송 주기.
    
    void SendMove()
    {
	    sendPeriod += Time.unscaledDeltaTime;
	    if (MOVING_INTERVAL_TIME > sendPeriod)
		    return;
	    sendPeriod = 0f;

		C_Move movePacket = new C_Move();
		_netMoveInfo.Pos = ToVector3Net(transform.position);
		
		movePacket.NetMoveInfo = _netMoveInfo;
		Managers.Network.Send(movePacket);
    }
    
    public void OnNetworkMove(NetMove netMoveInfo)
    {
	    if (IsMine)
		    return;

	    // Debug.Log(transform.position);
	    // transform.position = ToVector3(netMoveInfo.Pos);
	    
	    syncTimeElapsed = 0f;
	    syncDuration = Time.time - lastSynchronizationTime;
        lastSynchronizationTime = Time.time;
        // Debug.Log(syncDuration); // sendPeriod + a 가 됨

	    srcPos = transform.position;
	    dstPos = ToVector3(netMoveInfo.Pos);
    }

    private float syncTimeElapsed = 0f; // lerp 진행 시간
    private float syncDuration = 0f; // 동기화 주기
    
    private Vector3 srcPos = Vector3.zero; // 이동 정보 받은 시점의 위치
	private Vector3 dstPos = Vector3.zero; // 이동 정보 받았을 때 가야하는 위치 
	
	private float lastSynchronizationTime = 0f; 
	
    void Interpolation()
    {
	    // 이렇게 해야 이징 처리 안되고 linear 하게 처리됨
	    if (syncTimeElapsed < syncDuration)
	    {
			syncTimeElapsed += Time.deltaTime;
			transform.position = Vector3.Lerp(srcPos, dstPos, syncTimeElapsed / syncDuration);
	    }
	    else
	    {
		    transform.position = dstPos;
	    }
    }
    
}
