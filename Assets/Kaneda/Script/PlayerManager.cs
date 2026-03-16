using UnityEngine;

// プレイヤー管理クラス
// 状態管理のみを担当する
public class PlayerManager : CharaBase
{
    public enum PlayerState
    {
        Idle,
        Move,
        Jump,
        Attack,
        Damage,
        Die
    }

    // 入力管理
    PlayerInputManager c_PlayerInputManager;

    // 移動管理
    PlayerMoveManager c_PlayerMoveManager;

    // 攻撃管理
    PlayerAttackManager c_PlayerAttackManager;

    // 現在の状態
    PlayerState s_PlayerState;

    public override void Start()
    {
        base.Start();

        c_PlayerInputManager = GetComponent<PlayerInputManager>();
        c_PlayerMoveManager = GetComponent<PlayerMoveManager>();
        c_PlayerAttackManager = GetComponent<PlayerAttackManager>();

        s_PlayerState = PlayerState.Idle;
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
            case PlayerState.Idle:
                IdleUpdate();
                break;

            case PlayerState.Move:
                MoveUpdate();
                break;

            case PlayerState.Jump:
                JumpUpdate();
                break;

            case PlayerState.Attack:
                AttackUpdate();
                break;

            case PlayerState.Damage:
                break;

            case PlayerState.Die:
                break;
        }
    }

    // 待機状態
    void IdleUpdate()
    {
        if (c_PlayerAttackManager.HasAttackInput())
        {
            s_PlayerState = PlayerState.Attack;
            c_PlayerAttackManager.AttackEnter();
            return;
        }

        if (c_PlayerInputManager.GetJump())
        {
            s_PlayerState = PlayerState.Jump;
            c_PlayerMoveManager.JumpEnter();
            return;
        }

        float horizontal = c_PlayerInputManager.GetHorizontal();
        float vertical = c_PlayerInputManager.GetVertical();

        if (horizontal != 0f || vertical != 0f)
        {
            s_PlayerState = PlayerState.Move;
        }
    }

    // 移動状態
    void MoveUpdate()
    {
        if (c_PlayerAttackManager.HasAttackInput())
        {
            s_PlayerState = PlayerState.Attack;
            c_PlayerAttackManager.AttackEnter();
            return;
        }

        if (c_PlayerInputManager.GetJump())
        {
            s_PlayerState = PlayerState.Jump;
            c_PlayerMoveManager.JumpEnter();
            return;
        }

        c_PlayerMoveManager.MoveUpdate();

        float horizontal = c_PlayerInputManager.GetHorizontal();
        float vertical = c_PlayerInputManager.GetVertical();

        if (horizontal == 0f && vertical == 0f)
        {
            s_PlayerState = PlayerState.Idle;
        }
    }

    // ジャンプ状態
    void JumpUpdate()
    {
        c_PlayerMoveManager.MoveUpdate();

        if (c_PlayerMoveManager.GetIsGround())
        {
            float horizontal = c_PlayerInputManager.GetHorizontal();
            float vertical = c_PlayerInputManager.GetVertical();

            if (horizontal != 0f || vertical != 0f)
            {
                s_PlayerState = PlayerState.Move;
            }
            else
            {
                s_PlayerState = PlayerState.Idle;
            }
        }
    }

    // 攻撃状態
    void AttackUpdate()
    {
        c_PlayerAttackManager.AttackUpdate();

        if (!GetIsAttackFlag())
        {
            if (c_PlayerMoveManager.GetIsJumping())
            {
                s_PlayerState = PlayerState.Jump;
                return;
            }

            float horizontal = c_PlayerInputManager.GetHorizontal();
            float vertical = c_PlayerInputManager.GetVertical();

            if (horizontal != 0f || vertical != 0f)
            {
                s_PlayerState = PlayerState.Move;
            }
            else
            {
                s_PlayerState = PlayerState.Idle;
            }
        }
    }
}