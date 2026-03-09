using UnityEngine;

public class PlayerManager : CharaBase
{
    PlayerMoveManager c_PlayerMove;

    public override void Start()
    {
        base.Start();

        c_PlayerMove = GetComponent<PlayerMoveManager>();
    }

}