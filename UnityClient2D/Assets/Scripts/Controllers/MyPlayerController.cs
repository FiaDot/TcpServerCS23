using System;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MyPlayerController : PlayerController
{
	bool _moveKeyPressed = false;

	private int _rtt = 100;
	public int RTT
	{
		get { return _rtt; }
		set { _rtt = Math.Max(_rtt, 0); }
	}
	
	protected override void Init()
	{
		base.Init();

		StartCoroutine("SendRtt");
		
		// StartCoroutine("SendMoving");
	}

	protected override void UpdateController()
	{
		switch (State)
		{
			case CreatureState.Idle:
				GetDirInput();
				break;
			case CreatureState.Moving:
				GetDirInput();
				break;
		}
		
		base.UpdateController();

		// InternalUpdate();
	}

	protected override void UpdateIdle()
	{
		// 이동 상태로 갈지 확인
		if (_moveKeyPressed)
		{
			State = CreatureState.Moving;
			return;
		}

		if (_coSkillCooltime == null && Input.GetKey(KeyCode.Space))
		{
			Debug.Log("Skill !");

			C_Skill skill = new C_Skill() { Info = new SkillInfo() };
			skill.Info.SkillId = 1;
			Managers.Network.Send(skill);

			_coSkillCooltime = StartCoroutine("CoInputCooltime", 0.2f);
		}
	}

	Coroutine _coSkillCooltime;
	IEnumerator CoInputCooltime(float time)
	{
		yield return new WaitForSeconds(time);
		_coSkillCooltime = null;
	}

	void LateUpdate()
	{
		Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
	}

	// 키보드 입력
	void GetDirInput()
	{
		_moveKeyPressed = true;

		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
		{
			Dir = MoveDir.Up;
		}
		else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
		{
			Dir = MoveDir.Down;
		}
		else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
		{
			Dir = MoveDir.Left;
		}
		else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
		{
			Dir = MoveDir.Right;
		}
		else
		{
			_moveKeyPressed = false;
		}

		if (_moveKeyPressed)
		{
			Vector3 destPos = CellPos;
			
			switch (Dir)
			{
				case MoveDir.Up:
					// destPos += Vector3.up;
					destPos += Vector3.up * _speed * Time.deltaTime;
					break;
				case MoveDir.Down:
					destPos += Vector3.down * _speed * Time.deltaTime;
					break;
				case MoveDir.Left:
					destPos += Vector3.left * _speed * Time.deltaTime;
					break;
				case MoveDir.Right:
					destPos += Vector3.right * _speed * Time.deltaTime;
					break;
			}
			
			CellPos = destPos;
			transform.position = CellPos;
		}
		else
		{
			State = CreatureState.Idle;
		}
		
		// UpdateMoving();
		// MoveToNextPos();
		CheckUpdatedFlag();
	}


	protected override void UpdateMoving()
	{
		return; 
		Vector3 destPos = CellPos;

		switch (Dir)
		{
			case MoveDir.Up:
				// destPos += Vector3.up;
				destPos += Vector3.up * _speed * Time.deltaTime;
				break;
			case MoveDir.Down:
				destPos += Vector3.down * _speed * Time.deltaTime;
				break;
			case MoveDir.Left:
				destPos += Vector3.left * _speed * Time.deltaTime;
				break;
			case MoveDir.Right:
				destPos += Vector3.right * _speed * Time.deltaTime;
				break;
		}

				if (_moveKeyPressed == false)
				{
					State = CreatureState.Idle;
					// CheckUpdatedFlag();
					return;
				}
				else
				{
					CellPos = destPos;
					transform.position = CellPos;
				}
	}
	
	
	protected override void MoveToNextPos()
	{
		return;
		
		if (_moveKeyPressed == false)
		{
			State = CreatureState.Idle;
			CheckUpdatedFlag();
			return;
		}

		Vector3 destPos = CellPos;

		switch (Dir)
		{
			case MoveDir.Up:
				destPos += Vector3.up;
				break;
			case MoveDir.Down:
				destPos += Vector3.down;
				break;
			case MoveDir.Left:
				destPos += Vector3.left;
				break;
			case MoveDir.Right:
				destPos += Vector3.right;
				break;
		}

		// if (Managers.Map.CanGo(destPos))
		// {
		// 	if (Managers.Object.Find(destPos) == null)
		// 	{
				CellPos = destPos;
		// 	}
		// }

		CheckUpdatedFlag();
	}
	
	protected override void CheckUpdatedFlag()
	{
		m_fInterval += Time.unscaledDeltaTime;
		
//		if (_updated)
		{
			if (MOVING_INTERVAL_TIME <= m_fInterval)
			{
				m_fInterval = 0f;

				C_Move movePacket = new C_Move();
				movePacket.PosInfo = PosInfo;
				Managers.Network.Send(movePacket);
				_updated = false;
			}
		}
	}
	
	IEnumerator SendMoving()
	{
		while(true)
		{
			if (_updated)
			{
				C_Move movePacket = new C_Move();
				movePacket.PosInfo = PosInfo;
				Managers.Network.Send(movePacket);
				_updated = false;
			}
		
			yield return new WaitForSeconds(1.0f/5.0f);
		}
		
	}
		

	IEnumerator SendRtt()
	{
		while (true)
		{
			C_Rtt rtt = new C_Rtt();
			rtt.Time = Time.realtimeSinceStartup;
			Managers.Network.Send(rtt);
			yield return new WaitForSeconds(1);
		}
	}
	
	
	
	public const float MOVING_INTERVAL_TIME = 0.3f;
	protected long m_nTime = 0; // 이동 패킷 받은 시간.
	protected float m_fSyncDiff = 0f; // 동기화 시간 (서버에서 받는 시간).
	protected float m_fInterval = 0f; // 이동 패킷 전송 주기.
	protected float m_fSyncDeltaTime = 0f; // 동기화 보정 시간.(x)

	protected Vector2 m_vTargetPos = Vector3.zero; // 목표 좌표.
	
	protected Vector2 m_vInputDir = Vector3.zero; // 이동 입력 방향.
	protected Vector2 m_vMoveDir = Vector3.zero; // 이동 입력 방향.

	public const float currentMovementSpeed = 10.0f;
	
	void InternalUpdate()
	{
		GetDirInput2();
		
		float fDeltaTime = Time.smoothDeltaTime;

		Vector2 vPos    = transform.localPosition;
		Vector2 vNow    = Vector2.zero;
    
		m_vTargetPos = vPos+inputDir*(currentMovementSpeed*(MOVING_INTERVAL_TIME+fDeltaTime));
		m_fInterval += Time.unscaledDeltaTime;
		if( MOVING_INTERVAL_TIME <= m_fInterval )
		{
	        m_fInterval = 0f;
	        // Debug.Log("SendPos");
	        SendPos(); 
		}
		
		// vNow = Moving(vPos, _GetTargetPos(vPos, m_vTargetPos, currentMovementSpeed, fDeltaTime), currentMovementSpeed, fDeltaTime);
		vNow = _GetTargetPos(vPos, m_vTargetPos, currentMovementSpeed, fDeltaTime);
		// transform.localPosition = m_vTargetPos;
		transform.localPosition = vNow;
	}
		
	protected Vector2 _GetTargetPos(Vector2 pos, Vector2 target, float speed, float deltaTime)
	{
		// 이동 좌표 리턴.
		Vector2 vDistance = target-pos;
		float 	fDistance = vDistance.magnitude;
		float 	fMovement = speed*deltaTime;

		fMovement = Mathf.Min(fMovement, fDistance);
		// Debug.Log($"fMovement {fMovement.ToString()}");
		
		return pos+vDistance.normalized*fMovement;
	}
	
	public Vector3 Moving(Vector3 pos, Vector3 target, float speed, float deltaTime)
	{
		// 이동 처리.
		Vector2							vPos			= pos;
		Vector2							vDistance		= target-pos;
		Vector2							vDir			= vDistance.normalized;
		float 							fDistance		= vDistance.magnitude;
		float 							fMovement		= speed*deltaTime;
		bool 							IsDrop			= false;

		fMovement = Mathf.Min(fMovement, fDistance);
		return vPos;
	}
	
	public Vector2 inputDir
	{
		get { return m_vInputDir; }
		set		
		{
			if( m_vInputDir != value )
			{
				m_vInputDir = value;
				m_vMoveDir 	= value;
			}
		}
	}
		
	// 키보드 입력
	void GetDirInput2()
	{
		Vector2 dir = Vector2.zero;
		dir.x += Input.GetAxisRaw("Horizontal");
		dir.y += Input.GetAxisRaw("Vertical");

		// Debug.Log($"dir {dir.ToString()}");
		m_vMoveDir = transform.right*dir.x + transform.up*dir.y;
		m_vMoveDir.Normalize();
		
		inputDir = m_vMoveDir;
	}

	
	void SendPos()
	{
		// 	_CmdRpcPos(E_PACKET_TYPE.Command, this.tmCache.localPosition, this.inputDir, 
		// this.tmCache.localRotation, UNetManager.Inst.time
		//, m_IsJump || true == this.buffManager.isMoving || 0.0001f <= this.knockBack.sqrMagnitude
		//, this.isDeath
		//, this.inputMovementSpeed);

		CellPos = transform.localPosition;
		
		C_Move movePacket = new C_Move();
		movePacket.PosInfo = PosInfo;
		Managers.Network.Send(movePacket);
	}
		
	
}
