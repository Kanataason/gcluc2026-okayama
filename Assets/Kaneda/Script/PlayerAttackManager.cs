using UnityEngine;

// プレイヤー攻撃管理クラス
// 攻撃処理のみを担当する
public class PlayerAttackManager : CharaBase
{
    // 入力管理クラス
    PlayerInputManager c_PlayerInputManager;

    // 攻撃時間
    const float ATTACK_TIME = 0.35f;

    // 攻撃タイマー
    float f_AttackTimer;

    // 攻撃ステート名
    const string ATTACK_STATE_NAME = "Attack";

    public override void Start()
    {
        base.Start();

        c_PlayerInputManager = GetComponent<PlayerInputManager>();
    }

    // 攻撃開始
    public void AttackEnter()
    {
        if (GetIsAttackFlag()) return;

        SetIsAttackFlag(true);
        f_AttackTimer = ATTACK_TIME;

        if (a_Animator != null)
        {
            a_Animator.Play(ATTACK_STATE_NAME, 0, 0f);
        }
    }

    // 攻撃更新
    public void AttackUpdate()
    {
        if (!GetIsAttackFlag()) return;

        f_AttackTimer -= Time.deltaTime;

        if (f_AttackTimer <= 0f)
        {
            f_AttackTimer = 0f;
            SetIsAttackFlag(false);
        }
    }

    // 攻撃入力確認
    public bool HasAttackInput()
    {
        return c_PlayerInputManager.GetAttack();
    }
}