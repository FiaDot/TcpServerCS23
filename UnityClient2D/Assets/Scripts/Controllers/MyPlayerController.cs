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
		
		StartCoroutine("SendMoving");
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
	}


	protected override void UpdateMoving()
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

		// if (Managers.Map.CanGo(destPos))
		// {
		// 	if (Managers.Object.Find(destPos) == null)
		// 	{
				// 	}
				// }

				// CheckUpdatedFlag();

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
		if (_updated)
		{
			C_Move movePacket = new C_Move();
			movePacket.PosInfo = PosInfo;
			Managers.Network.Send(movePacket);
			_updated = false;
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
	
}
