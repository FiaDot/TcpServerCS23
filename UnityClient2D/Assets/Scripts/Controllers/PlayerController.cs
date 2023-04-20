using System.Collections;
using Google.Protobuf.Protocol;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
	protected Coroutine _coSkill;
	private bool _rangeSkill = false;
	
	
	protected override void UpdateAnimation()
	{
		if (State == CreatureState.Idle)
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
		else if (State == CreatureState.Moving)
		{
			switch (Dir)
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
		else if (State == CreatureState.Skill)
		{
			switch (_lastDir)
			{
				case MoveDir.Up:
					_animator.Play(_rangeSkill ? "ATTACK_WEAPON_BACK" : "ATTACK_BACK");
					_sprite.flipX = false;
					break;
				case MoveDir.Down:
					_animator.Play(_rangeSkill ? "ATTACK_WEAPON_FRONT" : "ATTACK_FRONT");
					_sprite.flipX = false;
					break;
				case MoveDir.Left:
					_animator.Play(_rangeSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
					_sprite.flipX = true;
					break;
				case MoveDir.Right:
					_animator.Play(_rangeSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
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
		base.UpdateController();
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
	}
	
	
	public void UseSkill(int skillId)
	{
		if (skillId == 1)
		{
			_coSkill = StartCoroutine("CoStartPunch");
		}
	}

	protected virtual void CheckUpdatedFlag()
	{

	}

	// IEnumerator CoStartPunch()
	// {
	// 	// 피격 판정
	// 	GameObject go = Managers.Object.Find(GetFrontCellPos());
	// 	if ( go )
	// 	{
	// 		CreatureController cc = go.GetComponent<CreatureController>();
	// 		if (cc != null)
	// 			cc.OnDamaged();
	// 		// print(target.name);
	// 	}
	// 	
	// 	_isRangeSkill = false;
	// 	
	// 	// 대기시간
	// 	yield return new WaitForSeconds(0.5f);
	// 	State = CreatureState.Idle;
	// 	_coSkill = null;
	// }
	//
	// IEnumerator CoStartShootArrow()
	// {
	// 	GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
	// 	ArrowController ac = go.GetComponent<ArrowController>();
	// 	ac.Dir = _lastDir;
	// 	ac.CellPos = CellPos;
	// 	
	// 	_isRangeSkill = true;
	// 	
	// 	// 대기시간
	// 	yield return new WaitForSeconds(0.3f);
	// 	State = CreatureState.Idle;
	// 	_coSkill = null;
	// }
	
	IEnumerator CoStartPunch()
	{
		// 대기 시간
		_rangeSkill = false;
		State = CreatureState.Skill;
		yield return new WaitForSeconds(0.5f);
		State = CreatureState.Idle;
		_coSkill = null;
		CheckUpdatedFlag();
	}

	IEnumerator CoStartShootArrow()
	{
		GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
		ArrowController ac = go.GetComponent<ArrowController>();
		ac.Dir = _lastDir;
		ac.CellPos = CellPos;

		// 대기 시간
		_rangeSkill = true;
		yield return new WaitForSeconds(0.3f);
		State = CreatureState.Idle;
		_coSkill = null;
	}

	public override void OnDamaged()
	{
		Debug.Log("Player HIT !");
	}
}
