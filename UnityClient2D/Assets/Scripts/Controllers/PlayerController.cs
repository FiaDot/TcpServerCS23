using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
public class PlayerController : MonoBehaviour
{
	// TODO : temp
	// public Grid _grid;

	// grid에서 좌표
	private Vector3Int _cellPos = Vector3Int.zero;
	
    public float _speed = 5.0f;
    private bool _isMoving = false;

    private Animator _animator;
	MoveDir _dir = MoveDir.Down;

	public MoveDir Dir
	{
		get { return _dir; }
		set
		{
			if (_dir == value)
				return;
			
			switch (value)
			{
				case MoveDir.Up:
					_animator.Play("WALK_BACK");
					transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					break;
				case MoveDir.Down:
					_animator.Play("WALK_FRONT");
					transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					break;
				case MoveDir.Left:
					_animator.Play("WALK_RIGHT");
					transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
					break;
				case MoveDir.Right:
					_animator.Play("WALK_RIGHT");
					transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					break;
				case MoveDir.None:
					// 이전 상태에 따라 방향 설정
					if (_dir == MoveDir.Up)
					{
						_animator.Play("IDLE_BACK");
						transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					}
					else if (_dir == MoveDir.Down)
					{
						_animator.Play("IDLE_FRONT");
						transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					}
					else if (_dir == MoveDir.Left)
					{
						_animator.Play("IDLE_RIGHT");
						transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f); // 애니가 오른쪽 기준이라 회전시켜줌
					}
					else
					{
						_animator.Play("IDLE_RIGHT");
						transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					}
					break;
			}

			_dir = value;
		}
	}
	
    void Start()
    {
	    _animator = GetComponent<Animator>();
	    
	    // 0.5f 는 중심점 
	    Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f);
	    // Vector3 pos = _grid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f);
	    transform.position = pos;
    }
    
    void Update()
    {
	    GetDirInput();
	    UpdateIsMoving();
	    UpdatePosition();
    }

    void LateUpdate()
	{
		Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
	}

        
    // 실제 좌표 업데이트 
    void UpdatePosition()
    {
	    if (!_isMoving)
		    return;

	    Vector3 dest = Managers.Map.CurrentGrid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f);
	    Vector3 moveDir = dest - transform.position;

	    float dist = moveDir.magnitude;
	    if ( dist < _speed * Time.deltaTime)
	    {
		    // 도착
		    transform.position = dest;
		    _isMoving = false;
	    }
	    else
	    {
		    // 이동중  
		    // TODO : 속도빠르면 넘어감 ;;
		    transform.position += moveDir.normalized * (_speed * Time.deltaTime);
		    // _isMoving = true;
	    }
    }
    
    void UpdateIsMoving()
    {
	    if (_isMoving || _dir == MoveDir.None)
		    return;
	    
		Vector3Int destPos = _cellPos;

		switch (_dir)
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
			_cellPos = destPos;
			_isMoving = true;
		}
    }
    
    // 키입력 방향 설정
    void GetDirInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
	        // transform.position += Vector3.up * (Time.deltaTime * _speed);
			Dir = MoveDir.Up;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			// transform.position += Vector3.down * (Time.deltaTime * _speed);
			Dir = MoveDir.Down;
		}
		else if (Input.GetKey(KeyCode.A))
		{
			// transform.position += Vector3.left * (Time.deltaTime * _speed);
			Dir = MoveDir.Left;
		}
		else if (Input.GetKey(KeyCode.D))
		{
			// transform.position += Vector3.right * (Time.deltaTime * _speed);
			Dir = MoveDir.Right;
		}
		else
		{
			Dir = MoveDir.None;
		}
    
    }
}
