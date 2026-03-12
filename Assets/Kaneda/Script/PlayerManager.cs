using UnityEngine;
using static UnityEditor.PlayerSettings;

// プレイヤー管理クラス
// プレイヤーの状態を管理する
public class PlayerManager : CharaBase
{
    //移動管理クラス
    PlayerMoveManager c_PlayerMove;

    //入力管理クラス
    PlayerInputManager c_PlayerInput;

    //現在のプレイヤー状態
    Player.PlayerState s_PlayerState;

    public override void Start()
    {
        base.Start();

        //スクリプト取得
        c_PlayerMove = GetComponent<PlayerMoveManager>();
        c_PlayerInput = GetComponent<PlayerInputManager>();

        //初期状態
        s_PlayerState = Player.PlayerState.Idle;
    }

    public override void Update()
    {
        //状態更新
        StateUpdate();
    }

    //状態ごとの処理
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

    //待機状態
    void IdleUpdate()
    {
        //入力取得
        float h = c_PlayerInput.GetHorizontal();
        float v = c_PlayerInput.GetVertical();

        //移動入力があればMove状態
        if (h != 0 || v != 0)
        {
            s_PlayerState = Player.PlayerState.Move;
        }

        //ジャンプ入力があればJump状態
        if (c_PlayerInput.GetJump())
        {
            s_PlayerState = Player.PlayerState.Jump;
        }
    }

    //移動状態
    void MoveUpdate()
    {
        //移動処理
        c_PlayerMove.MoveUpdate();

        //入力取得
        float h = c_PlayerInput.GetHorizontal();
        float v = c_PlayerInput.GetVertical();

        //入力が無くなったらIdleに戻る
        if (h == 0 && v == 0)
        {
            s_PlayerState = Player.PlayerState.Idle;
        }

        //ジャンプ入力があればJump状態
        if (c_PlayerInput.GetJump())
        {
            s_PlayerState = Player.PlayerState.Jump;
        }
    }

    //ジャンプ状態
    void JumpUpdate()
    {
        //ジャンプ中も移動処理を実行
        c_PlayerMove.MoveUpdate();
    }
}