using System.Collections;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
	private Coroutine _coSkill;
	private bool _isRangeSkill = false;
	
	
	protected override void UpdateAnimation()
	{
		if (_state == CreatureState.Idle)
		{
			switch (_lastDir)
			{
				case MoveDir.Up:
					_animator.Play("IDLE_BACK");
					_sprite.flipX = false;
					break;
				case MoveDir.Down:
					_animator.Play("IDLE_FRONT");
					_sprite.flipX = false;
					break;
				case MoveDir.Left:
					_animator.Play("IDLE_RIGHT");
					_sprite.flipX = true;
					break;
				case MoveDir.Right:
					_animator.Play("IDLE_RIGHT");
					_sprite.flipX = false;
					break;
			}
		}
		else if (_state == CreatureState.Moving)
		{
			switch (_dir)
			{
				case MoveDir.Up:
					_animator.Play("WALK_BACK");
					_sprite.flipX = false;
					break;
				case MoveDir.Down:
					_animator.Play("WALK_FRONT");
					_sprite.flipX = false;
					break;
				case MoveDir.Left:
					_animator.Play("WALK_RIGHT");
					_sprite.flipX = true;
					break;
				case MoveDir.Right:
					_animator.Play("WALK_RIGHT");
					_sprite.flipX = false;
					break;
			}
		}
		else if (_state == CreatureState.Skill)
		{
			switch (_lastDir)
			{
				case MoveDir.Up:
					_animator.Play(_isRangeSkill ? "ATTACK_WEAPON_BACK" : "ATTACK_BACK");
					_sprite.flipX = false;
					break;
				case MoveDir.Down:
					_animator.Play(_isRangeSkill ? "ATTACK_WEAPON_FRONT" : "ATTACK_FRONT");
					_sprite.flipX = false;
					break;
				case MoveDir.Left:
					_animator.Play(_isRangeSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
					_sprite.flipX = true;
					break;
				case MoveDir.Right:
					_animator.Play(_isRangeSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
					_sprite.flipX = false;
					break;
			}
		}
		else
		{

		}
	}
	
	protected override void Init()
	{
		base.Init();
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

	void LateUpdate()
	{
		Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
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
	
	// void GetIdleInput()
	// {
	// 	if (Input.GetKey(KeyCode.Space))
	// 	{
	// 		State = CreatureState.Skill;
	// 		// _coSkill = StartCoroutine("CoStartPunch");
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

		// 스킬 상태로 갈지 확인
		if (Input.GetKey(KeyCode.Space))
		{
			State = CreatureState.Skill;
			//_coSkill = StartCoroutine("CoStartPunch");
			_coSkill = StartCoroutine("CoStartShootArrow");
		}
	}

	IEnumerator CoStartPunch()
	{
		// 피격 판정
		GameObject go = Managers.Object.Find(GetFrontCellPos());
		if ( go )
		{
			CreatureController cc = go.GetComponent<CreatureController>();
			if (cc != null)
				cc.OnDamaged();
			// print(target.name);
		}
		
		_isRangeSkill = false;
		
		// 대기시간
		yield return new WaitForSeconds(0.5f);
		State = CreatureState.Idle;
		_coSkill = null;
	}
	
	IEnumerator CoStartShootArrow()
	{
		GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
		ArrowController ac = go.GetComponent<ArrowController>();
		ac.Dir = _lastDir;
		ac.CellPos = CellPos;
		
		_isRangeSkill = true;
		
		// 대기시간
		yield return new WaitForSeconds(0.3f);
		State = CreatureState.Idle;
		_coSkill = null;
	}
	
	
}
