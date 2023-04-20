using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using static Define;

public class MyPlayerController : PlayerController
{
	// 내가 컨트롤하는 캐릭터에 대한 제어
	
    protected override void Init()
    {
        base.Init();
    }
    
    void LateUpdate()
	{
		Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
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

	// 키보드 입력
	void GetDirInput()
	{
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
			Dir = MoveDir.None;
		}
	}
	
	// protected override void UpdateIdle()
	// {
	// 	// 이동 상태로 갈지 확인
	// 	if (Dir != MoveDir.None)
	// 	{
	// 		State = CreatureState.Moving;
	// 		return;
	// 	}
	//
	// 	// 스킬 상태로 갈지 확인
	// 	if (Input.GetKey(KeyCode.Space))
	// 	{
	// 		State = CreatureState.Skill;
	// 		//_coSkill = StartCoroutine("CoStartPunch");
	// 		_coSkill = StartCoroutine("CoStartShootArrow");
	// 	}
	// }

	protected override void UpdateIdle()
	{
		// 이동 상태로 갈지 확인
		if (Dir != MoveDir.None)
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

	protected override void MoveToNextPos()
	{
		if (Dir == MoveDir.None)
		{
			State = CreatureState.Idle;
			CheckUpdatedFlag();
			return;
		}

		Vector3Int destPos = CellPos;

		switch (Dir)
		{
			case MoveDir.Up:
				destPos += Vector3Int.up;
				break;
			case MoveDir.Down:
				destPos += Vector3Int.down;
				break;
			case MoveDir.Left:
				destPos += Vector3Int.left;
				break;
			case MoveDir.Right:
				destPos += Vector3Int.right;
				break;
		}

		if (Managers.Map.CanGo(destPos))
		{
			if (Managers.Object.Find(destPos) == null)
			{
				CellPos = destPos;
			}
		}

		CheckUpdatedFlag();
	}

	void CheckUpdatedFlag()
	{
		if ( _updated )
		{
			C_Move sendPacket = new C_Move();
			sendPacket.PosInfo = PosInfo;
			Managers.Network.Send(sendPacket);
			_updated = false;
		}
	}
}
