using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
public class PlayerController : MonoBehaviour
{
	// TODO : temp
	public Grid _grid;

	// grid에서 좌표
	private Vector3Int _cellPos = Vector3Int.zero;
	
    public float _speed = 5.0f;
	MoveDir Dir = Define.MoveDir.None;
	private bool _isMoving = false;
    void Start()
    {
	    // 0.5f 는 중심점 
	    Vector3 pos = _grid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f);
	    transform.position = pos;
    }
    
    void Update()
    {
	    GetDirInput();
	    UpdateIsMoving();
	    UpdatePosition();
    }

    // 실제 좌표 업데이트 
    void UpdatePosition()
    {
	    if (!_isMoving)
		    return;

	    Vector3 dest = _grid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f);
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
	    if (_isMoving)
		    return;
	    
	    switch ( Dir )
	    {
	     case MoveDir.Up:
		     _cellPos += Vector3Int.up;
		     _isMoving = true;
		     break;
	     
	     case MoveDir.Down:
		     _cellPos += Vector3Int.down;
		     _isMoving = true;
		     break;
	     
	     case MoveDir.Left:
		     _cellPos += Vector3Int.left;
		     _isMoving = true;
		     break;
	     
	     case MoveDir.Right:
		     _cellPos += Vector3Int.right;
		     _isMoving = true;
		     break;
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
