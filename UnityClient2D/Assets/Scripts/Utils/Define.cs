using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    // protobuf 에서 정의
    //    public enum CreatureState
    // {
    //        Idle,
    //        Moving,
    //        Skill,
    //        Dead,
    // }
    //
    //    public enum MoveDir
    //    {
    //        None,
    //        Up,
    //        Down,
    //        Left, 
    //        Right,
    //    }
    
    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Game,
        Game3D,
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }

    public enum UIEvent
    {
        Click,
        Drag,
    }
}
