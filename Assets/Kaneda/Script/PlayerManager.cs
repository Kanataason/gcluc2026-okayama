using UnityEngine;

// プレイヤー管理クラス
public class PlayerManager : CharaBase
{
    PlayerMoveManager c_PlayerMove;
    PlayerInputManager c_PlayerInput;

    //現在のプレイヤー状態
    Player.PlayerState s_PlayerState;

    public override void Start()
    {
        base.Start();

        c_PlayerMove = GetComponent<PlayerMoveManager>();
        c_PlayerInput = GetComponent<PlayerInputManager>();

        //初期状態
        s_PlayerState = Player.PlayerState.Idle;
    }

    public override void Update()
    {
        StateUpdate();
    }

    // 状態更新
    void StateUpdate()
    {
        switch (s_PlayerState)
        {
            case Player.PlayerState.Idle:
                IdleUpdate();
                break;

            case Player.PlayerState.Move:
                MoveUpdate();
                break;

            case Player.PlayerState.Attack:
                break;

            case Player.PlayerState.Damage:
                break;

            case Player.PlayerState.Die:
                break;
            case Player.PlayerState.Jump:
                JumpUpdate();
                break;
        }
    }

    // 待機状態
    void IdleUpdate()
    {
        float h = c_PlayerInput.GetHorizontal();
        float v = c_PlayerInput.GetVertical();

        if (h != 0 || v != 0)
        {
            s_PlayerState = Player.PlayerState.Move;
        }
        if (c_PlayerInput.GetJump())
        {
            s_PlayerState = Player.PlayerState.Jump;
        }
    }

    // 移動状態
    void MoveUpdate()
    {
        c_PlayerMove.MoveUpdate();

        float h = c_PlayerInput.GetHorizontal();
        float v = c_PlayerInput.GetVertical();

        if (h == 0 && v == 0)
        {
            s_PlayerState = Player.PlayerState.Idle;
        }
        if (c_PlayerInput.GetJump())
        {
            s_PlayerState = Player.PlayerState.Jump;
        }
    }

    void JumpUpdate()
    {
        c_PlayerMove.MoveUpdate();
    }
}