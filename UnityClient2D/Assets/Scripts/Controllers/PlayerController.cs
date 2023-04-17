using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float _speed = 5.0f;
    
    void Start()
    {
        
    }
    
    // TODO : 입력과 이동 분리 필요!
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
	        transform.position += Vector3.up * (Time.deltaTime * _speed);
			// Dir = MoveDir.Up;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			transform.position += Vector3.down * (Time.deltaTime * _speed);
			// Dir = MoveDir.Down;
		}
		else if (Input.GetKey(KeyCode.A))
		{
			transform.position += Vector3.left * (Time.deltaTime * _speed);
			// Dir = MoveDir.Left;
		}
		else if (Input.GetKey(KeyCode.D))
		{
			transform.position += Vector3.right * (Time.deltaTime * _speed);
			// Dir = MoveDir.Right;
		}
		// else
		// {
		// 	Dir = MoveDir.None;
		// }
    }
}
