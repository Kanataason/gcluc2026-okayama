using UnityEngine;

// プレイヤー管理クラス
// 状態管理のみを担当する
public class PlayerManager : CharaBase
{
    // 入力管理
    PlayerInputManager c_PlayerInputManager;

    // 移動管理
    PlayerMoveManager c_PlayerMoveManager;

    // 攻撃管理
    PlayerAttackManager c_PlayerAttackManager;

    // 現在の状態
    Player.PlayerState s_PlayerState;

    // 攻撃判定を出すか
    bool b_IsCollision;

    // ボス
    BossBaseManager g_Boss;

    // 攻撃判定サイズ
    const float ATTACK_SCALE_X = 5.2f;
    const float ATTACK_SCALE_Y = 1f;

    // 攻撃ダメージ
    const float ATTACK_DAMAGE = 7f;

    public override void Start()
    {
        base.Start();

        // 必要なスクリプト取得
        c_PlayerInputManager = GetComponent<PlayerInputManager>();
        c_PlayerMoveManager = GetComponent<PlayerMoveManager>();
        c_PlayerAttackManager = GetComponent<PlayerAttackManager>();

        // プレイヤーとして設定
        e_CharaState = CharaState.Player;

        // 初期向き
        CurrentDirection = 1;

        // 初期状態
        s_PlayerState = Player.PlayerState.Idle;

        // 攻撃判定OFF
        b_IsCollision = false;

        // 1フレーム後にボス取得
        NextFrame.Run(this, 1, () =>
        {
            var bossObject = SaveManager.Instance.c_CurrentData.GetCharacter(CharaState.Boss);

            if (bossObject != null)
            {
                g_Boss = bossObject.GetComponent<BossBaseManager>();
            }
        });
    }

    public override void Update()
    {
        // 必須参照が無ければ処理しない
        if (c_PlayerInputManager == null) return;
        if (c_PlayerMoveManager == null) return;
        if (c_PlayerAttackManager == null) return;
        if (TatuGameManager.Instance == null) return;
        if (BattleManager.Instance == null) return;

        // チュートリアル中・ロード中は動かさない
        if (!TatuGameManager.Instance.b_IsTutorial && !BattleManager.Instance.b_IsLoading)
        {
            StateUpdate();
        }
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

            case Player.PlayerState.Jump:
                JumpUpdate();
                break;

            case Player.PlayerState.Attack:
                AttackUpdate();
                break;

            case Player.PlayerState.Damage:
                break;

            case Player.PlayerState.Die:
                break;
        }
    }

    // 待機状態更新
    void IdleUpdate()
    {
        c_PlayerMoveManager.IdleUpdate();

        // 攻撃入力
        if (c_PlayerAttackManager.HasAttackInput())
        {
            s_PlayerState = Player.PlayerState.Attack;
            c_PlayerAttackManager.AttackEnter();
            return;
        }

        // ジャンプ入力
        if (c_PlayerInputManager.GetJump())
        {
            s_PlayerState = Player.PlayerState.Jump;
            c_PlayerMoveManager.JumpEnter();
            return;
        }

        float horizontal = c_PlayerInputManager.GetHorizontal();
        float vertical = c_PlayerInputManager.GetVertical();

        // 移動入力があればMoveへ
        if (horizontal != 0f || vertical != 0f)
        {
            s_PlayerState = Player.PlayerState.Move;
        }
    }

    // 移動状態更新
    void MoveUpdate()
    {
        // 攻撃入力
        if (c_PlayerAttackManager.HasAttackInput())
        {
            s_PlayerState = Player.PlayerState.Attack;
            c_PlayerAttackManager.AttackEnter();
            return;
        }

        // ジャンプ入力
        if (c_PlayerInputManager.GetJump())
        {
            s_PlayerState = Player.PlayerState.Jump;
            c_PlayerMoveManager.JumpEnter();
            return;
        }

        c_PlayerMoveManager.MoveUpdate();

        float horizontal = c_PlayerInputManager.GetHorizontal();
        float vertical = c_PlayerInputManager.GetVertical();

        // 入力が無ければIdleへ
        if (horizontal == 0f && vertical == 0f)
        {
            s_PlayerState = Player.PlayerState.Idle;
        }
    }

    // ジャンプ状態更新
    void JumpUpdate()
    {
        c_PlayerMoveManager.MoveUpdate();

        // 着地したらMoveかIdleへ戻る
        if (c_PlayerMoveManager.GetIsGround())
        {
            float horizontal = c_PlayerInputManager.GetHorizontal();
            float vertical = c_PlayerInputManager.GetVertical();

            if (horizontal != 0f || vertical != 0f)
            {
                s_PlayerState = Player.PlayerState.Move;
            }
            else
            {
                s_PlayerState = Player.PlayerState.Idle;
            }
        }
    }

    // 攻撃状態更新
    void AttackUpdate()
    {
        c_PlayerAttackManager.AttackUpdate();

        // 攻撃判定が有効ならボスに当たり判定を送る
        if (b_IsCollision && g_Boss != null)
        {
            g_Boss.CheckCollisionBox(
                ATTACK_SCALE_X,
                ATTACK_SCALE_Y,
                transform.position,
                g_Boss.transform.position,
                ATTACK_DAMAGE
            );
        }

        // 攻撃終了後
        if (!GetIsAttackFlag())
        {
            SetCollider(0);

            float horizontal = c_PlayerInputManager.GetHorizontal();
            float vertical = c_PlayerInputManager.GetVertical();

            if (horizontal != 0f || vertical != 0f)
            {
                s_PlayerState = Player.PlayerState.Move;
            }
            else
            {
                s_PlayerState = Player.PlayerState.Idle;
            }
        }
    }

    // 攻撃判定ON/OFF
    // 0 = false
    // 1 = true
    public void SetCollider(int isTrue)
    {
        b_IsCollision = isTrue == 1;
    }
}